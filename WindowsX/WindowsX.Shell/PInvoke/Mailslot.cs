using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.PInvoke
{
    public class Mailslot
    {
        public const string SHELLHOOK_MAILSLOT_NAME = "\\\\.\\mailslot\\shellhook_slot";

        [DllImport("WindowsXCore.dll")]
        public static extern bool WriteToMailslot([MarshalAs(UnmanagedType.LPWStr)] string lpszSlotName, [MarshalAs(UnmanagedType.LPWStr)] string lpszMessage);

        public static bool WriteToShellHookMailslot(string message)
        {
            return WriteToMailslot(SHELLHOOK_MAILSLOT_NAME, message);
        }
    }
}
