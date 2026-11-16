using System.Windows.Controls;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;

namespace ShowAutomation.Domain.Actions
{
    class SoloLaneAction : Action
    {
        private OutputDevice device;
        private bool solo;
        private Button button;

        public SoloLaneAction(OutputDevice device, bool solo, Button button)
        {
            this.device = device;
            this.solo = solo;
            this.button = button;
        }

        public override object GetFocus()
        {
            return device;
        }

        public override void Perform()
        {
            Action(solo);
        }

        public override string Title()
        {
            return ((solo) ? "Solo" : "Unsolo") + " " + device.type + " Lane " + device.deviceName;
        }

        public override void Undo()
        {
            Action(!solo);
        }

        private void Action(bool solo) {
            if (device.getDeviceMuteBySolo())
            {
                device.setDeviceMuteBySolo(false);
            }
            device.setDeviceSolo(solo);
            if (device.getDeviceSolo())
            {
                button.Content = "Unsolo";
                foreach (OutputDevice deviceToMute in MainWindow.main.outputs.getOutputs(device.type, false))
                {
                    if (!deviceToMute.getDeviceSolo())
                    {
                        deviceToMute.setDeviceMuteBySolo(true);
                    }
                }
                if (device.type == "Audio")
                {
                    foreach (OutputDevice deviceToMute in MainWindow.main.outputs.getOutputs(Types.VIDEO, false))
                    {
                        (deviceToMute as VideoDevice).muteVideo(true);
                    }
                }
            }
            else
            {
                button.Content = "Solo";
                bool anySolo = false;
                foreach (OutputDevice deviceToCheck in MainWindow.main.outputs.getOutputs(device.type, false))
                {
                    anySolo = deviceToCheck.getDeviceSolo();
                    if (anySolo) break;
                }
                if (!anySolo)
                {
                    foreach (OutputDevice deviceToMute in MainWindow.main.outputs.getOutputs(device.type, false))
                    {
                        deviceToMute.setDeviceMuteBySolo(false);
                    }
                }
                else
                {
                    device.setDeviceMuteBySolo(true);
                }
                if (device.type == "Audio")
                {
                    foreach (OutputDevice deviceToMute in MainWindow.main.outputs.getOutputs(Types.VIDEO, false))
                    {
                        foreach (Event vidEvent in MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].getEventsByOutputDevice(deviceToMute))
                        {
                            (deviceToMute as VideoDevice).muteVideo(false);
                        }
                    }
                }
            }
        }
    }
}
