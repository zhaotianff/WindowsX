using System;
using System.Collections.Generic;
using System.IO;
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
using WindowsX.Shell.Services.Beautify;
using WindowsX.Shell.Services.Beautify.Contract;
using WindowsX.Shell.Util;

namespace WindowsX.Shell.View.Beautify.Pages
{
    /// <summary>
    /// DesktopEffect.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopEffect : Page
    {
        private IDesktopService desktopService;

        public DesktopEffect()
        {
            InitializeComponent();

            desktopService = DesktopService.Instance;
        }

        public void Init()
        {
            var path = desktopService.GetCurrentWallpaperPath();
            this.img_background.Source = ImageHelper.GetBitmapImageFromLocalFile(path, true);
        }
    }
}
