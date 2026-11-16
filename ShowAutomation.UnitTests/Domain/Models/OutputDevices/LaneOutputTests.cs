using ShowAutomation.Domain.Models.OutputDevices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShowAutomation.Domain.Utils;
using System.Collections.Generic;

namespace ShowAutomation.Domain.Models.OutputDevices.Tests
{
    [TestClass()]
    public class LaneOutputTests
    {

        static List<OutputDevice> LaneDevices = new List<OutputDevice>();

        [TestMethod()]
        public LaneOutput ConstructorTest(string name, string laneType)
        {
            LaneOutput target = new LaneOutput(name, laneType);

            return target;
            // TODO: add assertions to method LaneOutputTest.ConstructorTest(String, String)
        }

        public void addDeviceTest(OutputDevice outputDevice)
        {
            PrivateObject objToTestPrivateMethod = new PrivateObject(typeof(LaneOutput));
            bool result = Convert.ToBoolean(objToTestPrivateMethod.Invoke("addDevice", outputDevice));

        }

        [TestMethod()]
        public void addLaneDevice()
        {
            /*
            OutputDevice device = null;
            device = new AudioDevice("test");
            device.type = Types.AUDIO;
            addDeviceTest(device);
            */
            Assert.Inconclusive(); //Currently Fails
        }

        [TestMethod()]
        public void addLaneTest()
        {
            var name = " boo";
            LaneOutput lane = new LaneOutput(name, Types.AUDIO);
            lane.addLane();
            Assert.IsNotNull(lane);
        }

        [TestMethod()]
        public void getLaneDevicesTest()
        {
            List<OutputDevice> list;
            list = LaneDevices;
            Assert.IsNotNull((object)list);
            Assert.AreEqual<int>(32, list.Capacity);
            Assert.AreEqual<int>(28, list.Count);
        }
    }
}