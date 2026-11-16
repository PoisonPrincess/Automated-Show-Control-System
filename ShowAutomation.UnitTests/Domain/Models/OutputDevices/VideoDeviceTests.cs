using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.OutputDevices.Tests
{
    [TestClass()]
    public class VideoDeviceTests
    {
 
      
        [DataTestMethod]
        public VideoDevice Constructor01(string deviceName)
        {
            VideoDevice target = new VideoDevice(deviceName);
            return target;
        }

        [DataTestMethod]
        public VideoDevice Constructor02(int id, string deviceName1)
        {
            VideoDevice target = new VideoDevice(id, deviceName1);
            return target;
        }

        [DataTestMethod]
        public VideoDevice Constructor03(SerializationInfo info, StreamingContext context)
        {
            VideoDevice target = new VideoDevice(info, context);
            return target;
           
        }

        [TestMethod]
        public void Constructor01()
        {
            VideoDevice videoDevice;
            videoDevice = this.Constructor01((string)null);
            Assert.IsNotNull((object)videoDevice);
            Assert.AreEqual<bool>(false, ((OutputDevice)videoDevice).isConnected);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)videoDevice).ghostType);
            Assert.IsNull(videoDevice.device);
            Assert.AreEqual<int>(0, ((OutputDevice)videoDevice).id);
            Assert.AreEqual<string>((string)null, ((OutputDevice)videoDevice).deviceName);
            Assert.AreEqual<string>("Video", ((OutputDevice)videoDevice).type);

            Assert.AreEqual<bool>(false, ((OutputDevice)videoDevice).isHidden);

            Assert.AreEqual<bool>(false, ((OutputDevice)videoDevice).HideEnabled);
        }

        [TestMethod]
        public void Constructor02()
        {
            VideoDevice videoDevice;
            videoDevice = this.Constructor02(0, (string)null);
            Assert.IsNotNull((object)videoDevice);
            Assert.AreEqual<bool>(true, ((OutputDevice)videoDevice).isConnected);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)videoDevice).ghostType);
            Assert.IsNull(videoDevice.device);
            Assert.AreEqual<int>(0, ((OutputDevice)videoDevice).id);
            Assert.AreEqual<string>((string)null, ((OutputDevice)videoDevice).deviceName);
            Assert.AreEqual<string>("Video", ((OutputDevice)videoDevice).type);

            Assert.AreEqual<bool>(false, ((OutputDevice)videoDevice).isHidden);  

            Assert.AreEqual<bool>(false, ((OutputDevice)videoDevice).HideEnabled);
        }

        public void Constructor03ThrowsNullReferenceException()
        {
            VideoDevice videoDevice;
            videoDevice =
              this.Constructor03((SerializationInfo)null, default(StreamingContext));
        }

    }
}