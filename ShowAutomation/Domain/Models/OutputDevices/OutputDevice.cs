using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    [Serializable()]
    public abstract class OutputDevice  : INotifyPropertyChanged, ISerializable
    {
        public int id { get; set; }
        public String deviceName { get ; set; }
        public Boolean isConnected = false;
        //mute/solo variables
        private bool deviceMute = false;
        public String MuteLabel
        {
            get
            {
                if (deviceMute)
                {
                    return "Unmute";
                }
                else return "Mute";
            }
        }
        private bool deviceSolo = false;
        public String SoloLabel
        {
            get
            {
                if (deviceSolo)
                {
                    return "Unsolo";
                }
                else return "Solo";
            }
        }
        private bool deviceMuteBySolo = false;

        public String type { get; set; }
        public String displayType { get
            {
                if (type.Equals(Types.GHOST))
                {
                    return type + ": " + ghostType;
                }
                else return type;
            }
        }

        public List<OutputDevice> duplicates = new List<OutputDevice>();
        public OutputDevice original;
        public bool IsDuplicate
        {
            get { return original != null; }
        }
        public String listItemTextColor
        {
            get
            {
                return ColorManager.TEXT_COLOR;
            }
            set { }
        }
        public String ghostType;

        //Command/Media playing Variables
        protected MediaManager mediaManager;

        public bool Hidden = false;

        public bool isHidden { get
            {
                return Hidden;
            }
            set {
                if (Hidden != value)
                {
                    MainWindow.main.DoAction(new Actions.HideOutputLaneAction(this, value));
                }
            }
        }
        public bool HideEnabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string deviceName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(deviceName));
        }

        public OutputDevice(int id, String deviceName)
        {
            this.deviceName = deviceName;
            this.id = id;
            mediaManager = new MediaManager(this);
        }

        public OutputDevice(string deviceName) : this(0, deviceName)
        {
        }

        public OutputDevice() : this("Device")
        {
        }

        public OutputDevice(SerializationInfo info, StreamingContext context) : this((int)info.GetValue("id", typeof(int)), (String)info.GetValue("deviceName", typeof(string)))
        {
            type = (String)info.GetValue("type", typeof(string));
            ghostType = (String)info.GetValue("ghostType", typeof(string));
            try
            {
                original = (OutputDevice)info.GetValue("original", typeof(OutputDevice));
                if (original != null && !MainWindow.main.outputs.outputList.Contains(original))
                {
                    //Remove old devices and replace with new to clean out duplicate devices and remove any issues with remembering old hardware setups
                    foreach (OutputDevice device in MainWindow.main.outputs.outputList)
                    {
                        if (original.deviceName.Equals(device.deviceName) && original.type.Equals(device.type))
                        {
                            original = device;
                            device.duplicates.Add(this);
                            break;
                        }
                    }
                }
            }
            catch {
                original = null;
            }
            try
            {
                Hidden = (bool)info.GetValue("hidden", typeof(bool));
            }
            catch {
                Hidden = false; 
            }
            try
            {
                deviceMute = (bool)info.GetValue("mute", typeof(bool));
                deviceSolo = (bool)info.GetValue("solo", typeof(bool));
                deviceMuteBySolo = (bool)info.GetValue("muteBySolo", typeof(bool));
            }
            catch
            {
                deviceMute = false;
                deviceSolo = false;
                deviceMuteBySolo = false;
            }
        }

        public abstract object ToJsonInstance();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);
            info.AddValue("ghostType", ghostType);
            info.AddValue("type", type);
            info.AddValue("deviceName", deviceName);
            info.AddValue("original", original);
            info.AddValue("hidden", Hidden);
            info.AddValue("mute", deviceMute);
            info.AddValue("solo", deviceSolo);
            info.AddValue("muteBySolo", deviceMuteBySolo);
        }

        public bool getDeviceMute()
        {
            return deviceMute;
        }

        public void setDeviceMute(bool mute)
        {
            deviceMute = mute;
            mediaManager.Mute(mute);
        }

        public bool getDeviceSolo()
        {
            return deviceSolo;
        }

        public void setDeviceSolo(bool solo)
        {
            deviceSolo = solo;
        }

        public bool getDeviceMuteBySolo()
        {
            return deviceMuteBySolo;
        }

        public void setDeviceMuteBySolo(bool mute)
        {
            deviceMuteBySolo = mute;
            mediaManager.MuteBySolo(mute);
        }

        internal virtual void OpenVideoPanel() { }

        internal abstract void PlayCommandAt(Event ev, ulong eventTime);
        internal abstract void LoadCommandAt(Event @event, ulong eventTime);
        internal void PauseCommands()
        {
            mediaManager.Pause();
        }

        internal void StopCommands()
        {
            mediaManager.Stop();
        }

        public OutputDevice CreateDuplicate()
        {
            String name;
            OutputDevice device = null;

            name = "(" + (duplicates.Count) + ") " + deviceName;
            Console.WriteLine("New Lane " + name + " " + type);

            if (type.Equals(Types.AUDIO))
            {
                device = new AudioDevice(name);
            }
            else if (type.Equals(Types.VIDEO))
            {
                device = new VideoDevice(name);
            }
            else if (type.Equals(Types.MIDI))
            {
                device = new MidiDevice(name);
            }

            if (device != null)
            {
                device.id = id;
                device.original = this;
            }
            return device;
        }

        public void RemoveDuplicate(OutputDevice device) {
            if (duplicates.Contains(device)) {
                duplicates.Remove(device);
                foreach (OutputDevice othersDevices in duplicates) {
                    othersDevices.deviceName = "(" + duplicates.IndexOf(othersDevices) + ") " + deviceName;
                }
            }
        }

        internal void Remove()
        {
            if (original != null) {
                original.RemoveDuplicate(this);
            }
        }

        internal bool OriginalExists()
        {
            Console.WriteLine("Check if " + original + " exists " + MainWindow.main.outputs.outputList.Contains(original));
            return MainWindow.main.outputs.outputList.Contains(original);
        }
    }

    public class JsonOutputDevice
    {
        public int id;
        public String type;
        public String deviceName;

        public JsonOutputDevice(int id, String deviceName)
        {
            this.id = id;
            this.deviceName = deviceName;
        }
    }
}
