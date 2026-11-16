using ShowAutomation.Domain.Models.Events;

namespace ShowAutomation.Domain.Actions
{
    // UngroupAction class for testing purposes
    public class UngroupActionTest : Action
    {
        private Event @event;
        private EventGroup group;
        private DissolveGroupAction DissolveGroup;
        public UngroupActionTest(Event @event, EventGroup group)
        {
            this.@event = @event;
            this.group = group;
            DissolveGroup = new DissolveGroupAction(group);
        }
        public override object GetFocus()
        {
           return GetFocus();
        }

        public override void Perform()
        {
            Perform();
        }

        public override string Title()
        {
            return Title();
        }

        public override void Undo()
        {
            Undo();
        }
    }

    class UngroupAction : Action
    {
        private Event @event;
        private EventGroup group;
        private DissolveGroupAction DissolveGroup;

        public UngroupAction(Event @event, EventGroup group)
        {
            this.@event = @event;
            this.group = group;
            DissolveGroup = new DissolveGroupAction(group);
        }

        public override object GetFocus()
        {
            return @event;
        }

        public override void Perform()
        {
            if (group.GetElements().Count > 2)
            {
                group.Remove(@event);
                @event.group = null;
                foreach (Event ev in group.GetElements())
                {
                    ev.Select(false);
                }
            }
            else
            {
                DissolveGroup.Perform();
            }
        }

        public override string Title()
        {
            return "Ungroup Event " + @event.name;
        }

        public override void Undo()
        {
            if (group.GetElements().Count > 1)
            {
                group.Add(@event);
                @event.group = group;
                foreach (Event ev in group.GetElements())
                {
                    ev.Select(true);
                }
            }
            else
            {
                DissolveGroup.Undo();
            }
        }
    }
}
