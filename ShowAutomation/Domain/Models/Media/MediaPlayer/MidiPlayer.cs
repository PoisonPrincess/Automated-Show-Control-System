using System;
using System.Timers;
using NAudio.Midi;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.Domain.Utils.Midi;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer
{
    public class MidiPlayer : MediaPlayer
    {

        private MidiOut midiOut;
        private MidiDeviceManager midiDeviceManager = MidiDeviceManager.Instance;

        public MidiPlayer(Events.MidiEvent @event) : base(@event.OutputDevice)
        {
            ev = @event;
            timer.Elapsed += FinishedPlaying;
        }

        private void FinishedPlaying(object sender, ElapsedEventArgs e)
        {
            ev.isPlaying = false;
            MediaEnded();
        }

        protected override void LoadAt(ulong time)
        {
            timer.Interval = ev.duration;
        }

        protected override void Pause()
        {
            ev.isPlaying = false;
        }

        protected override void Play()
        {
            ev.isPlaying = true;
            if (MainWindow.main.outputs.getOutputs(Types.MIDI, true).Count > device.id)
            {
                midiOut = ((MidiDevice)device).device;
                if (midiOut == null)
                {
                    midiOut = ((MidiDevice)device).device;
                }
                Console.WriteLine("Set MidiOut to device: " + midiOut + " from device " + MainWindow.main.outputs.getOutputs(Types.MIDI, true)[device.id].deviceName);
            }
            timer.Start();
            if (midiOut == null)
            {
                Console.WriteLine("Set MidiOut to default: " + midiOut);
                midiOut = ((MidiDevice)MainWindow.main.outputs.GetPreviewDevice(Types.MIDI)).device;
            }

            foreach (NAudio.Midi.MidiEvent midiEvent in (ev.command as MidiCommand).MidiEvents)
            {
                try
                {
                    Console.WriteLine(string.Format("Sent {0}", midiEvent));
                    Console.WriteLine("MIDI Out: " + midiOut);
                    midiOut.Send(midiEvent.GetAsShortMessage());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error:" + e);
                }

            }
        }

        protected override void Stop()
        {
            timer.Stop();
            ev.isPlaying = false;
        }

        protected override void Fade(ulong time, float percent)
        {
            //MIDI Events don't have duration and don't need to fade
        }
    }
}
