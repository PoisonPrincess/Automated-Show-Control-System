using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;

namespace ShowAutomation.Domain.Actions
{
    class UpdateSceneFadeSettingsAction : Action
    {
        private SceneFadeSettings settings;
        private ulong duration;
        private MidiCommand startMidiCommand;
        private MidiDevice startMidiDevice;
        private MidiCommand endMidiCommand;
        private MidiDevice endMidiDevice;

        private ulong oldDuration;
        private MidiCommand oldStartMidiCommand;
        private MidiDevice oldStartMidiDevice;
        private MidiCommand oldEndMidiCommand;
        private MidiDevice oldEndMidiDevice;

        public UpdateSceneFadeSettingsAction(SceneFadeSettings settings, ulong duration, MidiCommand midiCommand1, MidiDevice midiDevice1, MidiCommand midiCommand2, MidiDevice midiDevice2)
        {
            this.settings = settings;
            this.duration = duration;
            this.startMidiCommand = midiCommand1;
            this.startMidiDevice = midiDevice1;
            this.endMidiCommand = midiCommand2;
            this.endMidiDevice = midiDevice2;

            oldDuration = settings.Duration;
            oldStartMidiCommand = (settings.StartMidiEvent != null) ? settings.StartMidiEvent.command as MidiCommand : null;
            oldStartMidiDevice = (settings.StartMidiEvent != null) ? settings.StartMidiEvent.OutputDevice as MidiDevice : null;
            oldEndMidiCommand = (settings.EndMidiEvent != null) ? settings.EndMidiEvent.command as MidiCommand : null;
            oldEndMidiDevice = (settings.EndMidiEvent != null) ? settings.EndMidiEvent.OutputDevice as MidiDevice : null;
        }

        public override object GetFocus()
        {
            return settings;
        }

        public override void Perform()
        {
            Action(duration, startMidiCommand, startMidiDevice, endMidiCommand, endMidiDevice);
        }

        public override string Title()
        {
            return "Edit Scene Fade Out Settings";
        }

        public override void Undo()
        {
            Action(oldDuration, oldStartMidiCommand, oldStartMidiDevice, oldEndMidiCommand, oldEndMidiDevice);
        }

        private void Action(ulong duration, MidiCommand startMidiCommand, MidiDevice startMidiDevice, MidiCommand endMidiCommand, MidiDevice endMidiDevice) {
            settings.Duration = duration;
            settings.StartMidiEvent = null;
            settings.EndMidiEvent = null;
            if (startMidiCommand != null)
            {
                settings.StartMidiEvent = new MidiEvent(startMidiCommand, 0, null, startMidiDevice);
            }
            if (endMidiCommand != null)
            {
                settings.EndMidiEvent = new MidiEvent(endMidiCommand, 0, null, endMidiDevice);
            }
            MainWindow.main.sceneList.fadeSettings = settings;
        }
    }
}
