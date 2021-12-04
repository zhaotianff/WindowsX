using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.MessageBoxEx
{
    public class WaitMessageBox
    {
        public static async Task<bool> Show(string title,string content,string confirmText,int timeout = 10)
        {
            return await WaitMessageBoxWindow.Show(title,content,confirmText,timeout);
        }
    }
}
