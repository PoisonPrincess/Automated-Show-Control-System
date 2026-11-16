using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;

namespace ShowAutomation.UI.ViewModels.Commands.Tests
{
    [TestClass()]
    public class CommandListTests
    {
        [TestMethod()]
        //Test method that tests if there is still a command when it is deleted.
        public void DeleteItemTest()
        {
            CommandList list = new CommandList();
            AbstractCommand midiCommand = new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode()));

            list.AddItem(midiCommand);
            list.DeleteItem(midiCommand);
            Assert.AreEqual(0, list.Commands.Count);
        }

        [TestMethod()]
        //Test Method that test whether or not a new command was added.
        public void AdditemTest()
        {
            CommandList list = new CommandList();
            AbstractCommand midiCommand = new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode()));

            list.AddItem(midiCommand);
            Assert.AreEqual(1, list.Commands.Count);

        }
        [TestMethod()]
        //Method to check what type of command you have
        public void getCommandTypeTest()
        {
            CommandList list = new CommandList();
            AbstractCommand midiCommand = new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode()));
            list.AddItem(midiCommand);
            Assert.AreEqual(midiCommand.type,"MIDI");
        }
    }
}