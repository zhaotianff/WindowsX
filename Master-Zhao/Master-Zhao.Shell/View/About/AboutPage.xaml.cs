using Master_Zhao.Shell.Model.About;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            InitProjectReference();
        }

        private void InitProjectReference()
        {
            var list = new List<ReferenceProjectInfo>()
            {
                new ReferenceProjectInfo(){ProjectName = "TranslucentTB",ProjectUrl = "https://github.com/TranslucentTB/TranslucentTB",ProjectDescription = "A lightweight (uses a few MB of RAM and almost no CPU) utility that makes the Windows taskbar translucent/transparent on Windows 10." },
                new ReferenceProjectInfo(){ProjectName = "TaskbarX",ProjectUrl = "https://github.com/ChrisAnd1998/TaskbarX",ProjectDescription = "TaskbarX gives you control over the position of your taskbar icons. TaskbarX will give you an original Windows dock like feel." },
                new ReferenceProjectInfo(){ProjectName = "TaskbarTool",ProjectUrl = "https://github.com/Elestriel/TaskbarTools",ProjectDescription = "Grants the ability to set the Windows Taskbars to any opacity and colour desired. This will affect the Taskbars on every monitor/desktop, not only the primary." },
                new ReferenceProjectInfo(){ProjectName = "BlurWindow",ProjectUrl = "https://github.com/TianXiaTech/BlurWindow",ProjectDescription = "WPF Aero Glass Lib." },
                new ReferenceProjectInfo(){ProjectName = "ManagedShell",ProjectUrl = "https://github.com/cairoshell/ManagedShell",ProjectDescription = "A library for creating Windows shell replacements using .NET, written in C#." },
                new ReferenceProjectInfo(){ProjectName = "WPFDevelopers",ProjectUrl="https://github.com/WPFDevelopersOrg/WPFDevelopers",ProjectDescription = "This is a UI library for WPF developers based on WPF custom advanced controls. Welcome to use.." },
                new ReferenceProjectInfo(){ProjectName = "ExplorerBgTool",ProjectUrl="https://github.com/Maplespe/explorerTool",ProjectDescription = "Let your Explorer have a custom background image for Windows 11 and WIndows 10" }
            };

            this.list_ProjectReference.ItemsSource = list;
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            (System.Windows.Application.Current.MainWindow as MainWindow).EndShowMenuAnimation();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperLink = sender as Hyperlink;
            var tr = new TextRange(hyperLink.ContentStart, hyperLink.ContentEnd);
            ProcessHelper.OpenUrl(tr.Text);
        }

        private void list_ProjectReference_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (list_ProjectReference.SelectedItem == null)
                return;

            var projectInfo = list_ProjectReference.SelectedItem as ReferenceProjectInfo;
            ProcessHelper.OpenUrl(projectInfo.ProjectUrl);
        }
    }
}
