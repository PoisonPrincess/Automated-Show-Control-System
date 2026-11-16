using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowAutomation.Domain.Models.Events
{
    [Serializable()]
    public class MidiEvent : Event, ISerializable
    {
        public MidiEvent(MidiCommand command, ulong startTime, Scene scene) : base(command, startTime, scene)
        {
            this.type = Types.MIDI;
            player = new MidiPlayer(this);

        }

        public MidiEvent(MidiCommand command, ulong startTime, Scene scene, MidiDevice device) : this (command, startTime, scene)
        {
            this.OutputDevice = device;
        }

        public MidiEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            player = new MidiPlayer(this);

        }

        public void setCommand(MidiCommand command)
        {
            this.command = command;
            this.name = command.name;
        }

        public override object ToJsonInstance()
        {
            return new JsonMidiEvent(this);
        }

        private static string GetColorName(System.Windows.Media.Color col)
        {
            System.Reflection.PropertyInfo colorProperty = typeof(Colors).GetProperties()
                .FirstOrDefault(p => System.Windows.Media.Color.AreClose((System.Windows.Media.Color)p.GetValue(null), col));
            return colorProperty != null ? colorProperty.Name : "LightBlue";
        }
        protected override void AddEventSpecificContextMenuItems(ContextMenu contextMenu)
        {
        }

        public override object Clone()
        {
            MidiEvent clone = new MidiEvent((MidiCommand)command, startTime, null, (MidiDevice)device);
            clone.group = group;
            return clone;
        }
    }

    public class JsonMidiEvent : JsonEvent
    {
        public dynamic outputDevice;
        public JsonMidiEvent(MidiEvent ev) : base(ev)
        {
            this.command = ev.command.ToJsonInstance();
            this.outputDevice = ev.OutputDevice.ToJsonInstance();
        }
    }
}
