using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShowAutomation.UI.ViewModels.OutputDevices
{
    [Serializable()]
    public class OutputManager : ISerializable
    {
        public List<OutputDevice> outputList = new List<OutputDevice>();
        public List<OutputDevice> DisplayList
        {
            get
            {
                List<OutputDevice> display = new List<OutputDevice>();
                for (int i = 0; i < outputList.Count; i++) {
                    if (!outputList[i].isHidden && (Mode.isEditMode || !outputList[i].type.Equals(Types.GHOST)))
                    {
                        display.Add(outputList[i]);
                    }
                }
                return display;
            }
        }

        public Dictionary<String, OutputDevice> PreviewDevices { get; set; }
        public bool PreviewVideoOnPanel { set; get;}

        public OutputManager() {
            outputList = MainWindow.GetOutputDevices();
            PreviewDevices = new Dictionary<string, OutputDevice>();
            PreviewDevices.Add(Types.AUDIO, GetDefaultDevice(Types.AUDIO));
            PreviewDevices.Add(Types.MIDI, GetDefaultDevice(Types.MIDI));
            PreviewDevices.Add(Types.VIDEO, null);
            PreviewVideoOnPanel = true;
        }

        public OutputManager(SerializationInfo info, StreamingContext context)
        {
            outputList = (List<OutputDevice>)info.GetValue("outputs", typeof(List<OutputDevice>));
            try
            {
                PreviewDevices = (Dictionary<string, OutputDevice>)info.GetValue("previewDevices", typeof(Dictionary<string, OutputDevice>));
                PreviewVideoOnPanel = (bool)info.GetValue("videoOnPanel", typeof(bool));

                PreviewDevices[Types.AUDIO] = (PreviewDevices[Types.AUDIO] != null) ? getOutputByName(PreviewDevices[Types.AUDIO].deviceName) : null;
                PreviewDevices[Types.VIDEO] = (PreviewDevices[Types.VIDEO] != null) ? getOutputByName(PreviewDevices[Types.VIDEO].deviceName) : null;
                PreviewDevices[Types.MIDI] = (PreviewDevices[Types.MIDI] != null) ? getOutputByName(PreviewDevices[Types.MIDI].deviceName) : null;
            }
            catch
            {
                PreviewDevices = new Dictionary<string, OutputDevice>();
                PreviewDevices.Add(Types.AUDIO, GetDefaultDevice(Types.AUDIO));
                PreviewDevices.Add(Types.MIDI, GetDefaultDevice(Types.MIDI));
                PreviewDevices.Add(Types.VIDEO, null);
                PreviewVideoOnPanel = true;
            }
        }

        public void UpdateList()
        {
            List<OutputDevice> fullList = MainWindow.GetOutputDevices();
            for (int i = 0; i< fullList.Count; i++)
            {
                if (AddOutput(fullList[i])) {
                     UpdateList();
                    return;
               }
            }
            List<OutputDevice> list = new List<OutputDevice>();
            for (int i = 0; i < fullList.Count; i++) {
                for (int j = 0; j < outputList.Count; j++)
                {
                    if (fullList[i].deviceName.Equals(outputList[j].deviceName) && fullList[i].type.Equals(outputList[j].type))
                    {
                        list.Add(outputList[j]);
                    }
                }
            }
            List<OutputDevice> oldList = new List<OutputDevice>();
            oldList.AddRange(outputList);
            outputList = list;
            for (int j = 0; j < oldList.Count; j++)
            {
                if (!list.Contains(oldList[j]) && !oldList[j].type.Equals(Types.GHOST))
                {
                    MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(false, MainWindow.main.sceneList);
                    mapDevicesWindow.allOutputs = false;
                    mapDevicesWindow.cancelBtn.IsEnabled = false;
                    mapDevicesWindow.mapDevice(oldList[j]);
                }
            }
            MainWindow.main.UpdateMonitorTabs();
        }

        public void HideOutput(bool hide, OutputDevice device)
        {
            for (int j = 0; j < outputList.Count; j++)
            {
                if (outputList[j].deviceName.Equals(device.deviceName))
                {
                    outputList[j].isHidden = hide;
                }
            }
        }

        private bool AddOutput(OutputDevice device) {
            if (device != null && !outputList.Contains(device))
            {
                for (int j = 0; j < outputList.Count; j++)
                {
                    if (outputList[j].deviceName.Equals(device.deviceName) && outputList[j].type.Equals(device.type))
                    {
                        return false;
                    }
                }
                outputList.Add(device);
                if (!device.type.Equals(Types.GHOST) && !device.IsDuplicate)
                {
                    MapToNewDeviceWindow map = new MapToNewDeviceWindow(device, MainWindow.main.sceneList);
                    map.Owner = MainWindow.main;
                    if (map.devicesToMapListView.Items.Count > 0)
                    {
                        map.ShowDialog();
                    }
                    return true;
                }
            }
            return false;
        }

        internal OutputDevice getOutputByName(string deviceName)
        {
            foreach (OutputDevice device in outputList) {
                if (device.deviceName.Equals(deviceName)) return device;
            }
            return null;
        }

        public List<OutputDevice> getOutputs(String Type, bool ghost) {
            List<OutputDevice> list = new List<OutputDevice>();
            foreach (OutputDevice device in outputList) {
                if (device.type.Equals(Type) || (ghost && device.ghostType.Equals(Type) )) {
                    list.Add(device);
                }
            }

            return list;
        }

        public List<OutputDevice> GetDisplayOutputs(String Type, bool ghost)
        {
            List<OutputDevice> list = new List<OutputDevice>();
            foreach (OutputDevice device in DisplayList)
            {
                if (device.type.Equals(Type) || (ghost && device.ghostType.Equals(Type)))
                {
                    list.Add(device);
                }
            }

            return list;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("outputs", outputList);
            info.AddValue("previewDevices", PreviewDevices);
            info.AddValue("videoOnPanel", PreviewVideoOnPanel);
        }

        internal OutputDevice GetDefaultDevice(string type)
        {
            foreach (OutputDevice device in DisplayList)
            {
                if (device.type.Equals(type))
                {
                    return device;
                }
            }
            return null;
        }

        public OutputDevice GetPreviewDevice(String type) {
            if (PreviewVideoOnPanel && type.Equals(Types.VIDEO)) {
                return getOutputByName(MainWindow.main.getMainScreen().DeviceName);
            }
            return PreviewDevices[type];
        }
    }
}
