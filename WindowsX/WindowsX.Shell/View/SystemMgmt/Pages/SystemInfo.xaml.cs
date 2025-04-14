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

namespace WindowsX.Shell.View.SystemMgmt.Pages
{
    /// <summary>
    /// SystemInfo.xaml 的交互逻辑
    /// </summary>
    public partial class SystemInfo : Page
    {
        public SystemInfo()
        {
            InitializeComponent();

            LoadSystem();
        }

        private void LoadSystem()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["系统"] = "Windows 10 专业版";
            dic["版本"] = "1903";
            dic["版本号"] = "18362.295";
            dic["系统类型"] = "64位操作系统";
            dic["安装内存"] = "64GB";
            dic["处理器"] = "Intel(R) Core(TM) i5-12400";
            dic["系统名称"] = "Vostro 3777";
            dic["系统制造商"] = "Dell Inc.";
            dic["安装目录"] = "C:\\Windows";
            dic["安装日期"] = "2025年10月3日";
            dic["上次开机时间"] = "2025年10月3日 24:00:00";
            dic["上次关机时间"] = "2025年10月2日 24:00:00";

            this.Panel_Sys.SystemInfoCollection = dic;
        }

        private void LoadMotherBoard()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["BIOS模式"] = "UEFI";
            dic["BIOS版本"] = "1.0";
            dic["BIOS日期"] = "2023年10月3日";
            dic["序列号"] = "24323N";
            dic["唤醒方式"] = "电源";
        }

        private void LoadCPU()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["制造商"] = "Intel";
            dic["型号"] = "12th Gen Intel(R) Core(TM) i5-12400";
            dic["基准速度"] = "2.5 GHz";
            dic["核心数"] = "6核";
            dic["线程数"] = "12线程";
            dic["L1缓存"] = "480KB";
            dic["L2缓存"] = "7.5MB";
            dic["L3缓存"] = "18.0MB";
        }

        private void LoadMemory()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["容量"] = "64GB";
            dic["制造商"] = "Kingston";
            dic["速度"] = "3200 MHz";
            dic["电压"] = "1200";
        }

        private void LoadGraphics()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["型号"] = "RTX 4080";
            dic["制造商"] = "Nvidia";
            dic["显存"] = "16GB";
            dic["驱动版本"] = "368.86349";
            dic["DX版本"] = "12.1";
            dic["位宽"] = "256Bit";
        }

        private void LoadDisk()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["型号"] = "Blue";
            dic["制造商"] = "WestData";
            dic["容量"] = "1920GB";
            dic["可用空间"] = "100GB";
            dic["使用次数"] = "837次";

        }
    }
}
