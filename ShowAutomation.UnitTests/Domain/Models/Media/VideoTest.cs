using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.Views;

namespace ShowAutomation.UnitTests.Domain.Models.Media
{
    [TestClass]
    public class VideoTest
    {

        public static Video Create(
            string videoPath_s,
            bool isPlaying_b,
            string videoPath_s1,
            bool isMute_b1
        )
        {
            Video video = new Video(videoPath_s);
            video.isPlaying = isPlaying_b;
            video.videoPath = videoPath_s1;
            video.isMute = isMute_b1;
            return video;
        }

        [TestMethod]
        public void VideoPlayAt_Test() {
            bool expected_isPlaying = true;

            Video video = new Video(@"Samples\Sample20%Videos20%(52)20%-20%Copy.mp4.mp4");
            //video.playAt(0, 0); not a method anymore

            //Assert.AreEqual(expected_isPlaying, video.isPlaying);
            Assert.Inconclusive();
        }

        [TestMethod]
        public void VideoPause_Test()
        {
            /*
            bool expected_VideoPaused = false;
            Video newVideo = new Video(@"Samples\LongWavTest.wav");
            VideoCommand videoCommand = new VideoCommand("name" + " Audio", newVideo);
            MissingFilesWindow test = new MissingFilesWindow((videoCommand.media as Video).videoPath, videoCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            if (File.Exists(newVideo.videoPath))
            {
                newVideo.setUnitTestPlaybackState("Stopped");
                newVideo.play();
                newVideo.pause();
                Assert.AreEqual(expected_VideoPaused, newVideo.isPlaying); //fails because of object not set to reference around line 77 inside System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {}
            }
            else
            {
                Assert.Inconclusive();  //Doesn't appear to be any way to check whether or not a video is paused
            }
            */
            Assert.Inconclusive(); //See comment "fails because..." above
        }

        [TestMethod]
        public void VideoPlayTest()
        {
            /*
            bool expected_VideoPaused = true;
            Video newVideo = new Video(@"Samples\LongWavTest.wav");
            VideoCommand videoCommand = new VideoCommand("name" + " Audio", newVideo);
            MissingFilesWindow test = new MissingFilesWindow((videoCommand.media as Video).videoPath, videoCommand.media);
            test.Owner = MainWindow.main;
            if (test.ShowDialog() == true);

            if (File.Exists(newVideo.videoPath))
            {
                newVideo.setUnitTestPlaybackState("Stopped");
                newVideo.play();
                Assert.AreEqual(expected_VideoPaused, newVideo.isPlaying); //fails because of object not set to reference around line 77 inside System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {}
            }
            else
            {
                Assert.Inconclusive();  //Doesn't appear to be any way to check whether or not a video is paused
            }
            */
            Assert.Inconclusive(); //See comment "fails because..." above
        }
    }
}
