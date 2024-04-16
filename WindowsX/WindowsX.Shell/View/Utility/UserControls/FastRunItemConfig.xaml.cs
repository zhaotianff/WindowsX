using WindowsX.Config.Model;
using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WindowsX.Shell.UserControls
{
    /// <summary>
    /// Interaction logic for FastRunItemConfig.xaml
    /// </summary>
    public partial class FastRunItemConfig : UserControl,INotifyPropertyChanged
    {
        private FastRunConfigItem currentItem;
        private string fastRunName = "";
        private string titleText;

        public string FastRunName
        {
            get
            {
                return fastRunName;
            }

            set
            {
                fastRunName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FastRunName"));
            }
        }

        public string TitleText 
        {
            get => this.lbl_Title.Content.ToString();
            set
            {
                titleText = value;
                lbl_Title.Content = value;
            }
        }


        public string FastRunPath
        {
            get
            {
                return tbox_Path.Text;
            }
            set
            {
                tbox_Path.Text = value;
                FastRunName = System.Diagnostics.FileVersionInfo.GetVersionInfo(value).FileDescription;
            }
        }


        public Action<FastRunConfigItem> OnFastRunItemConfigChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public FastRunItemConfig()
        {
            InitializeComponent();
        }

        private void btnBrowerPath_Click(object sender, RoutedEventArgs e)
        {
            var path = DialogHelper.BrowserSingleFile("可执行程序|*.exe;*.cpl;*.msc;*.bat;*.vbs;*.lnk",initPath: Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));

            if(!string.IsNullOrEmpty(path))
            {
                tbox_Path.Text = path;

                var fastRunItem = new FastRunConfigItem();
                fastRunItem.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                FastRunName = fastRunItem.Name;
                fastRunItem.Path = path;
                fastRunItem.RunType = FastRunType.Applicataion;
                currentItem = fastRunItem;
                OnFastRunItemConfigChanged?.Invoke(fastRunItem);
            }
        }
    }
}
