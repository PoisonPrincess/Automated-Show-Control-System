using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.ViewModels.Commands;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Models.Commands;
using System.IO;
using ShowAutomation.UI.Views;
using NAudio.Wave;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using ShowAutomation.UI.ViewModels.Media;

namespace ShowAutomation.UnitTests.Domain.Models.Media
{
    [TestClass]
    public class UnitTest1
    {
        
        private bool playbackStop = false;
        private void playbackStopped()
        {
            playbackStop = true;
        }

        
        internal WaveOut createSong(Song target, ulong time)
        {
            object[] args = new object[1];
            args[0] = (object)time;
            Type[] parameterTypes = new Type[1];
            parameterTypes[0] = typeof(ulong);
            WaveOut result0 = ((MethodBase)(typeof(Song).GetMethod("createSong",
                                                                   BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, (Binder)null,
                                                                   CallingConventions.HasThis, parameterTypes, (ParameterModifier[])null)))
                                  .Invoke((object)target, args) as WaveOut;
            WaveOut result = result0;
            return result;
        }

        public void songPathSet(Song target, string value)
        {
            target.songPath = value;
            
        }

        [TestMethod]
        public void createSong()
        {
                Song song;
                WaveOut waveOut;
                song = new Song((string)null);
                song.isPlaying = false;
                song.waveformGenerator = (SongWaveformGenerator)null;
                song.waveData = (List<float>[])null;
                song.ChannelPosition = 0;
                song.setUnitTestPlaybackState((string)null);
                //song.play();
                //song.playAt(0uL, 0);
                waveOut = this.createSong(song, 0uL);
                Assert.IsNull((object)waveOut);
                Assert.IsNotNull((object)song);
                Assert.IsNull((object)(song.waveformGenerator));
                Assert.IsNull((object)(song.waveData));
                Assert.AreEqual<bool>(false, ((MediaModel)song).isPlaying);
                Assert.AreEqual<string>((string)null, song.songPath);
                Assert.AreEqual<double>(0, song.ChannelPosition);
                Assert.AreEqual<double>(0, song.ChannelLength);
                Assert.AreEqual<bool>(false, song.IsPlaying);
                Assert.IsNull(song.ActiveStream);
            
        }

        [TestMethod()]
        public void SongFileExistsTest()
        {
            CommandList list = new CommandList();
            var scene = new Scene
            {
                name = "Scrub Test Scene",
                listItemColor = "LightYellow"
            };

            MusicCommand audioCommand = new MusicCommand(scene.name + " Audio", new Song(@"Samples\WallonLilliShort.mp3"));
            list.AddItem(audioCommand);
            (audioCommand.media as Song).songPath = null;

            if (!File.Exists((audioCommand.media as Song).songPath))
            {
                MissingFilesWindow test = new MissingFilesWindow((audioCommand.media as Song).songPath, audioCommand.media);
                test.Owner = MainWindow.main;
                if (test.ShowDialog() == true);
            }
        }


        [TestMethod]
        public void songPathSet()
        {
            Song song;
            song = new Song((string)null);
            song.isPlaying = false;
            song.waveformGenerator = (SongWaveformGenerator)null;
            song.waveData = (List<float>[])null;
            song.ChannelPosition = 0;
            song.setUnitTestPlaybackState((string)null);
            //song.play();
            //song.playAt(0uL, 0);
            this.songPathSet(song, (string)null);
            Assert.IsNotNull((object)song);
            Assert.IsNotNull((object)(song.waveformGenerator));
            Assert.AreEqual<string>((string)null, song.waveformGenerator.songPath);
            Assert.IsNotNull((object)(song.waveformGenerator.UIDisaptcher));
            Assert.IsNotNull(song.waveformGenerator.UIDisaptcher.Thread);
            Assert.AreEqual<string>("Exploration Worker Thread",
                                    song.waveformGenerator.UIDisaptcher.Thread.Name);
            Assert.AreEqual<bool>
                (false, song.waveformGenerator.UIDisaptcher.HasShutdownStarted);
            Assert.AreEqual<bool>
                (false, song.waveformGenerator.UIDisaptcher.HasShutdownFinished);
            Assert.IsNotNull((object)(song.waveformGenerator.waves));
            Assert.AreEqual<int>(3, song.waveformGenerator.waves.Length);
            Assert.IsNull((object)(song.waveformGenerator.waves[0]));
            Assert.IsNull((object)(song.waveformGenerator.waves[1]));
            Assert.IsNull((object)(song.waveformGenerator.waves[2]));
            Assert.IsNull((object)(song.waveData));
            Assert.AreEqual<bool>(false, ((MediaModel)song).isPlaying);
            Assert.AreEqual<string>((string)null, song.songPath);
            Assert.AreEqual<double>(0, song.ChannelPosition);
            Assert.AreEqual<double>(0, song.ChannelLength);
            Assert.AreEqual<bool>(false, song.IsPlaying);
            Assert.IsNull(song.ActiveStream);
        }

        [TestMethod]
        public void songPauseTest02()
        {
            Song song;
            song = new Song((string)null);
            song.isPlaying = false;
            song.waveformGenerator = (SongWaveformGenerator)null;
            song.waveData = (List<float>[])null;
            song.ChannelPosition = double.NaN;
            song.setUnitTestPlaybackState((string)null);
            //song.play();
            //song.playAt(0uL, 0);
            //this.pause(song);
            Assert.IsNotNull((object)song);
            Assert.IsNull((object)(song.waveformGenerator));
            Assert.IsNull((object)(song.waveData));
            Assert.AreEqual<bool>(false, ((MediaModel)song).isPlaying);
            Assert.AreEqual<string>((string)null, song.songPath);
            Assert.AreEqual<double>(double.NaN, song.ChannelPosition);
            Assert.AreEqual<double>(0, song.ChannelLength);
            Assert.AreEqual<bool>(false, song.IsPlaying);
            Assert.IsNull(song.ActiveStream);
        }

        [TestMethod]
        public void songPlayTest()
        {
            Song song;
            song = new Song((string)null);
            song.isPlaying = false;
            song.waveformGenerator = (SongWaveformGenerator)null;
            song.waveData = (List<float>[])null;
            song.ChannelPosition = double.NaN;
            song.setUnitTestPlaybackState((string)null);
            //song.play();
            //song.playAt(0uL, 0);
            //this.play(song);
            Assert.IsNotNull((object)song);
           
            Assert.AreEqual<bool>(false, song.IsPlaying);
            Assert.IsNull(song.ActiveStream);
        }
    }
}
