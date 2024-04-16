using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Util
{
    public class DialogHelper
    {
        public static string BrowserSingleFile(string filter = "全部文件|*.*",string title = "",string initPath = "")
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.Title = title;
            openFileDialog.InitialDirectory = initPath;

            if(openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return "";
        }

    }
}
