using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Scenes;
using Microsoft.Win32;

namespace ShowAutomation.UnitTests.Domain.Models.Events
{
    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public void EventPauseTest()
        {
            bool expected_isPlaying = false;

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Viewo files | *.avi; *.mp4; *.mpeg; *.wmv;";

            Event @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, new Scene());

            Assert.AreEqual(expected_isPlaying, @event.isPlaying);
        }
    }
}
