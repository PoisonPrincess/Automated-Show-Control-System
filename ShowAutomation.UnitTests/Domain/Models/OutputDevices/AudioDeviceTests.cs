using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.OutputDevices.Tests
{
    [TestClass()]
    public class AudioDeviceTests
    {

        public List<AudioDevice> getAudioDevicesTest(bool includeGhost, bool includeLane)
        {
            List<AudioDevice> result
               = AudioDevice.getAudioDevices(includeGhost, includeLane);
            return result;

        }
        [TestMethod()]
        public void getAudioDevicesTest()
        {
            List<OutputDevice> devices = new List<OutputDevice>();
            devices.AddRange(AudioDevice.getAudioDevices(true, true));
            Assert.IsNotNull(devices);
            Assert.AreEqual(devices.Count, 1);
        }

      
    }
}