using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class CreateGhostDeviceAction : Action
    {
        private OutputDevice device;

        public CreateGhostDeviceAction(OutputDevice device)
        {
            this.device = device;
        }

        public override object GetFocus()
        {
            return device;
        }

        public override void Perform()
        {
            GhostOutput.addDevice(device);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return "Create Ghost: "+ device.ghostType + " Device " + device.deviceName;
        }

        public override void Undo()
        {
            GhostOutput.removeDevice(device);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }
    }
}
