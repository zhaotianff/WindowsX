using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using WindowsX.Shell.Services.Application;
using WindowsX.Shell.Services.Application.Contracts;

namespace WindowsX.Shell.View.Setting.Pages
{
    public partial class SystemSetting : Page
    {
        private readonly IApplicationStartupService startupService;
        private bool updating = true;
        private bool lastEnabled;

        public SystemSetting() : this(new ApplicationStartupService())
        {
        }

        public SystemSetting(IApplicationStartupService startupService)
        {
            this.startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            updating = true;
            try
            {
                lastEnabled = startupService.IsEnabled();
                check_AutoStart.IsChecked = lastEnabled;
                check_AutoStart.IsEnabled = true;
                text_Error.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) when (IsStartupError(ex))
            {
                check_AutoStart.IsEnabled = false;
                ShowError("无法读取自启动状态", ex);
            }
            finally { updating = false; }
        }

        private void AutoStart_Changed(object sender, RoutedEventArgs e)
        {
            if (updating)
                return;

            try
            {
                var enabled = check_AutoStart.IsChecked == true;
                startupService.SetEnabled(enabled);
                lastEnabled = enabled;
                text_Error.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) when (IsStartupError(ex))
            {
                updating = true;
                try { check_AutoStart.IsChecked = lastEnabled; }
                finally { updating = false; }
                ShowError("无法修改自启动设置", ex);
            }
        }

        private static bool IsStartupError(Exception ex)
        {
            return ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException;
        }

        private void ShowError(string message, Exception ex)
        {
            text_Error.Text = message + "：" + ex.Message;
            text_Error.Visibility = Visibility.Visible;
        }
    }
}
