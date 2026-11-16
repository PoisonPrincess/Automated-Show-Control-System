using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Midi;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ShowAutomation.Domain.Utils.Tests
{
    [TestClass()]
    public partial class MidiDeviceTest
    {
        [TestMethod]
        public MidiDevice Constructor(int id, MidiOut device)
        {
            MidiDevice target = new MidiDevice(id, device);
            return target;

        }

        [TestMethod]
        public void Constructor()
        {
            MidiDevice midiDevice;
            midiDevice = this.Constructor(0, (MidiOut)null);
            Assert.IsNotNull((object)midiDevice);
            Assert.IsNull((object)(midiDevice.device));
            Assert.AreEqual<bool>(true, ((OutputDevice)midiDevice).isConnected);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)midiDevice).ghostType);
            Assert.AreEqual<int>(0, ((OutputDevice)midiDevice).id);
            Assert.AreEqual<string>
                ("Microsoft GS Wavetable Synth", ((OutputDevice)midiDevice).deviceName);
            Assert.AreEqual<string>("MIDI", ((OutputDevice)midiDevice).type);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).isHidden);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).HideEnabled);
        }

        // Ghost Midi Event
        public void Constructor1()
        {
            MidiDevice midiDevice;
            midiDevice = this.Constructor1((string)null);
            Assert.IsNotNull((object)midiDevice);
            Assert.IsNull((object)(midiDevice.device));
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).isConnected);
            Assert.AreEqual<string>("Ghost", ((OutputDevice)midiDevice).ghostType);
            Assert.AreEqual<int>(0, ((OutputDevice)midiDevice).id);
            Assert.AreEqual<string>((string)null, ((OutputDevice)midiDevice).deviceName);
            Assert.AreEqual<string>("MIDI", ((OutputDevice)midiDevice).type);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).isHidden);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).HideEnabled);
        }


        [TestMethod]
        public MidiDevice Constructor1(string name)
        {
            MidiDevice target = new MidiDevice(name);
            return target;

        }

        [TestMethod]
        public int getDevice(MidiDevice target)
        {
            int result = target.getDevice();
            return result;

        }

        // get device for MIDI type
        [TestMethod]
        public void getDevice0()
        {
            MidiDevice midiDevice;
            int i;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            i = this.getDevice(midiDevice);
            Assert.AreEqual<int>(0, i);
            Assert.IsNotNull((object)midiDevice);
            Assert.IsNull((object)(midiDevice.device));
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).isConnected);
            Assert.AreEqual<string>((string)null, ((OutputDevice)midiDevice).ghostType);
            Assert.AreEqual<int>(0, ((OutputDevice)midiDevice).id);
            Assert.AreEqual<string>
                ("Microsoft GS Wavetable Synth", ((OutputDevice)midiDevice).deviceName);
            Assert.AreEqual<string>("MIDI", ((OutputDevice)midiDevice).type);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).isHidden);
            Assert.AreEqual<bool>(false, ((OutputDevice)midiDevice).HideEnabled);
        }
    }
}