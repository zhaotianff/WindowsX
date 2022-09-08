using Master_Zhao.Shell.PInvoke;
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

namespace Master_Zhao.Shell.Pages
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
        }

        private void LoadDesktopSettings()
        {
            cbx_Shortcut.IsChecked = DesktopTool.GetShortcutArrowState();
            cbx_PhotoViewer.IsChecked = true;
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
        }

        private void cbx_Shortcut_UnChecked(object sender, RoutedEventArgs e)
        {
            DesktopTool.RestoreShortcutArrow();
        }

        private void cbx_Shortcut_Checked(object sender, RoutedEventArgs e)
        {
            DesktopTool.RemoveShortcutArrow();
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
