using Master_Zhao.Shell.Util;
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

namespace Master_Zhao.Shell.View.Beautify.Pages
{
    /// <summary>
    /// ExplorerSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ExplorerSetting : Page
    {
        public ExplorerSetting()
        {
            InitializeComponent();
        }

        private void btnBrowseBgImage_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = DialogHelper.BrowserSingleFile("图片文件|*.jpg;*.png;*.bmp;*.jpeg", "浏览背景图片", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

            if (!string.IsNullOrEmpty(imgPath))
                this.img_bg.Source = ImageHelper.GetBitmapImageFromLocalFile(imgPath);
        }
    }
}
