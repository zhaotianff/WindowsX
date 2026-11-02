using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.Services.SystemMgmt;
using WindowsX.Shell.View.SystemMgmt.UserControls;

namespace WindowsX.Shell.View.SystemMgmt.Pages
{
    /// <summary>
    /// SystemInfo.xaml 的交互逻辑
    /// </summary>
    public partial class SystemInfo : Page
    {
        private readonly SystemInformationService hardwareService = new SystemInformationService();
        private readonly InstalledSoftwareService softwareService = new InstalledSoftwareService();
        private readonly Dictionary<string, SystemInfoItemControl> panels;
        private CancellationTokenSource loadCancellation;
        private Task loadTask;
        private bool hasLoaded;
        private ICollectionView softwareView;
        private List<InstalledSoftware> software = new List<InstalledSoftware>();

        public SystemInfo()
        {
            InitializeComponent();
            panels = new Dictionary<string, SystemInfoItemControl>
            {
                ["System"] = Panel_Sys, ["MotherBoard"] = Panel_MotherBoard,
                ["Cpu"] = Panel_Cpu, ["Memory"] = Panel_Memory, ["Disk"] = Panel_Disk,
                ["Graphics"] = Panel_Graphics, ["Monitor"] = Panel_Monitor, ["Network"] = Panel_Network
            };
            SetPanelStatus("尚未读取本机信息");
            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        public Task LoadSystemInfoAsync()
        {
            if (hasLoaded)
                return Task.CompletedTask;
            if (loadTask != null && !loadTask.IsCompleted)
                return loadTask;
            loadTask = LoadCoreAsync();
            return loadTask;
        }

        private async Task LoadCoreAsync()
        {
            var cancellation = new CancellationTokenSource();
            loadCancellation = cancellation;
            var token = cancellation.Token;
            SetBusy(true);
            SetPanelStatus("正在读取…");
            software.Clear();
            softwareView = null;
            grid_Software.ItemsSource = null;
            tbk_SoftwareCount.Text = string.Empty;
            tbk_SoftwareEmpty.Text = "正在读取软件清单…";
            tbk_SoftwareEmpty.Visibility = Visibility.Visible;
            tbk_Status.Text = "正在读取本机信息…";
            tbk_Status.ToolTip = null;
            var progress = new Progress<string>(message =>
            {
                if (ReferenceEquals(loadCancellation, cancellation) && !token.IsCancellationRequested)
                    tbk_Status.Text = message;
            });
            try
            {
                var hardwareTask = Task.Run(() => hardwareService.GetHardwareInformation(token, progress), token);
                var softwareTask = Task.Run(() => softwareService.GetInstalledSoftware(token), token);
                await Task.WhenAll(hardwareTask, softwareTask);
                token.ThrowIfCancellationRequested();
                if (!ReferenceEquals(loadCancellation, cancellation))
                    return;

                var hardware = await hardwareTask;
                var installed = await softwareTask;
                foreach (var panel in panels)
                    panel.Value.SystemInformation = hardware.Categories[panel.Key];
                software = installed.Items;
                softwareView = CollectionViewSource.GetDefaultView(software);
                softwareView.Filter = item => item is InstalledSoftware entry && InstalledSoftwareService.Matches(entry, tbox_SoftwareSearch.Text);
                grid_Software.ItemsSource = softwareView;
                RefreshSoftwareFilter();
                var warnings = hardware.Warnings.Concat(installed.Warnings).ToList();
                tbk_Status.Text = $"已更新 {DateTime.Now:HH:mm:ss}" + (warnings.Count > 0 ? $"；{warnings.Count} 项提示（悬停查看）" : "；读取完成");
                tbk_Status.ToolTip = warnings.Count == 0 ? "本页显示最近一次采集的快照，可点击刷新重新读取。" : string.Join(Environment.NewLine, warnings.Take(20));
                hasLoaded = true;
            }
            catch (OperationCanceledException)
            {
                if (ReferenceEquals(loadCancellation, cancellation))
                {
                    SetPanelStatus("读取已取消，请刷新重试");
                    tbk_Status.Text = "读取已取消";
                    tbk_SoftwareEmpty.Text = "读取已取消，请刷新重试";
                }
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(loadCancellation, cancellation))
                {
                    SetPanelStatus("读取失败，请刷新重试");
                    tbk_Status.Text = "读取失败：" + ex.Message;
                    tbk_SoftwareEmpty.Text = "读取失败，请刷新重试";
                }
            }
            finally
            {
                if (ReferenceEquals(loadCancellation, cancellation))
                {
                    loadCancellation = null;
                    SetBusy(false);
                }
                cancellation.Dispose();
            }
        }

        private void SetPanelStatus(string message)
        {
            foreach (var panel in panels.Values)
                panel.SystemInformation = SystemInformationService.Message(message);
        }

        private void SetBusy(bool busy)
        {
            btn_Refresh.IsEnabled = !busy;
            btn_Cancel.IsEnabled = busy;
            progress_Loading.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadSystemInfoAsync();

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            loadCancellation?.Cancel();
            loadCancellation = null;
            loadTask = null;
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            hasLoaded = false;
            await LoadSystemInfoAsync();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            loadCancellation?.Cancel();
            btn_Cancel.IsEnabled = false;
            tbk_Status.Text = "正在取消，等待当前系统查询返回…";
        }

        private void tbox_SoftwareSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshSoftwareFilter();

        private void RefreshSoftwareFilter()
        {
            if (softwareView == null)
                return;
            softwareView.Refresh();
            var count = softwareView.Cast<object>().Count();
            tbk_SoftwareCount.Text = $"显示 {count} / {software.Count} 个软件（可点击列标题排序）";
            tbk_SoftwareEmpty.Text = software.Count == 0 ? "未发现已登记的桌面程序" : "没有匹配的软件";
            tbk_SoftwareEmpty.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
