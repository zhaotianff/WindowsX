using Microsoft.VisualStudio.TestTools.UnitTesting;
using Master_Zhao.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master_Zhao.Shell.Model.SystemMgmt;

namespace Master_Zhao.Shell.PInvoke.Tests
{
    [TestClass()]
    public class Kernel32Tests
    {
        [TestMethod()]
        public void EnumerateSubDirectoryTest()
        {
            var list = new List<DiskPath>();
            PInvoke.Kernel32.EnumerateSubDirectory("C:\\", list);

            Assert.IsTrue(list.Count > 0);
        }
    }
}