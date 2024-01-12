using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Master_Zhao.Shell.Model.SystemMgmt
{
    public class DiskPath
    {
        private static readonly ImageSource RootIcon = PInvoke.IconTool.ExtractStokeIcon(PInvoke.SHSTOCKICONID.SIID_DESKTOPPC);
        private static readonly ImageSource DiskIcon = PInvoke.IconTool.ExtractStokeIcon(PInvoke.SHSTOCKICONID.SIID_DRIVEFIXED);
        private static readonly ImageSource FolderIcon = PInvoke.IconTool.ExtractStokeIcon(PInvoke.SHSTOCKICONID.SIID_FOLDER);

        public string DisplayName { get; set; }

        public string Path { get; set; }

        public ImageSource Icon { get; set; }

        private DiskPathType diskPathType;
        public DiskPathType DiskPathType 
        {
            get => diskPathType;
            set
            {
                diskPathType = value;

                switch(diskPathType)
                {
                    case DiskPathType.Computer:
                        Icon = RootIcon;
                        break;
                    case DiskPathType.Disk:
                        Icon = DiskIcon;
                        break;
                    case DiskPathType.Folder:
                        Icon = FolderIcon;
                        break;
                    default:
                        break;
                }
            }
        }

        public ObservableCollection<DiskPath> Children { get; set; }

        /// <summary>
        /// current folder size
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// all files count in current folder
        /// </summary>
        public long FileCount { get; set; }

        /// <summary>
        /// all sub folder count in current folder
        /// </summary>
        public long SubDirCount { get; set; }
    }

    public enum DiskPathType
    {
        Computer,
        Disk,
        Folder,
        File
    }
}
