using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.OutputDevices;
using NAudio.Midi;

namespace ShowAutomation.UnitTests.Domain.OutputDevices
{
    [TestClass]
     class OutputDeviceTests
    {
       

        [TestMethod]
        public void getDeviceMuteTest()
        {
            bool expected_deviceMute = false;
            OutputDevice device = new AudioDevice("Device");

            device.getDeviceMute();

            Assert.AreEqual(expected_deviceMute, device.getDeviceMute());

        }

        [TestMethod]
        public void setDeviceMuteTest()
        {
            bool expected_deviceMute = false;

            OutputDevice device = new AudioDevice("Device");

            device.setDeviceMute(false);

            Assert.AreEqual(expected_deviceMute, device.getDeviceMute());

            expected_deviceMute = true;

            device.setDeviceMute(true);

            Assert.AreEqual(expected_deviceMute, device.getDeviceMute());
        }

        [TestMethod]
        public void getDeviceSoloTest()
        {
            bool expected_deviceSolo = false;

            OutputDevice device = new AudioDevice("Device");

            device.getDeviceSolo();

            Assert.AreEqual(expected_deviceSolo, device.getDeviceSolo());
        }
        [TestMethod]
        public void setDeviceSoloTest()
        {
            bool expected_deviceSolo = false;

            OutputDevice device = new AudioDevice("Device");

            device.setDeviceSolo(false);

            Assert.AreEqual(expected_deviceSolo, device.getDeviceSolo());

            expected_deviceSolo = true;

            device.setDeviceSolo(true);

            Assert.AreEqual(expected_deviceSolo, device.getDeviceSolo());
        }
        [TestMethod]
        public void getDeviceMuteBySoloTest()
        {
            bool expected_deviceMuteBySolo = false;

            OutputDevice device = new AudioDevice("Device");

            device.getDeviceMuteBySolo();

            Assert.AreEqual(expected_deviceMuteBySolo, device.getDeviceMuteBySolo());
        }
        [TestMethod]
        public void setDeviceMuteBySoloTestTest()
        {
            bool expected_deviceMuteBySolo = false;

            OutputDevice device = new AudioDevice("Device");

            device.setDeviceMuteBySolo(false);

            Assert.AreEqual(expected_deviceMuteBySolo, device.getDeviceMuteBySolo());

            expected_deviceMuteBySolo = true;

            device.setDeviceMuteBySolo(true);

            Assert.AreEqual(expected_deviceMuteBySolo, device.getDeviceMuteBySolo());
        }

        
        public string listItemTextColorGet(OutputDevice target)
        {
            string result = target.listItemTextColor;
            return result;
           
        }

  
        public bool isHiddenGet(OutputDevice target)
        {
            bool result = target.isHidden;
            return result;
           
        }

        public void Hide(OutputDevice target, bool hide)
        {
            target.Hide(hide);
          
        }
    
        public string displayTypeGet(OutputDevice target)
        {
            string result = target.displayType;
            return result;
          
        }

        [TestMethod]
        public void displayTypeGet2()
        {
            MidiDevice midiDevice;
            string s;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            s = this.displayTypeGet((OutputDevice)midiDevice);
            Assert.AreEqual<string>("MIDI", s);
        }
        [TestMethod]
        public void Hide1()
        {
            MidiDevice midiDevice;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            this.Hide((OutputDevice)midiDevice, false);
            Assert.AreEqual<bool>(false, midiDevice.isHidden);
        }

        [TestMethod]
        public void isHiddenGet()
        {
            MidiDevice midiDevice;
            bool b;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            b = this.isHiddenGet((OutputDevice)midiDevice);
            Assert.AreEqual<bool>(false, b);
        }
        [TestMethod]
        public void listItemTextColorGet()
        {
            MidiDevice midiDevice;
            string s;
            midiDevice = new MidiDevice(0, (MidiOut)null);
            midiDevice.isConnected = false;
            midiDevice.ghostType = (string)null;
            midiDevice.device = (MidiOut)null;
            ((OutputDevice)midiDevice).isHidden = false;
            ((OutputDevice)midiDevice).HideEnabled = false;
            s = this.listItemTextColorGet((OutputDevice)midiDevice);
            Assert.AreEqual<string>("Black", s);
        }


    }

}
