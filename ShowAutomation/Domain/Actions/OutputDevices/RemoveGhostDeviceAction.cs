using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class RemoveGhostDeviceAction : Action
    {
        private OutputDevice device;

        public RemoveGhostDeviceAction(OutputDevice device)
        {
            this.device = device;
        }

        public override object GetFocus()
        {
            return device;
        }

        public override void Perform()
        {
            GhostOutput.removeDevice(device);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return "Remove Ghost: " + device.ghostType + " Device " + device.deviceName;
        }

        public override void Undo()
        {
            GhostOutput.addDevice(device);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }
    }
}
