using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using System;

namespace ShowAutomation.Domain.Actions
{
    internal class EditEventAction : Action
    {
        private Event eventToEdit;
        private AbstractCommand command, oldCommand;
        private OutputDevice device, oldDevice;
        private ulong time, oldTime;
        private string label;

        public EditEventAction(Event eventToEdit, AbstractCommand command, OutputDevice device, ulong time)
        {
            this.eventToEdit = eventToEdit;
            this.command = command;
            this.device = device;
            this.time = time;
            oldCommand = eventToEdit.command;
            oldDevice = eventToEdit.OutputDevice;
            oldTime = eventToEdit.startTime;
            label = "Edit "+ eventToEdit.type +" Event " + eventToEdit.name;
        }

        public EditEventAction(Event eventToEdit, OutputDevice device, ulong time) : this(eventToEdit, eventToEdit.command, device, time) {}
        public EditEventAction(Event eventToEdit, ulong time) : this(eventToEdit, eventToEdit.command, eventToEdit.OutputDevice, time) {
            label = "Change Start Time of " + eventToEdit.type + " Event " + eventToEdit.name;
        }

        public override object GetFocus()
        {
            return eventToEdit;
        }

        public override void Perform()
        {
            Action(command, device, time);
        }

        public override string Title()
        {
            return label;
        }

        public override void Undo()
        {
            Action(oldCommand, oldDevice, oldTime);
        }

        private void Action(AbstractCommand command, OutputDevice device, ulong time)
        {
            EventGroup group = eventToEdit.group;
            ulong startTime;

            startTime = time;

            String type = eventToEdit.type;

            if (type == Types.AUDIO)
            {
                eventToEdit.setCommand((MusicCommand)command);
                eventToEdit.OutputDevice = device as AudioDevice;
            }
            else if (type == Types.MIDI)
            {
                eventToEdit.setCommand((MidiCommand)command);
                eventToEdit.OutputDevice = device as MidiDevice;
            }
            else if (type == Types.VIDEO)
            {
                eventToEdit.setCommand((VideoCommand)command);
                eventToEdit.OutputDevice = device as VideoDevice;
            }
            if (group != null)
            {
                long timeDiff = (long)startTime - (long)eventToEdit.startTime;
                group.updateTimesBy(timeDiff);
            }

            eventToEdit.setStartTime(startTime);
            eventToEdit.getScene().setDuration();
            MainWindow.main.DrawTimeline();

            MainWindow.main.lvSceneList.DataContext = null;
            MainWindow.main.lvSceneList.DataContext = MainWindow.main.sceneList;
        }
    }
}