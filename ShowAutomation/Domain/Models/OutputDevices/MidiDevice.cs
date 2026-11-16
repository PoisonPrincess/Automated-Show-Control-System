using NAudio.Midi;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    [Serializable()]
    public class MidiDevice : OutputDevice, ISerializable
    {
        public MidiOut _device;
        public MidiOut device { get { return _device; } set { _device = value;}}

        public MidiDevice (int id, MidiOut device) : base(id, MidiOut.DeviceInfo(id).ProductName)
        {
            this.device = device;
            this.isConnected = true;
            this.type = Types.MIDI;
           this.ghostType = Types.GHOST;
        }
        public MidiDevice(String name)
        {
            this.deviceName = name;
            this.type = Types.MIDI;
            this.ghostType = Types.GHOST;
        }

        public MidiDevice(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public int getDevice()
        {
            return (this.isActive()) ? this.id : 0;
        }

        public Boolean isActive()
        {
            return true;
        }

        public override object ToJsonInstance ()
        {
            return new JsonMidiDevice(this);
        }

        public static int getDeviceIndexByName(String deviceName)
        {
            List<OutputDevice> devices = MainWindow.main.outputs.getOutputs(Types.MIDI, true);
            for (var i = 0; i < devices.Count; i++)
            {
                if (devices[i].deviceName == deviceName)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int isDeviceConnected (String deviceName)
        {
            List<OutputDevice> devices = MainWindow.main.outputs.getOutputs(Types.MIDI, false);
            for (int i = 0; i < devices.Count; i++)
            {
                
            }
            return -1;
        }

        internal override void PlayCommandAt(Event command, ulong eventTime)
        {
            if (!IsDuplicate)
            {
                mediaManager.PlayEvent((command as Events.MidiEvent).getPlayer(), eventTime);
            }
            else
            {
                original.PlayCommandAt(command, eventTime);
            }
        }

        internal override void LoadCommandAt(Event command, ulong eventTime)
        {
            if (!IsDuplicate)
            {
                mediaManager.LoadEvent((command as Events.MidiEvent).getPlayer(), eventTime);
            }
            else
            {
                original.LoadCommandAt(command, eventTime);
            }
        }
    }

    public class JsonMidiDevice : JsonOutputDevice
    {
        public JsonMidiDevice(MidiDevice device) : base(device.id, device.deviceName)
        {
            this.type = Types.MIDI;
        }
    }
}
