using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class DuplicateOutputActioncs : Action
    {
        private OutputDevice original;
        private OutputDevice duplicate;

        public DuplicateOutputActioncs(OutputDevice test)
        {
            original = test;
            duplicate = test.CreateDuplicate();
        }

        public override object GetFocus()
        {
            return original;
        }

        public override void Perform()
        {
            original.duplicates.Add(duplicate);
            LaneOutput.addDevice(duplicate);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.UpdateOutputDevices();
        }

        public override string Title()
        {
            return "Duplicate " +original.type+ " Lane " + original.deviceName;
        }

        public override void Undo()
        {
            original.duplicates.Remove(duplicate);
            LaneOutput.removeDevice(duplicate);
            MainWindow.main.outputs.UpdateList();
            MainWindow.main.UpdateOutputDevices();
        }
    }
}
