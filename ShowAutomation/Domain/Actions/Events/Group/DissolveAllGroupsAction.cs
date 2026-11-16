using System.Collections.Generic;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.UI.ViewModels.Scenes;

namespace ShowAutomation.Domain.Actions
{
    class DissolveAllGroupsAction : Action
    {
        private SceneList sceneList;
        private List<EventGroup> groups = new List<EventGroup>();

        public DissolveAllGroupsAction(SceneList sceneList)
        {
            this.sceneList = sceneList;
            if (sceneList != null)
            {
                foreach (Scene scene in sceneList.Scenes)
                {
                    foreach (Event @event in scene.Events)
                    {
                        if (@event.group != null && !groups.Contains(@event.group)) {
                            groups.Add(@event.group);
                        }
                    }
                }
            }
        }

        public override object GetFocus()
        {
            return sceneList;
        }

        public override void Perform()
        {
            if (sceneList != null)
            {
                foreach (Scene scene in sceneList.Scenes)
                {
                    foreach (Event @event in scene.Events)
                    {
                        @event.Ungroup();
                        @event.Select(false);
                    }
                }
            }
        }

        public override string Title()
        {
            return "Ungroup All Events";
        }

        public override void Undo()
        {
            foreach (EventGroup group in groups) {
                bool gSelected = false;
                foreach (Event ev in group.GetElements())
                {
                    if (ev.IsSelected()) {
                        gSelected = true;
                    }
                }
                foreach (Event ev in group.GetElements())
                {
                    ev.group = group;
                    ev.Select(gSelected);
                }
            }
        }
    }
}
