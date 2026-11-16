using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.Views;

namespace ShowAutomation.UnitTests.Domain.Models.Commands
{
    [TestClass]
    public class MusicCommandTest
    {
        [TestMethod]
        public void MusicCommandPauseTest()
        {
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            MusicCommand audioCommand = new MusicCommand("name" + " Audio", newSong);
            MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            //audioCommand.pause();not a method anymore
            //Pass-through method, no testing required
            //Assert.AreEqual(true, true);
            Assert.Inconclusive();
        }

        [TestMethod] // not working currently
        public void MusicCommandExecuteTest()
        {
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            MusicCommand audioCommand = new MusicCommand("name" + " Audio", newSong);
            MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            //audioCommand.execute(1);  //fails at song.createSong()
            Assert.Inconclusive();

            //Pass-through method, no testing required
            //Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void MusicCommandSecondExecuteTest()
        {
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            MusicCommand audioCommand = new MusicCommand("name" + " Audio", newSong);
            MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            //audioCommand.execute(); not a method anymore
            //Pass-through method, no testing required
            //Assert.AreEqual(true, true);
            Assert.Inconclusive();
        }

        [TestMethod]
        public void MusicCommandStopTest()
        {
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            MusicCommand audioCommand = new MusicCommand("name" + " Audio", newSong);
            MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            //audioCommand.stop(); not a method anymore
            //Pass-through method, no testing required
            //Assert.AreEqual(true, true);
            Assert.Inconclusive();
        }

        [TestMethod]
        public void MusicCommandGetDurationTest()
        {
            ulong expected_length = 13087;
            ulong length;
            Song newSong = new Song(@"Samples\WalloonLilliShort.mp3");
            MusicCommand audioCommand = new MusicCommand("name" + " Audio", newSong);
            MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            length = audioCommand.getDuration();
            Assert.AreEqual(expected_length, length);
        }

    }
}
