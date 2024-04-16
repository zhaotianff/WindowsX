using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsX.Shell.Model.SystemMgmt;

namespace WindowsX.Shell.PInvoke.Tests
{
    [TestClass()]
    public class Kernel32Tests
    {
        [TestMethod()]
        public void EnumerateSubDirectoryTest()
        {
            var list = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
            PInvoke.Kernel32.EnumerateSubDirectory("C:\\", list,true,true);

            Assert.IsTrue(list.Count > 0);
        }
    }
}