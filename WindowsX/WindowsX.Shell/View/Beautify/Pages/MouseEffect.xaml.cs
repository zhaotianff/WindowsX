using WindowsX.Shell.View.Beautify.UserControls;
using WindowsX.Shell.Windows;
using WindowsX.Config;
using WindowsX.Config.Model;
using WindowsX.Config.Helper;
using WindowsX.Shell.Infrastructure.MouseEffect;
using System.IO;
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
    /// MouseEffect.xaml 的交互逻辑
    /// </summary>
    public partial class MouseEffect : Page
    {
        private readonly MainConfig mainConfig;
        private bool updatingState = true;
        MouseEffectWindow mouseEffectWindow;

        public MouseEffect() : this(GlobalConfig.Instance.MainConfig ?? new MainConfig())
        {
        }

        public MouseEffect(MainConfig mainConfig)
        {
            this.mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
            InitializeComponent();
            updatingState = false;
        }

        private void mouseEffect_Checked(object sender, RoutedEventArgs e)
        {
            SetEnabled(true);
        }

        private void mouseEffect_Unchecked(object sender, RoutedEventArgs e)
        {
            SetEnabled(false);
        }

        public void RestoreMouseEffect()
        {
            SynchronizeCheckBox();
            if (mainConfig.MouseEffectSetting.IsEnabled)
                ShowMouseEffectWindow();
            else
                CloseMouseEffectWindow();
        }

        private void SynchronizeCheckBox()
        {
            updatingState = true;
            try { cbx_EnableMouseEffect.IsChecked = mainConfig.MouseEffectSetting.IsEnabled; }
            finally { updatingState = false; }
        }

        private void SetEnabled(bool enabled)
        {
            if (updatingState)
                return;

            if (!SaveState(enabled, GetSelectedEffectType()))
            {
                SynchronizeCheckBox();
                return;
            }

            if (enabled)
                ShowMouseEffectWindow();
            else
                CloseMouseEffectWindow();
        }

        private MouseEffectType GetSelectedEffectType()
        {
            var type = (MouseEffectType)mainConfig.MouseEffectSetting.SelectedEffectType;
            return Enum.IsDefined(typeof(MouseEffectType), type) ? type : MouseEffectType.FollowingDot;
        }

        private void ShowMouseEffectWindow()
        {
            if (mouseEffectWindow != null)
                return;

            mouseEffectWindow = new MouseEffectWindow(mainConfig.MouseEffectSetting);
            mouseEffectWindow.UpdateMouseEffectType(GetSelectedEffectType());
            mouseEffectWindow.Show();
        }

        private bool SaveState(bool enabled, MouseEffectType type)
        {
            var settings = mainConfig.MouseEffectSetting;
            var previousEnabled = settings.IsEnabled;
            var previousType = settings.SelectedEffectType;
            settings.IsEnabled = enabled;
            settings.SelectedEffectType = (int)type;
            bool saved;
            try
            {
                saved = ConfigHelper.SaveMainConfig(mainConfig);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                saved = false;
            }

            if (saved)
                return true;

            settings.IsEnabled = previousEnabled;
            settings.SelectedEffectType = previousType;
            MessageBox.Show("无法保存鼠标特效状态，请检查配置目录的写入权限后重试。", "保存失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        public void CloseMouseEffectWindow()
        {
            if (mouseEffectWindow == null)
                return;

            mouseEffectWindow.StopMouseEffect();
            mouseEffectWindow?.Close();
            mouseEffectWindow = null;
        }

        private void MouseEffectItemControl_OnSet(object sender, RoutedEventArgs e)
        {
            if (mouseEffectWindow == null || !(sender is MouseEffectItemControl mouseEffectControl))
                return;

            if (SaveState(true, mouseEffectControl.MouseEffectType))
                mouseEffectWindow.UpdateMouseEffectType(mouseEffectControl.MouseEffectType);
        }

        private void MouseEffectItemControl_OnOpenSetting(object sender, RoutedEventArgs e)
        {
            var mouseEffectControl = (MouseEffectItemControl)sender;
            var settingWindow = new MouseEffectSettingWindow(mouseEffectControl.MouseEffectType, mainConfig)
            {
                Owner = Window.GetWindow(this)
            };
            settingWindow.ShowDialog();
        }
    }
}
