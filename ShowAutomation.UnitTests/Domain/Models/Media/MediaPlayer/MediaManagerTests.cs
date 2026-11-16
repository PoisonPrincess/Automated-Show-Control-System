using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Media.Tests
{
    [TestClass()]
    public class MediaManagerTests
    {
       

        // Media Manager Constructor
        public MediaManager Constructor(OutputDevice device)
        {
            //MediaManager target = new MediaPlayer(device); ;
            //return target;
        }

        public void PlayEvent(
          MediaManager target,
           global::ShowAutomation.Domain.Models.Media.MediaPlayer.MediaPlayer media,
           ulong eventTime
       )
        {
            target.PlayEvent(media, eventTime);
           
        }

        // test media manager's constructor
        [TestMethod]
        public void ConstructorTest()
        {
            MediaManager mediaManager;
            mediaManager = this.Constructor((OutputDevice)null);
            Assert.IsNotNull((object)mediaManager);
        }


        [TestMethod]
        public void PlayEventTest()
        {
            MediaManager mediaManager;
            mediaManager = new MediaManager((OutputDevice)null);
            this.PlayEvent(mediaManager,
                           (global::ShowAutomation.Domain.Models.Media.MediaPlayer.MediaPlayer)null,
                           0uL);
            Assert.IsNotNull((object)mediaManager);
        }
    }
}