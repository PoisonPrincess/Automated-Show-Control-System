using ShowAutomation.Domain.Models.Events;

namespace ShowAutomation.Domain.Actions
{
    class CreateGroupAction : Action
    {
        private EventGroup group;

        public CreateGroupAction(EventGroup group)
        {
            this.group = group;
        }

        public override object GetFocus()
        {
            return group;
        }

        public override void Perform()
        {
            foreach (Event ev in group.GetElements())
            {
                ev.group = group;
                ev.Select(true);
            }
        }

        public override string Title()
        {
            return "Group Events";
        }

        public override void Undo()
        {
            foreach (Event ev in group.GetElements())
            {
                ev.Ungroup();
                ev.Select(false);
            }
            //force regroup of items that stay in groups after ungrouping
            foreach (Event ev in group.GetElements())
            {
                if (ev.group != null)
                {
                    for (int j = 0; j < ev.group.GetElements().Count; j++)
                    {
                        ev.group.GetElements()[j].group = group;
                        ev.group.GetElements()[j].Select(true);
                    }
                }
            }
        }
    }
}
