using System;
using System.Collections.Generic;
using ShowAutomation.Domain.Models.Events;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    public class LaneOutput : OutputDevice
    {
        static List<OutputDevice> LaneDevices = new List<OutputDevice>();
        public LaneOutput(String name, String laneType) : base(0, name)
        {
            this.type = "Lane";
            this.isConnected = false;
            addLane();
        }
        public static List<OutputDevice> getLaneDevices()
        {
            return LaneDevices;
        }
        public void addLane()
        {
            LaneDevices.Add(this);
        }

        public override object ToJsonInstance()
        {
            throw new NotImplementedException();
        }

        internal static void addDevice(OutputDevice outputDevice)
        {
            if (!LaneDevices.Contains(outputDevice))
            {
                for (int i = 0; i < LaneDevices.Count; i++)
                {
                    if (LaneDevices.ToArray()[i].deviceName.Equals(outputDevice.deviceName))
                    {
                        return;
                    }
                }
               LaneDevices.Add(outputDevice);
            }
        }

        internal static void removeDevice(OutputDevice outputDevice)
        {
            if (LaneDevices.Contains(outputDevice))
            {
                LaneDevices.Remove(outputDevice);
            }
            for (int i = 0; i < LaneDevices.Count; i++)
            {
                if (LaneDevices[i].deviceName.Equals(outputDevice.deviceName))
                {
                    LaneDevices.Remove(LaneDevices[i]);
                }
            }
        }

        internal static void clearDevices()
        {
            LaneDevices.Clear();
        }

        internal override void PlayCommandAt(Event command, ulong eventTime)
        {
            throw new NotImplementedException();
        }

        internal override void LoadCommandAt(Event @event, ulong eventTime)
        {
            throw new NotImplementedException();
        }
    }
}
