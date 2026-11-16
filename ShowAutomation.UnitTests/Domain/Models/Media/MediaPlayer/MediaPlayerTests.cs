using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer.Tests
{
    [TestClass()]
    public class MediaPlayerTests
    {
        public MediaPlayer Constructor(OutputDevice device)
        {
           // MediaPlayer target = new MediaPlayer();
           // return target; 
        }
        [TestMethod()]
        public void MediaPlayerTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void MuteTest()
        {
            Assert.Fail();
        }
    }
}

namespace ShowAutomation.UnitTests.Domain.Models.Media.MediaPlayer
{
    [TestClass]
    public class MediaPlayerTests
    {
        [TestMethod]
        public void MuteTest()
        {
            bool expected_muted = true;
            bool mute = true;
            MidiEvent @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, new Scene());

            //@event.getPlayer().Mute(mute);

        }
    }


    
}
