using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;

namespace ShowAutomation.Domain.Actions
{
    internal class NewEventAction : Action
    {
        private Event newEvent;
        private Scene scene;

        public NewEventAction(Event newEvent, Scene scene)
        {
            this.newEvent = newEvent;
            this.scene = scene;
        }

        public override object GetFocus()
        {
            return newEvent;
        }

        public override void Perform()
        {
            scene.addEvent(newEvent);
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return "Add Event " + newEvent.name;
        }

        public override void Undo()
        {
            scene.removeEvent(newEvent);
            MainWindow.main.DrawTimeline();
        }
    }
}