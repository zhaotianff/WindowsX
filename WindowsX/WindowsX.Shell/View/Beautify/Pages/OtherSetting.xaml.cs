using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsX.Shell.Pages
{
    /// <summary>
    /// Interaction logic for OtherSetting.xaml
    /// </summary>
    public partial class OtherSetting : Page
    {
        public OtherSetting()
        {
            InitializeComponent();
            LoadDesktopIconState();
            LoadDesktopSettings();
        }

        private void LoadDesktopIconState()
        {
            cbx_Computer.IsChecked =  DesktopTool.GetDesktopIconState(DesktopTool.DESKTOPICONS.ICON_COMPUTER);
            cbx_User.IsChecked = DesktopTool.GetDesktopIconState(DesktopTool.DESKTOPICONS.ICON_USER);
            cbx_Recycle.IsChecked = DesktopTool.GetDesktopIconState(DesktopTool.DESKTOPICONS.ICON_RECYCLE);
            cbx_ControlPanel.IsChecked = DesktopTool.GetDesktopIconState(DesktopTool.DESKTOPICONS.ICON_CONTROL_PANEL);
            cbx_Network.IsChecked = DesktopTool.GetDesktopIconState(DesktopTool.DESKTOPICONS.ICON_NETWORK);
            cbx_GodMode.IsChecked = DesktopTool.GetGodModeShortCutState();

            cbx_Computer.Checked += (sender,e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_COMPUTER, true); };
            cbx_User.Checked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_USER, true); };
            cbx_Recycle.Checked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_RECYCLE, true); };
            cbx_ControlPanel.Checked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_CONTROL_PANEL, true); };
            cbx_Network.Checked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_NETWORK, true); };
            cbx_GodMode.Checked += (sender, e) => { DesktopTool.CreateGodModeShortCut(); ; };

            cbx_Computer.Unchecked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_COMPUTER, false); };
            cbx_User.Unchecked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_USER, false); };
            cbx_Recycle.Unchecked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_RECYCLE, false); };
            cbx_ControlPanel.Unchecked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_CONTROL_PANEL, false); };
            cbx_Network.Unchecked += (sender, e) => { DesktopTool.SetDesktopIcon(DesktopTool.DESKTOPICONS.ICON_NETWORK, false); };
            cbx_GodMode.Unchecked += (sender, e) => { DesktopTool.RemoveGodModeShortCut(); };
        }

        private void LoadDesktopSettings()
        {
            //cbx_Shortcut.IsChecked = DesktopTool.GetShortcutArrowState();
            cbx_PhotoViewer.IsChecked = DesktopTool.GetWindowsPhotoViewerState();
            cbx_Version.IsChecked = DesktopTool.GetPaintVersionState();
            var size = 0;
            if( DesktopTool.GetTaskbarThumbnailSize(ref size))
            {
                tbox_TaskbarThunbSize.Text = size.ToString();
            }
            else
            {
                tbox_TaskbarThunbSize.Text = "系统默认";
            }

            //cbx_Shortcut.Checked += (sender, e) => { DesktopTool.RemoveShortcutArrow(); };
            //cbx_Shortcut.Unchecked += (sender, e) => { DesktopTool.RestoreShortcutArrow(); };
            cbx_PhotoViewer.Checked += (sender, e) => { RegisterWindowsPhotoViewer(); };
            cbx_PhotoViewer.Unchecked += (sender, e) => { UnRegisterWindowsPhotoViewer(); };
            cbx_Version.Checked += (sender, e) => { DesktopTool.PaintVersionInfo(true); };
            cbx_Version.Unchecked += (sender, e) => { DesktopTool.PaintVersionInfo(false); };
        }

        private async void RegisterWindowsPhotoViewer()
        {
            //temp
            var adminTaskExe = Environment.CurrentDirectory + "\\WindowsX.AdminTask.exe";
            if(System.IO.File.Exists(adminTaskExe))
            {
                var process = Util.ProcessHelper.Execute(adminTaskExe, new string[] { "register" });

                if(process != null)
                {
                    await Task.Delay(1000);
                    process.Kill();
                }
            }
        }

        private async void UnRegisterWindowsPhotoViewer()
        {
            //temp
            var adminTaskExe = Environment.CurrentDirectory + "\\WindowsX.AdminTask.exe";
            if (System.IO.File.Exists(adminTaskExe))
            {
                var process = Util.ProcessHelper.Execute(adminTaskExe, new string[] { "unregister" });

                if (process != null)
                {
                    await Task.Delay(100);
                    process.Kill();
                }
            }
        }

        private void btnSetTaskbarThumbSize_Click(object sender, RoutedEventArgs e)
        {
            var sizeStr = tbox_TaskbarThunbSize.Text;
            var restart = false;

            if (string.IsNullOrEmpty(sizeStr))
            {
                MessageBox.Show("请输入大小");
                return;
            }

            if(int.TryParse(sizeStr, out int size))
            {
                
                if(MessageBox.Show("是否立即生效","提示信息",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    restart = true;
                }
            }

            DesktopTool.SetTaskbarThumbnailSize(size, restart);
        }

        private void btnResetTaskbarThumbSize_Click(object sender, RoutedEventArgs e)
        {
            var restart = false;
            if (MessageBox.Show("是否立即生效", "提示信息", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                restart = true;
            }

            DesktopTool.SetTaskbarThumbnailSize(DesktopTool.DEFAULT_TASKBAR_THUMBNAIL_SIZE, restart);
        }
    }
}
