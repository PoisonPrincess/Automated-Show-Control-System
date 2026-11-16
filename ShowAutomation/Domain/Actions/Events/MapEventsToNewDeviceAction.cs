using System;
using System.Collections.Generic;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class MapEventsToNewDeviceAction : Action
    {
        private List<Event> events;
        private OutputDevice originalDevice;
        private OutputDevice newDevice;

        public MapEventsToNewDeviceAction(List<Event> events, OutputDevice originalDevice, OutputDevice newDevice)
        {
            this.events = events;
            this.originalDevice = originalDevice;
            this.newDevice = newDevice;
        }

        public override object GetFocus()
        {
            return originalDevice;
        }

        public override void Perform()
        {
            Action(newDevice);
        }

        public override string Title()
        {
            return "Map Events from " + originalDevice.deviceName + " to " + newDevice.deviceName;
        }

        public override void Undo()
        {
            Action(originalDevice);
        }

        private void Action(OutputDevice newDevice) {
            Console.WriteLine("Map events to " + newDevice.deviceName);
            foreach (Event ev in events)
            {
                Console.WriteLine("Map events " + ev.name);
                ev.OutputDevice = newDevice;
            }

            MainWindow.main.DrawTimeline();

            MainWindow.main.lvSceneList.DataContext = null;
            MainWindow.main.lvSceneList.DataContext = MainWindow.main.sceneList;
        }
    }
}
