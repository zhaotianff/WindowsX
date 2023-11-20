using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private bool isLoaded = false;

        public ExecuteToolSetting()
        {
            InitializeComponent();
        }

        public async Task LoadExecuteListAsync()
        {
            if (isLoaded == true)
                return;

            var fullList = await GetAllExecuteItemInEnvironmentVariable();
            //list_installed.ItemsSource = LoadInstalledExecuteItems();
            list_exe.ItemsSource = fullList.Where(x => x.Name.ToLower().EndsWith(".exe")).SetItemType(ExecuteItemType.EXE);
            list_mmc.ItemsSource = fullList.Where(x => x.Name.ToLower().EndsWith(".msc")).SetItemType(ExecuteItemType.MMC);
            list_control.ItemsSource = fullList.Where(x => x.Name.ToLower().EndsWith(".cpl")).SetItemType(ExecuteItemType.CPL);
            list_dll.ItemsSource = LoadDllExecuteItemsFromResource().SetItemType(ExecuteItemType.DLL);
            list_mssetting.ItemsSource = LoadMsSettingItemsFromResource().SetItemType(ExecuteItemType.MSSETTING);
            list_shellfolder.ItemsSource = LoadShellShotcutItemsFromResource().SetItemType(ExecuteItemType.SHELL);
            isLoaded = true;
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
            if (e.AddedItems.Count == 0)
                return;

            var selectedItem = e.AddedItems[0] as ExecuteItem;
            this.lbl_description.Content = selectedItem.Description;
            this.lbl_path.Content = selectedItem.Path;
            currentSelectedItem = selectedItem;
        }

        private void btn_execute_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem == null)
                return;

            var path = currentSelectedItem.Path;
            string[] args = null;

            switch (currentSelectedItem.ItemType)
            {
                case ExecuteItemType.DLL:
                    var pathArray = path.Split(' ');
                    path = pathArray[0];
                    args = pathArray.Skip(1).ToArray();
                    break;
            }
            
            if(args != null && args.Length > 0)
                ProcessHelper.Execute(path,args);
            else
                ProcessHelper.Execute(path);
        }

        private void btn_openrun_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem == null)
                return;

            var name = currentSelectedItem.Name;

            switch (currentSelectedItem.ItemType)
            {
                case ExecuteItemType.EXE:
                    name = System.IO.Path.GetFileNameWithoutExtension(currentSelectedItem.Name);
                    break;
                case ExecuteItemType.DLL:
                    name = currentSelectedItem.Path;
                    break;
                default:
                    break;
            }

            PInvoke.DesktopTool.OpenRunDialog(name);
        }

        private void menu_LocateFile_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem == null)
                return;

            if (System.IO.File.Exists(currentSelectedItem.Path) == false)
                return;

            PInvoke.DesktopTool.SelectFile(currentSelectedItem.Path);
        }

        private void menu_SearchFile_Click(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem == null)
                return;

            if (System.IO.File.Exists(currentSelectedItem.Path) == false)
                return;

            ProcessHelper.Execute("https://cn.bing.com/search?q=" + currentSelectedItem.Name);
        }

        private List<ExecuteItem> LoadDllExecuteItemsFromResource()
        {
            var list = new List<ExecuteItem>();
            var listStr = new List<string>();

            using (StringReader sr = new StringReader(Properties.Resources.rundll32))
            {
                var str = sr.ReadLine();

                while (!string.IsNullOrEmpty(str))
                {
                    listStr.Add(str);
                    str = sr.ReadLine();
                }
            }

            for (int i = 0; i < listStr.Count; i += 3)
            {
                ExecuteItem executeItem = new ExecuteItem();
                executeItem.Description = listStr[i + 1];
                executeItem.Path = listStr[i + 2];
                executeItem.Name = executeItem.Path.Substring(13);
                list.Add(executeItem);
            }
            return list;
        }

        private List<ExecuteItem> LoadMsSettingItemsFromResource()
        {
            var list = new List<ExecuteItem>();
            var listStr = new List<string>();

            using (StringReader sr = new StringReader(Properties.Resources.mssetting))
            {
                var str = sr.ReadLine();

                while (!string.IsNullOrEmpty(str))
                {
                    listStr.Add(str);
                    str = sr.ReadLine();
                }
            }

            for (int i = 0; i < listStr.Count; i += 3)
            {
                ExecuteItem executeItem = new ExecuteItem();
                executeItem.Name = listStr[i];
                executeItem.Path = listStr[i + 1];
                executeItem.Description = listStr[i + 2];
                list.Add(executeItem);
            }
            return list;
        }

        private List<ExecuteItem> LoadInstalledExecuteItems()
        {
            var list = new List<ExecuteItem>();

            uint size = 2048;
            IntPtr ptr = Marshal.AllocHGlobal((int)size);

            var result = PInvoke.AppTool.GetAppPath(ptr, size);

            if (result == false)
                return list;

            var str = Marshal.PtrToStringAuto(ptr);
            Marshal.FreeHGlobal(ptr);

            if (str == null)
                return list;

            var appArray = str.Split(';');

            foreach (var app in appArray)
            {
                var appSubArray = app.Split('-');
                ExecuteItem executeItem = new ExecuteItem();
                executeItem.Name = appSubArray[0];
                executeItem.Path = appSubArray[1].Replace("\"", "");

                //TODO
                if (System.IO.File.Exists(executeItem.Path) == false)
                    continue;

                executeItem.Description = FileExtension.GetFileDescription(executeItem.Path);
                list.Add(executeItem);
            }

            return list;
        }

        private List<ExecuteItem> LoadShellShotcutItemsFromResource()
        {
            var list = new List<ExecuteItem>();
            var listStr = new List<string>();

            using (StringReader sr = new StringReader(Properties.Resources.shell))
            {
                var str = sr.ReadLine();

                while (!string.IsNullOrEmpty(str))
                {
                    listStr.Add(str);
                    str = sr.ReadLine();
                }
            }

            for (int i = 0; i < listStr.Count; i += 2)
            {
                ExecuteItem executeItem = new ExecuteItem();
                executeItem.Name = listStr[i];
                executeItem.Path = listStr[i + 1];
                executeItem.Description = listStr[i];
                list.Add(executeItem);
            }
            return list;
        }
    }
}
