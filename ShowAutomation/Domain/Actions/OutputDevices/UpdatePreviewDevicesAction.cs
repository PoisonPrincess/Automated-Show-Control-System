using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.OutputDevices;

namespace ShowAutomation.Domain.Actions
{
    class UpdatePreviewDevicesAction : Action
    {
        private AudioDevice audioDevice;
        private MidiDevice midiDevice;
        private VideoDevice videoDevice;
        private AudioDevice oldAudioDevice;
        private MidiDevice oldMidiDevice;
        private VideoDevice oldVideoDevice;
        private bool isChecked;
        private bool oldIsChecked;
        private OutputManager outputs;

        public UpdatePreviewDevicesAction(AudioDevice audioDevice, MidiDevice midiDevice, VideoDevice videoDevice, bool isChecked, OutputManager outputs)
        {
            this.audioDevice = audioDevice;
            this.midiDevice = midiDevice;
            this.videoDevice = videoDevice;
            this.isChecked = isChecked;
            this.outputs = outputs;
            oldAudioDevice = (AudioDevice)outputs.PreviewDevices[Types.AUDIO];
            oldMidiDevice = (MidiDevice)outputs.PreviewDevices[Types.MIDI];
            oldVideoDevice = (VideoDevice)outputs.PreviewDevices[Types.VIDEO];
            oldIsChecked = outputs.PreviewVideoOnPanel;
        }

        public override object GetFocus()
        {
           return outputs;
        }

        public override void Perform()
        {
            Action(audioDevice, midiDevice, videoDevice, isChecked);
        }

        public override string Title()
        {
            return "Change Preview Devices";
        }

        public override void Undo()
        {
            Action(oldAudioDevice, oldMidiDevice, oldVideoDevice, oldIsChecked);
        }

        private void Action(AudioDevice audio, MidiDevice midi, VideoDevice video, bool docked)
        {
            outputs.PreviewDevices[Types.AUDIO] = audio;
            outputs.PreviewDevices[Types.MIDI] = midi;
            outputs.PreviewDevices[Types.VIDEO] = video;
            outputs.PreviewVideoOnPanel = docked;
        }
    }
}
