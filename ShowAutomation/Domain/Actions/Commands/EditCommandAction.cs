using System.Collections.Generic;
using System.Windows.Media;
using NAudio.Midi;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils;

namespace ShowAutomation.Domain.Actions
{
    internal class EditCommandAction : Action
    {
        private AbstractCommand command;
        private string name, oldName;
        private string filePath, oldFilePath;
        private Color midiColor, oldColor;
        private List<MidiEvent> list, oldList;

        public EditCommandAction(AbstractCommand command, string name, string filePath, Color midiColor, List<MidiEvent> list)
        {
            this.command = command;
            this.name = name;
            this.filePath = filePath;
            this.midiColor = midiColor;
            this.list = list;
            oldName = command.name;
            oldFilePath = (command.type.Equals(Types.VIDEO)) ? ((Video)command.media).videoPath : (command.type.Equals(Types.AUDIO) ? ((Song)command.media).songPath : null);
            oldColor = command.color;
            if (command.type.Equals(Types.MIDI))
            {
                oldList = (command as MidiCommand).MidiEvents;
            }
        }

        public override object GetFocus()
        {
            return command;
        }

        public override void Perform()
        {
            command.name = name;
            if (command.type.Equals(Types.VIDEO)) (command as VideoCommand).media = new Video(filePath);
            if (command.type.Equals(Types.AUDIO)) (command as MusicCommand).media = new Song(filePath);
            if (command.type.Equals(Types.MIDI)) (command as MidiCommand).MidiEvents = list;
            command.color = midiColor;

            //force redraw of command list in case of name change
            MainWindow.main.lvCommandList.DataContext = null;
            MainWindow.main.lvCommandList.DataContext = MainWindow.main.commandList;
        }

        public override string Title()
        {
            return "Edit Command " + command.name;
        }

        public override void Undo()
        {
            command.name = oldName;
            if (command.type.Equals(Types.VIDEO)) (command as VideoCommand).media = new Video(oldFilePath);
            if (command.type.Equals(Types.AUDIO)) (command as MusicCommand).media = new Song(oldFilePath);
            if (command.type.Equals(Types.MIDI)) (command as MidiCommand).MidiEvents = oldList;
            command.color = oldColor;
            
            //force redraw of command list in case of name change
            MainWindow.main.lvCommandList.DataContext = null;
            MainWindow.main.lvCommandList.DataContext = MainWindow.main.commandList;
        }
    }
}