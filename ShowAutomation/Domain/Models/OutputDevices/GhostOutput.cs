using System;
using System.Collections.Generic;
using ShowAutomation.Domain.Models.Events;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    public class GhostOutput : OutputDevice
    {
        static List<OutputDevice> GhostDevices = new List<OutputDevice>();
        public GhostOutput (String name, String ghostType) :base (0, name)
        {
            this.type = "Ghost";
            this.ghostType = ghostType;
            this.isConnected = false;
            addGhost();
        }
        public static List<OutputDevice> getGhostDevices()
        {
            return GhostDevices;
        }
        public void addGhost()
        {
            GhostDevices.Add(this);
        }

        public override object ToJsonInstance()
        {
            throw new NotImplementedException();
        }

        internal static void addDevice(OutputDevice outputDevice)
        {
            if (!GhostDevices.Contains(outputDevice))
            {
                for (int i = 0; i < GhostDevices.Count; i++) {
                    if (GhostDevices.ToArray()[i].deviceName.Equals(outputDevice.deviceName)) {
                        return;
                    }
                }
                GhostDevices.Add(outputDevice);
            }
        }

        internal static void removeDevice(OutputDevice outputDevice)
        {
            if (GhostDevices.Contains(outputDevice))
            {
                GhostDevices.Remove(outputDevice);
                Console.WriteLine("Removed Ghost output: " + outputDevice.deviceName);
            }
            for (int i = 0; i < GhostDevices.Count; i++)
            {
                if (GhostDevices[i].deviceName.Equals(outputDevice.deviceName))
                {
                    GhostDevices.Remove(GhostDevices[i]);
                    Console.WriteLine("Removed Ghost output: " + outputDevice.deviceName);
                }
            }
        }

        internal static void clearDevices()
        {
            GhostDevices.Clear();
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
