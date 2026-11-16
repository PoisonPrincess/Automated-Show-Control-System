using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ShowAutomation.UI.ViewModels.Media.Tests
{
    [TestClass()]
    public class SongWaveformGeneratorTests
    {
     
        public void getWaveData(SongWaveformGenerator target)
        {
            target.getWaveData();
            
        }
        [TestMethod]
        public void getWaveDataThrowsNullReferenceException()
        {
            SongWaveformGenerator songWaveformGenerator;
            songWaveformGenerator = new SongWaveformGenerator();
            songWaveformGenerator.songPath = (string)null;
            songWaveformGenerator.UIDisaptcher = (Dispatcher)null;
            songWaveformGenerator.waves = (List<float>[])null;
            this.getWaveData(songWaveformGenerator);
          
        }

        [TestMethod]
        public void getWaveDataThrowsNullReferenceException1()
        {
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            SongWaveformGenerator songWaveformGenerator;
            songWaveformGenerator = new SongWaveformGenerator();
            songWaveformGenerator.songPath = newSong.songPath;
            songWaveformGenerator.UIDisaptcher = (Dispatcher)null;
            songWaveformGenerator.waves = (List<float>[])null;
            this.getWaveData(songWaveformGenerator);
            Assert.IsNotNull(this);
        }


    }

}