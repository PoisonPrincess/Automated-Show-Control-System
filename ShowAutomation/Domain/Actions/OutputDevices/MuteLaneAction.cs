using System.Windows.Controls;
using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class MuteLaneAction : Action
    {
        private OutputDevice device;
        private bool mute;
        private Button button;

        public MuteLaneAction(OutputDevice device, bool mute, Button button)
        {
            this.device = device;
            this.mute = mute;
            this.button = button;
        }

        public override object GetFocus()
        {
            return device;
        }

        public override void Perform()
        {
            device.setDeviceMute(mute);
            if (device.getDeviceMute())
            {
                button.Content = "Unmute";
                MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].GreyOutLane(MainWindow.main.outputs.DisplayList.IndexOf(device), true);
            }
            else
            {
                button.Content = "Mute";
                MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].GreyOutLane(MainWindow.main.outputs.DisplayList.IndexOf(device), false);
            }
        }

        public override string Title()
        {
            return ((mute) ? "Mute" : "Unmute") + " " + device.type + " Lane " + device.deviceName;
        }

        public override void Undo()
        {
            device.setDeviceMute(!mute);
            if (device.getDeviceMute())
            {
                button.Content = "Unmute";
                MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].GreyOutLane(MainWindow.main.outputs.DisplayList.IndexOf(device), true);
            }
            else
            {
                button.Content = "Mute";
                MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].GreyOutLane(MainWindow.main.outputs.DisplayList.IndexOf(device), false);
            }
        }
    }
}
