using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.UI.ViewModels.Scenes;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.UI.Views.Tests
{
    [TestClass()]
    public class MapDevicesWindowTests
    {

        public MapDevicesWindow Constructor(
            bool onOpen,
            SceneList sceneList,
            Func<SceneList, SceneList> doneFunction
        )
        {
            MapDevicesWindow target = new MapDevicesWindow(onOpen, sceneList, doneFunction);
            return target;
      
        }

        public void MapDevicesWindowTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void MapDevicesWindowConstructorThrowsTypeInitializationException()
        {
            MapDevicesWindow mapDevicesWindow;
            mapDevicesWindow =
              this.Constructor(false, (SceneList)null, (Func<SceneList, SceneList>)null);
            Assert.IsNotNull((object)mapDevicesWindow);
        }
    }
}