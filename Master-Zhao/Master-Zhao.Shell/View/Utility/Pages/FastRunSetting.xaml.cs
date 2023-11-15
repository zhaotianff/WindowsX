//#define FASTRUN_PREVIOUS

using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using Master_Zhao.Shell.View.Utility.Windows;
using Master_Zhao.Shell.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// FastRunSetting.xaml 的交互逻辑
    /// </summary>
    public partial class FastRunSetting : Page
    {
        private ObservableCollection<FastRunItem> fastRunItemList = new ObservableCollection<FastRunItem>();

#if FASTRUN_PREVIOUS
        FastRun fastRun;
#else
        FastRunNew fastRunNew;
#endif

        public FastRunSetting()
        {
            InitializeComponent();
        }

        public void InitFastRun()
        {
            InitConfig();
            InitUi();
        }

        private void InitUi()
        {
#if FASTRUN_PREVIOUS
            if (fastRun == null)
            {
                fastRun = new FastRun();
                fastRun.Visibility = Visibility.Visible;
                fastRun.Visibility = Visibility.Hidden;
            }
#else
            if (fastRunNew == null)
            {
                fastRunNew = new FastRunNew();
                fastRunNew.Visibility = Visibility.Visible;
                fastRunNew.Visibility = Visibility.Hidden;
                fastRunNew.LoadFastRunList(fastRunItemList);
                this.lst_FastRunItems.ItemsSource = fastRunItemList;
            }
#endif
        }

        private void InitConfig()
        {
            var list = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;
            var angle = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var fastRunConfigItem = list[i];
                FastRunItem fastRunItem = new FastRunItem();
                fastRunItem.Angle = angle;
                fastRunItem.DisplayName = fastRunConfigItem.Name;
                fastRunItem.Icon = ImageHelper.GetBitmapImageFromLocalFile(GetCachedIconPath(list[i].Path));
                fastRunItem.Args = fastRunConfigItem.Args;
                fastRunItem.HotKey = fastRunConfigItem.HotKey;
                fastRunItem.Path = fastRunConfigItem.Path;
                angle += 45;
                fastRunItemList.Add(fastRunItem);
            }
        }

        public void CloseFastRun()
        {
#if FASTRUN_PREVIOUS
            fastRun?.Close();
#else
            fastRunNew?.Close();
#endif
        }

        private void cbxFastrun_Checked(object sender, RoutedEventArgs e)
        {
#if FASTRUN_PREVIOUS
            fastRun.RegisterHotKey();
#else
            fastRunNew.RegisterHotKey();
#endif
        }

        private void cbxFastrun_Unchecked(object sender, RoutedEventArgs e)
        {
#if FASTRUN_PREVIOUS
            fastRun.UnregisterHotKey();
#else
            fastRunNew.UnregisterHotKey();
#endif
        }

        private string GetCachedIconPath(string path)
        {
            var temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Master-Zhao");
            var iconPath = System.IO.Path.Combine(temp, System.IO.Path.GetFileNameWithoutExtension(path) + ".png");
            if (System.IO.Directory.Exists(temp) == false)
                System.IO.Directory.CreateDirectory(temp);

            if (System.IO.File.Exists(iconPath) == false)
            {
                IntPtr hIcon = IntPtr.Zero;
                if (IconTool.ExtractFirstIconFromFile(path, true, ref hIcon))
                {
                    var bi = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    ImageHelper.SaveBitmapImageToFile(bi, iconPath);
                }
            }

            return iconPath;
        }

        private void btnBrowerPath_Click(object sender, RoutedEventArgs e)
        {
            var item = TreeHelper.FindParent<ListBoxItem>(sender as DependencyObject);
            var fastRunItem = item.Content as FastRunItem;
            var index = this.lst_FastRunItems.Items.IndexOf(fastRunItem);
            var path = DialogHelper.BrowserSingleFile("可执行程序|*.exe;*.cpl;*.msc;*.bat;*.vbs;*.lnk", initPath: Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));

            if (!string.IsNullOrEmpty(path))
            {
                fastRunItem.Path = path;
                fastRunItem.DisplayName = System.IO.Path.GetFileNameWithoutExtension(path);
                fastRunItem.Icon = ImageHelper.GetBitmapImageFromLocalFile(GetCachedIconPath(path));

                var fastRunSettingList = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;
                FastRunConfigItem updateFastRunItem;
                if(index < fastRunSettingList.Count)
                {
                    updateFastRunItem = fastRunSettingList[index];
                }
                else
                {
                    updateFastRunItem = new FastRunConfigItem();
                    fastRunSettingList.Add(updateFastRunItem);
                }

                updateFastRunItem.Name = fastRunItem.DisplayName;
                updateFastRunItem.Path = fastRunItem.Path;
                updateFastRunItem.RunType = FastRunType.Applicataion;
            }
        }

        private void btn_AddFastRunItem_Click(object sender, RoutedEventArgs e)
        {
            if(fastRunItemList.Count == 8)
            {
                MessageBox.Show("当前仅支持添加8个菜单项");
                return;
            }

            FastRunItem fastRunItem = new FastRunItem();
            fastRunItem.Angle = fastRunItemList.Count * 45;
            fastRunItem.DisplayName = "";
            fastRunItem.Path = "";
            fastRunItemList.Add(fastRunItem);
        }

        private void btn_RemoveFastRunItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.lst_FastRunItems.SelectedIndex == -1)
                return;

            var index = this.lst_FastRunItems.SelectedIndex;

            fastRunItemList.RemoveAt(index);
            GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList.RemoveAt(index);

            var angle = 0;
            for (int i = 0; i < fastRunItemList.Count; i++)
            {
                fastRunItemList[i].Angle = angle;
                angle += 45;
            }
            fastRunNew.LoadFastRunList(fastRunItemList);
        }
    }
}
