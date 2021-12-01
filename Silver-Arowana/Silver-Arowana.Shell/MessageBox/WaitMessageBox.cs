using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silver_Arowana.Shell.MessageBox
{
    public class WaitMessageBox
    {
        private static int waitSecond;

        public static void Init(int second)
        {
            waitSecond = second;
        }

        public static void Init()
        {
            waitSecond = 10;
        }

        public static async Task<bool> Show(string title,string content)
        {
            //TODO
            await Task.Delay(waitSecond);
            return true;
        }
    }
}
