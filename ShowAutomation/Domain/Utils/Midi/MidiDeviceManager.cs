using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio;
using NAudio.Midi;
using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Utils.Midi
{
    public class MidiDeviceManager
    {
        // Public Properties
        private List<MidiDevice> MidiOutDevices { get; set; }

        public static MidiDeviceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MidiDeviceManager();
                }
                return instance;
            }
        }

        // Private Properties
        private static MidiDeviceManager instance;

        // Constructors
        private MidiDeviceManager()
        {
            RefreshMidiOutDevices();
        }

        public void RefreshMidiOutDevices()
        {
            if (MidiOutDevices == null)
            {
                MidiOutDevices = new List<MidiDevice>();
            }

            // Clear device list.
            MidiOutDevices.Clear();
            // Open devices.
            for (int deviceID = 0; deviceID < MidiOut.NumberOfDevices; deviceID++)
            {
                MidiOut midiOut = null;
                try
                {
                    midiOut = new MidiOut(deviceID);                     
                }
                catch(MmException e){
                    //Duplicate device midiOut not created due to it aleardy being opened
                    if (e.Message.Equals("AlreadyAllocated calling midiOutOpen")) {
                        Console.WriteLine("Tried to create Duplicate MIDI device");
                    } else {
                        MessageBox.Show(
                        "Error on MIDI Device Creation: check you have the proper drivers for the device you have plugged in",
                        "MIDI Device Creation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.None);
                        return;
                    }
                }
                var midiOutDevice = new MidiDevice(deviceID, midiOut);
                MidiOutDevices.Add(midiOutDevice);
            }
        }

        //this method scans for connected devices and creates new outputs from them
        //DON'T CALL UNLESS YOU WANT DUPLICATE OUTPUT DEVICES FLOATING AROUND
        public List<MidiDevice> getMidiDevices(bool includeGhost, bool includeLane) {
            List<MidiDevice> deviceList = new List<MidiDevice>();

            deviceList.AddRange(MidiOutDevices);
            int id = deviceList.Count;
            if (includeLane)
            {
                List<OutputDevice> laneDevices = LaneOutput.getLaneDevices().ToList();
                for (int i = 0; i < laneDevices.Count; i++)
                {
                    if (laneDevices[i].type.Equals(Types.MIDI))
                    {
                        deviceList.Add(laneDevices[i] as MidiDevice);
                        laneDevices[i].id = id;
                        id++;
                    }
                }
            }
            if (includeGhost)
            {
                List<OutputDevice> ghostDevices = GhostOutput.getGhostDevices().ToList();
                for (int i = 0; i < ghostDevices.Count; i++)
                {
                    if (ghostDevices[i].type.Equals(Types.GHOST) && ghostDevices[i].ghostType.Equals(Types.MIDI))
                    {
                        deviceList.Add(ghostDevices[i] as MidiDevice);
                        ghostDevices[i].id = id;
                        id++;
                    }
                }
            }
            return deviceList;
        }
    }
}
