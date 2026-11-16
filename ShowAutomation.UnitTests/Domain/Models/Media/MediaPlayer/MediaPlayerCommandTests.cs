using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer.Tests
{
    // Test how Media Player Command Class plays around with the system.
    [TestClass()]
    public class MediaPlayerCommandTests
    {
       
        public MediaPlayerCommand Constructor(String type, ulong time, float percent)
        {
            MediaPlayerCommand target = new MediaPlayerCommand(type, time, percent);
            return target;
        }

        public MediaPlayerCommand Constructor(String type)
        {
            MediaPlayerCommand target = new MediaPlayerCommand(type);
            return target;
        }
        public MediaPlayerCommand Constructor(String type, ulong time)
        {
            float percent = 0.1f;
            MediaPlayerCommand target = new MediaPlayerCommand(type, time, percent);
            return target;
        }

        [TestMethod()]
        public void MediaPlayerCommandTest1()
        {
            MediaPlayerCommand mediaPlayerCommand;
            mediaPlayerCommand = this.Constructor((string)null, 0uL, (float)0);
            Assert.IsNotNull((object)mediaPlayerCommand);
            Assert.AreEqual<string>((string)null, mediaPlayerCommand.type);
            Assert.AreEqual<ulong>(0uL, mediaPlayerCommand.time);
            Assert.AreEqual<float>((float)0, mediaPlayerCommand.percent);
        }

        [TestMethod]
        public void MediaPlayerCommandTest2()
        {
            MediaPlayerCommand mediaPlayerCommand;
            mediaPlayerCommand = this.Constructor((String)null);
            Assert.IsNotNull((object)mediaPlayerCommand);
            Assert.AreEqual<string>((string)null, mediaPlayerCommand.type);
            Assert.AreEqual<ulong>(0uL, mediaPlayerCommand.time);
            Assert.AreEqual<float>((float)0, mediaPlayerCommand.percent);
        }

        [TestMethod]
        public void MediaPlayerCommandTest3()
        {
            MediaPlayerCommand mediaPlayerCommand;
            mediaPlayerCommand = this.Constructor((String)null,0uL);
            Assert.IsNotNull((object)mediaPlayerCommand);
            Assert.AreEqual<string>((string)null, mediaPlayerCommand.type);
            Assert.AreEqual<ulong>(0uL, mediaPlayerCommand.time);
            Assert.AreEqual<float>((float)0, mediaPlayerCommand.percent);
        }
      
    }
}