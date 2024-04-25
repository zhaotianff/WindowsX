using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsX.Shell.PInvoke;

namespace WindowsX.Shell.View.Beautify.Pages
{
    /// <summary>
    /// ExplorerSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ExplorerSetting : Page
    {
        private static readonly string ShellHookLibraryName = "WindowsXCoreShellHook.dll";
        private static readonly string ShellHookLibraryPath = Environment.CurrentDirectory + "\\" + ShellHookLibraryName;
        private static readonly string DefaultExplorerBgImagePath = Environment.CurrentDirectory + "\\res\\explorerbg.jpg";

        private bool isPatched = false;
        private string currentSelectedImagePath;

        public ExplorerSetting()
        {
            InitializeComponent();

            LoadExplorerSetting();
        }

        public void LoadExplorerSetting()
        {
            LoadBackgroundImageSetting();
        }

        private void LoadBackgroundImageSetting()
        {
            if (SystemTool.IsProcessContainModule("explorer.exe", ShellHookLibraryName) == true)
            {
                this.cbx_EnableBackground.IsChecked = true;
                LoadPresetExplorerBg();
                isPatched = true;
            }
            else
            {
                this.cbx_EnableBackground.IsChecked = false;
                LoadDefaultExplorerBg();
                isPatched = false;
            }
        }

        private void btnBrowseBgImage_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = DialogHelper.BrowserSingleFile("图片文件|*.jpg;*.jpeg", "浏览背景图片", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

            if (!string.IsNullOrEmpty(imgPath))
            {
                this.img_bg.Source = ImageHelper.GetBitmapImageFromLocalFile(imgPath);
                currentSelectedImagePath = imgPath;
            }
        }

        private void btnApplyBgImage_Click(object sender, RoutedEventArgs e)
        {
            if (isPatched == true)
            {
                if (string.IsNullOrEmpty(currentSelectedImagePath))
                {
                    return;
                }
                else
                {
                    btnResetBgImage_Click(null, null);
                }
            }

            CopyImageFileToLocalAndReset();

            var pid = ProcessHelper.GetExplorerProcessId();
            var result = SystemTool.CreateRemoteThreadInject((uint)pid, ShellHookLibraryPath);
            isPatched = result;

            if (result)
                ProcessHelper.Execute("explorer.exe");
        }

        private void CopyImageFileToLocalAndReset()
        {
            if (!string.IsNullOrEmpty(currentSelectedImagePath))
            {
                var destFileName = FileHelper.CopyFileToCurrentExecutablePath(currentSelectedImagePath, "res");
                WriteImageInfoToFile(destFileName, slider_Opacity.Value, this.combox_StretchMode.SelectedIndex);
                currentSelectedImagePath = "";
            }
        }

        private void WriteImageInfoToFile(string filePath,double opacity,int stretch)
        {
            double opacityRatio = 0.003921568627451d;
            int nOpacity = (int)(opacity / opacityRatio);

            var buffer = System.IO.File.ReadAllBytes(filePath);

            if (buffer[0] == 0xFF && buffer[1] == 0xD8)
            {
                var newBuffer = new byte[buffer.Length + 5];
                Array.Copy(buffer, 0, newBuffer, 0, 2);
                newBuffer[2] = 0xFF;
                newBuffer[3] = 0x00;
                newBuffer[4] = 0x02;
                newBuffer[5] = (byte)nOpacity;
                newBuffer[6] = (byte)stretch;
                Array.Copy(buffer, 2, newBuffer, 7, buffer.Length - 2);
                System.IO.File.WriteAllBytes(filePath, newBuffer);
            }  
        }

        private void btnResetBgImage_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("恢复默认时会关闭所有资源管理器窗口，是否确认？","提示信息",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DesktopTool.RestartExplorer();
                LoadDefaultExplorerBg();
                isPatched = false;
                cbx_EnableBackground.IsChecked = false;
            }
        }

        private void LoadDefaultExplorerBg()
        {
            this.img_bg.Source = ImageHelper.GetBitmapImageFromResource("/Icon/white.png");
        }

        private void LoadPresetExplorerBg()
        {
            if (System.IO.File.Exists(DefaultExplorerBgImagePath) == false)
            {
                Properties.Resources.explorerbg.Save(DefaultExplorerBgImagePath);
            }

            this.img_bg.Source = ImageHelper.GetBitmapImageFromLocalFile(DefaultExplorerBgImagePath, true);
        }

        private void cbx_EnableBackground_Checked(object sender, RoutedEventArgs e)
        {
            LoadPresetExplorerBg();
        }

        private void cbx_EnableBackground_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadDefaultExplorerBg();
        }
    }
}
