using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Midi;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.OutputDevices.Tests
{
    [TestClass()]
    public class GhostOutputTests
    {
        [TestMethod()]
        public void getGhostDevicesTest()
        {

        }
    }
}

namespace ShowAutomation.Tests
{


    [TestClass]
    public partial class GhostOutputTest
    {
        [TestMethod]
        public void Constructor()
        {
            GhostOutput ghostOutput;
            ghostOutput = this.Constructor((string)null, (string)null);
            Assert.IsNotNull((object)ghostOutput);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).isConnected);
            Assert.AreEqual<string>((string)null, ((OutputDevice)ghostOutput).ghostType);
            Assert.AreEqual<int>(0, ((OutputDevice)ghostOutput).id);
            Assert.AreEqual<string>((string)null, ((OutputDevice)ghostOutput).deviceName);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)ghostOutput).type);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).isHidden);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).HideEnabled);
        }
        [TestMethod]
        public void addGhost(GhostOutput target)
        {
            target.addGhost();
            addGhost();
        }
    
        [TestMethod]
        public GhostOutput Constructor(string name, string ghostType)
        {
            GhostOutput target = new GhostOutput(name, ghostType);
            return target;
        }

        [TestMethod]
        internal void addDevice(OutputDevice outputDevice)
        {
            addDevice(outputDevice);
        }

        [TestMethod]
        internal void clearDevices()
        {
            clearDevices();
        }

        [TestMethod]
        public List<OutputDevice> getGhostDevices()
        {
            List<OutputDevice> result = GhostOutput.getGhostDevices();
            return result;
        }

        [TestMethod]
        internal void removeDevice(OutputDevice outputDevice)
        {
            removeDevice(outputDevice);
        }
        
        public void addDevice()
        {
            GhostOutput ghostOutput;
            ghostOutput = new GhostOutput((string)null, (string)null);
            ghostOutput.isConnected = false;
            ghostOutput.ghostType = (string)null;
            ((OutputDevice)ghostOutput).isHidden = false;
            ((OutputDevice)ghostOutput).HideEnabled = false;
            this.addDevice((OutputDevice)ghostOutput);
        }

        [TestMethod]
        public void addGhost()
        {
            GhostOutput ghostOutput;
            ghostOutput = new GhostOutput((string)null, (string)null);
            ghostOutput.isConnected = false;
            ghostOutput.ghostType = (string)null;
            ((OutputDevice)ghostOutput).isHidden = false;
            ((OutputDevice)ghostOutput).HideEnabled = false;
            this.addGhost(ghostOutput);
            Assert.IsNotNull((object)ghostOutput);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).isConnected);
            Assert.AreEqual<string>((string)null, ((OutputDevice)ghostOutput).ghostType);
            Assert.AreEqual<int>(0, ((OutputDevice)ghostOutput).id);
            Assert.AreEqual<string>((string)null, ((OutputDevice)ghostOutput).deviceName);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)ghostOutput).type);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).isHidden);
            Assert.AreEqual<bool>(false, ((OutputDevice)ghostOutput).HideEnabled);
        }
        

        public void addDeviceThrowsNullReferenceException()
        {
            MidiDevice midiDevice;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            this.addDevice((OutputDevice)midiDevice);
        }
        

        public void addDeviceThrowsNullReferenceException1()
        {
            this.addDevice((OutputDevice)null);
        }

        
        public void removeDevice()
        {
            MidiDevice midiDevice;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            this.removeDevice((OutputDevice)midiDevice);
        }
        

        public void removeDevice2()
        {
            GhostOutput ghostOutput;
            ghostOutput = new GhostOutput((string)null, (string)null);
            ghostOutput.isConnected = false;
            ghostOutput.ghostType = (string)null;
            ((OutputDevice)ghostOutput).isHidden = false;
            ((OutputDevice)ghostOutput).HideEnabled = false;
            this.removeDevice((OutputDevice)ghostOutput);
        }


    }
}
