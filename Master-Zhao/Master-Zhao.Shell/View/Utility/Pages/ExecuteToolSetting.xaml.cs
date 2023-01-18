using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.Util;
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

namespace Master_Zhao.Shell.View.Utility.Pages
{
    /// <summary>
    /// ExecuteToolSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ExecuteToolSetting : Page
    {
        private ExecuteItem currentSelectedItem;

        public ExecuteToolSetting()
        {
            InitializeComponent();
        }

        public async Task LoadExecuteListAsync()
        {
            var fullList = await GetAllExecuteItemInEnvironmentVariable();
            list_exe.ItemsSource = fullList.Where(x => x.Name.ToLower().EndsWith(".exe"));
        }

        private async Task<List<ExecuteItem>> GetAllExecuteItemInEnvironmentVariable()
        {
            var list = new List<ExecuteItem>();

            try
            {
                await Task.Run(() => {
                    var path = Environment.GetEnvironmentVariable("Path");
                    var pathText = Environment.GetEnvironmentVariable("PathExt");

                    var pathArray = path.Split(';');
                    var pathTextArray = pathText.Split(';');
                    pathTextArray = pathTextArray.ToList().Select(x => x.ToLower()).ToArray();

                    if (pathTextArray.Contains(".cpl") == false)
                    {
                        pathTextArray = pathTextArray.Append(".cpl").ToArray();
                    }

                    foreach (var item in pathArray)
                    {
                        if (System.IO.Directory.Exists(item) == false)
                            continue;

                        var files = DirectoryExtension.GetFiles(item, pathTextArray);
                        var runItems = files.Select(x => new ExecuteItem() { Name = System.IO.Path.GetFileName(x), Path = x, Description = FileExtension.GetFileDescription(x) });
                        list.AddRange(runItems);
                    }
                });

                return list;
            }
            catch
            {
                return list;
            }
        }

        private void list_exe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems[0] as ExecuteItem;
            this.lbl_description.Content = selectedItem.Description;
            this.lbl_path.Content = selectedItem.Path;
            currentSelectedItem = selectedItem;
        }

        private void btn_execute_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem == null)
                return;

            ProcessHelper.Execute(currentSelectedItem.Path);
        }
    }
}
