using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;
using System;
using System.Runtime.Serialization;
using System.Timers;

namespace ShowAutomation.Domain.Utils
{
    [Serializable()]
    public class SceneFadeSettings : ISerializable
    {
        protected Timer timer = new Timer();
        private ulong duration;
        public ulong Duration { get { return duration; } set { duration = value; timer.Interval = value + 250; } }
        public MidiEvent StartMidiEvent { get; set; }
        public MidiEvent EndMidiEvent { get; set; }

        private Scene scene;

        
        public SceneFadeSettings() {
            Duration = 3000;
            StartMidiEvent = null;
            EndMidiEvent = null;
            timer.Elapsed += EndOfFade;
        }

        public void StartFadeOnScene(Scene scene) {
            this.scene = scene;
            timer.Start();
            Console.WriteLine("Start Fade Scene");
        }

        private void EndOfFade(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Stop Scene");
            timer.Stop();
            scene.EndOfFade();
        }

        public SceneFadeSettings(SerializationInfo info, StreamingContext context)
        {
            Duration = (ulong)info.GetValue("duration", typeof(ulong));
            StartMidiEvent = (MidiEvent)info.GetValue("startMidiEvent", typeof(MidiEvent));
            EndMidiEvent = (MidiEvent)info.GetValue("endMidiEvent", typeof(MidiEvent));
            timer.Elapsed += EndOfFade;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("duration", Duration);
            info.AddValue("startMidiEvent", StartMidiEvent);
            info.AddValue("endMidiEvent", EndMidiEvent);
        }
    }
}
