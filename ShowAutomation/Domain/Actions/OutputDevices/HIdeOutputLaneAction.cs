using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{ 
    class HideOutputLaneAction : Action
    {
        private OutputDevice outputDevice;
        private bool value;

        public HideOutputLaneAction(OutputDevice outputDevice, bool value)
        {
            this.outputDevice = outputDevice;
            this.value = value;
        }

        public override object GetFocus()
        {
            return outputDevice;
        }

        public override void Perform()
        {
            outputDevice.Hidden = value;
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return ((value) ? "Hide " : "Show ") + outputDevice.type + " Device " + outputDevice.deviceName;
        }

        public override void Undo()
        {
            outputDevice.Hidden = !value;
            MainWindow.main.lvOutputList.ItemsSource = null;
            MainWindow.main.lvOutputList.ItemsSource = MainWindow.main.outputs.DisplayList;
            MainWindow.main.DrawTimeline();
        }
    }
}
