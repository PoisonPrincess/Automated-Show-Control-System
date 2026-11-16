using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class RemoveDuplicateOutputAction : Action
    {
        private OutputDevice original;
        private OutputDevice duplicate;

        public RemoveDuplicateOutputAction(OutputDevice outputDevice)
        {
            this.duplicate = outputDevice;
            this.original = outputDevice.original;
        }

        public override object GetFocus()
        {
            return duplicate;
        }

        public override void Perform()
        {
            duplicate.Remove();
            LaneOutput.removeDevice(duplicate);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.UpdateOutputDevices();
        }

        public override string Title()
        {
            return "Remove Duplicate " + duplicate.type + " Lane " + duplicate.deviceName;
        }

        public override void Undo()
        {
            original.duplicates.Add(duplicate);
            LaneOutput.addDevice(duplicate);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.UpdateOutputDevices();
        }
    }
}
