using Master_Zhao.Shell.Infrastructure.DesignPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Util
{
    public class NotifyIconCreateData
    {
        public string Tooltip { get; set; }
        public string IconRelativePath { get; set; }
        public System.Windows.Controls.ContextMenu ContextMenu { get; set; }
        public System.Windows.Forms.MouseEventHandler ClickHandler { get; set; }
    }

    public class NotifyIconHelper : SingletonBase<NotifyIconHelper>
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Controls.ContextMenu contextMenu;
        private System.Windows.Forms.MouseEventHandler clickHander;

        public void SetNotifyIconState(bool isShow)
        {
            notifyIcon.Visible = isShow;
        }

        public void ShowBallonTipText(string tipTitle,string tipText,int timeMillonSeconds)
        {
            if (notifyIcon == null)
                return;

            notifyIcon.BalloonTipTitle = tipTitle;
            notifyIcon.BalloonTipText = tipText;
            notifyIcon.ShowBalloonTip(timeMillonSeconds);
        }

        public void CreateNotifyIcon(NotifyIconCreateData data)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = data.Tooltip;
            notifyIcon.Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri(data.IconRelativePath,UriKind.Relative)).Stream);
            notifyIcon.Visible = false;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;

            contextMenu = data.ContextMenu;
            contextMenu.LostFocus += ContextMenu_LostFocus;  //TODO fix this
        }

        private void ContextMenu_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var menu = sender as System.Windows.Controls.ContextMenu;
            if (menu != null)
                menu.IsOpen = false;
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if(contextMenu != null)
                {
                    contextMenu.IsOpen = true;
                    App.Current.MainWindow.Activate();
                }
            }

            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                clickHander?.Invoke(sender, e);
            }
        }
    }
}
