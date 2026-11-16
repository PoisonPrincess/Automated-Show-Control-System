using NAudio.CoreAudioApi;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    [Serializable()]
    public class AudioDevice : OutputDevice, ISerializable
    {

        public MMDevice device { get; set; }

        public AudioDevice (int id, MMDevice device) : base(id, device.ToString())
        {
            this.device = device;
            this.isConnected = true;
            this.type = Types.AUDIO;
            this.ghostType = Types.GHOST;
        }

        public AudioDevice(string deviceName)
        {
            this.deviceName = deviceName;
            this.type = Types.AUDIO;
            this.ghostType = Types.GHOST;
        }

        public AudioDevice(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public int getDevice ()
        {
            return (this.isActive()) ? this.id : -1;
        }

        public Boolean isActive ()
        {
            return isConnected;
        }

        public override object ToJsonInstance ()
        {
            return new JsonAudioDevice(this);
        }

        public static int getDeviceIndexByName (String deviceName)
        {
            List<OutputDevice> devices = MainWindow.main.outputs.getOutputs(Types.AUDIO, true);
            for (var i = 0; i < devices.Count; i++)
            {
                if (devices[i].deviceName == deviceName)
                {
                    return i;
                }
            }
            return -1;
        }

        //this method scans for connected devices and creates new outputs from them
        //DON'T CALL UNLESS YOU WANT DUPLICATE OUTPUT DEVICES FLOATING AROUND
        public static List<AudioDevice> getAudioDevices (bool includeGhost, bool includeLane)
        {
            int id = 0;
            List <AudioDevice> deviceList = new List<AudioDevice>();

            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                deviceList.Add(new AudioDevice(id, endpoint));
                id++;
            }
            if (includeLane)
            {
                List<OutputDevice> laneDevices = LaneOutput.getLaneDevices().ToList();
                for (int i = 0; i < laneDevices.Count; i++)
                {
                    if (laneDevices[i].type.Equals(Types.AUDIO))
                    {
                        deviceList.Add(laneDevices[i] as AudioDevice);
                    }
                }
            }
            if (includeGhost)
            {
                List<OutputDevice> ghostDevices = GhostOutput.getGhostDevices().ToList();
                for (int i = 0; i < ghostDevices.Count; i++)
                {
                   
                    if (ghostDevices[i].type.Equals(Types.GHOST) && ghostDevices[i].ghostType.Equals(Types.AUDIO))
                    {
                        deviceList.Add(ghostDevices[i] as AudioDevice);
                        ghostDevices[i].id = id;
                        id++;
                    }
                }
            }

           

            return deviceList;
        }

        internal override void PlayCommandAt(Event command, ulong eventTime)
        {
            if (!IsDuplicate)
            {
                mediaManager.PlayEvent((command as MusicEvent).getPlayer(), eventTime);
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
                mediaManager.LoadEvent((command as MusicEvent).getPlayer(), eventTime);
            }
            else
            {
                original.LoadCommandAt(command, eventTime);
            }
        }
    }

    public class JsonAudioDevice : JsonOutputDevice
    {
        public JsonAudioDevice(AudioDevice device) : base(device.id, device.deviceName)
        {
            this.type = Types.AUDIO;
        }
    }
}
