using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Tests
{
    [TestClass()]
    public partial class MainWindowTests
    {
        public MainWindow Constructor()
        {
            MainWindow target = new MainWindow();
            return target;
        }

        public void ConstructorThrowsTypeInitializationException()
        {
            MainWindow mainWindow;
            mainWindow = this.Constructor();
        }

        public List<OutputDevice> GetOutputDevices()
        {
            List<OutputDevice> result = MainWindow.GetOutputDevices();
            return result;

        }

        public void MainWindowTest()
        {
            MainWindow main;
        }

        [TestMethod]
        public void GetOutputDevices220()
        {
            List<OutputDevice> list;
            list = this.GetOutputDevices();
            Assert.IsNotNull((object)list);
            Assert.AreEqual<int>(4, list.Capacity);
            Assert.AreEqual<int>(3, list.Count);
        }

        [TestMethod]
        public void getDefaultDeviceTests()
        {
            Assert.Inconclusive();
        }


    }
}