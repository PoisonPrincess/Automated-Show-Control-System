using System;
using System.Collections.Generic;
using NAudio.Midi;
using ShowAutomation.Domain.Utils;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ShowAutomation.Domain.Models.Commands
{/// <summary>
 /// MIDI inherits from Abstract command 
 /// </summary>

    [Serializable()]
    public class MidiCommand: AbstractCommand, ISerializable
    {
        // Public Properties
        public List<MidiEvent> MidiEvents { get; set; }
        /// <summary>
        /// Sets preferd midi duration , This sets size that midi will appear on timeLine 
        /// </summary>
        /// <returns></returns>
        public override ulong getDuration()
        {
            return 200;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get
            {
                var description = "MIDI";
                foreach (MidiEvent midiEvent in MidiEvents)
                {
                    description += " " + midiEvent.ToString();
                }
                return description;
            }
        }

        /// <summary>
        /// Constructer that takes one midi event as a parameter 
        /// </summary>
        /// <param name="midiEvent"></param>
        public MidiCommand(MidiEvent midiEvent)
        {
            MidiEvents = new List<MidiEvent>();
            MidiEvents.Add(midiEvent);
            type = Types.MIDI;
            id = Guid.NewGuid();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvents"></param>
        public MidiCommand(List<MidiEvent> midiEvents)
        {
            MidiEvents = midiEvents;
            type = Types.MIDI;
            id = Guid.NewGuid();
        }
        public MidiCommand(SerializationInfo info, StreamingContext context)
        {
            name = (String)info.GetValue("name", typeof(String));
            List<int> midi = (List<int>)info.GetValue("midi", typeof(List<int>));
            MidiEvents = new List<MidiEvent>();
            foreach (int midiEvent in midi)
            {
                MidiEvents.Add(MidiEvent.FromRawMessage(midiEvent));
            }
            type = Types.MIDI;
            id = (Guid)info.GetValue("id", typeof(Guid));
            try
            {
                color = ((SavedColor)info.GetValue("color", typeof(SavedColor))).getColor();
            }
            catch (SerializationException e) {
                color = Colors.LightBlue;
            }
        }
        /// <summary>
        /// Constructer that takes two parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="midiEvents"></param>
        public MidiCommand(string name, List<MidiEvent> midiEvents)
        {
            this.name = name;
            MidiEvents = midiEvents;
            type = Types.MIDI;
            id = Guid.NewGuid();
            this.color = Colors.LightBlue; //sets color to default if loaded from old save format
        }

        /// <summary>
        /// Uses to save midi commands to Json
        /// </summary>
        /// <returns></returns>
        public override object ToJsonInstance()
        {
            return new JsonMidiCommand(this);
        }
  
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
            List<int> midi = new List<int>();
            foreach (MidiEvent midiEvent in MidiEvents)
            {
                midi.Add(midiEvent.GetAsShortMessage());
            }
            info.AddValue("midi", midi);
            info.AddValue("id", id);
            info.AddValue("color", new SavedColor(color));
        }
    }
    /// <summary>
    /// Save Midi information to Json
    /// </summary>
    public class JsonMidiCommand : JsonCommand
    {
        public List<int> MidiEvents = new List<int>();

        public JsonMidiCommand (MidiCommand command) : base(command)
        {
            foreach (MidiEvent midiEvent in command.MidiEvents)
            {
                MidiEvents.Add(midiEvent.GetAsShortMessage());
            }
        }
    }
}
