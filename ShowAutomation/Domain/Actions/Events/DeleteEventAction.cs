using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;

namespace ShowAutomation.Domain.Actions
{
    class DeleteEventAction : Action
    {
        private Event ev;
        private Scene scene;

        public DeleteEventAction(Event ev, Scene scene)
        {
            this.ev = ev;
            this.scene = scene;
        }

        public override object GetFocus()
        {
            return ev;
        }

        public override void Perform()
        {
            scene.removeEvent(ev);
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return "Delete Event " + ev.name;
        }

        public override void Undo()
        {
            scene.addEvent(ev);
            MainWindow.main.DrawTimeline();
        }
    }
}
