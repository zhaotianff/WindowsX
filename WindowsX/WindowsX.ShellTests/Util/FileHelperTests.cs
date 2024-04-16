using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsX.Shell.Model.SystemMgmt;

namespace WindowsX.Shell.Util.Tests
{
    [TestClass()]
    public class FileHelperTests
    {
        [TestMethod()]
        public void EnumerateSubDirectoryTest()
        {
            var list = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
            //FileHelper.EnumerateSubDirectory("E:\\files", list);

            Assert.IsTrue(list.Count > 0);
        }
    }
}