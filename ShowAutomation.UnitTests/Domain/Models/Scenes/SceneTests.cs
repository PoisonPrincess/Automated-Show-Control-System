using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.ViewModels.Commands;

namespace ShowAutomation.Domain.Models.Scenes.Tests
{
    [TestClass()]
    public class SceneTests
    {
        private object expectedVal;

        [TestMethod()]
        public void stopScrubTest()
        {
            Assert.Inconclusive();  //Can't be tested due to construction
            MainWindow window = new MainWindow();
            CommandList list = new CommandList();
            var scene = new Scene
            {
                name = "Scrub Test Scene",
                listItemColor = "LightBrown"
            };
            ulong startTime = 10;

            MusicCommand audioCommand = new MusicCommand(scene.name + " Audio", new Song(@"Samples\WalloonLilliShort.mp3"));
            list.AddItem(audioCommand);

            MusicEvent Audio = new MusicEvent(audioCommand, startTime, scene);
            Event @event = Audio;
            scene.addEvent(@event);

            //this.ScenePauseTest();

            window.Status = "Play";

            // Getting some weird error about not finding resoures named 'CommandListItem'

            //PrivateObject obj = new PrivateObject(window);
            //var retVal = obj.Invoke("PrivateMethod");
            //Assert.AreEqual(retVal, expectedVal);

            //bool expected_scrubStartState = false;
            //string expected_MainWindowStatus = "Play";

            scene.stopScrub();

            //Assert.AreEqual(scene.getSceneTimer(), scene.getScrubStart());
            //Assert.AreEqual(expected_MainWindowStatus, MainWindow.main.Status); //see above
            
        }




        /*
        [TestMethod]
        public void drawTickerTest(Grid LaneGrid, List<OutputDevice> outputList)
        {

        }var SceneList = new UI.ViewModels.Scenes.SceneList();
        */

        [TestMethod]
        public void ScenePauseTest()
        {
            bool expected_isPlaying = false;
            bool expected_timerEnabled = false;

            Scene scene = new Scene();
            scene.Pause();

            Assert.AreEqual(expected_isPlaying, scene.isPlaying);
            Assert.AreEqual(expected_timerEnabled, scene.getSceneTimer().Enabled);
        }

        [TestMethod]
        public void IsScenePlayingTest()
        {
            var Scene = new Scene
            {
                name = "Scene 1",
                listItemColor = "Green"
            };

            Event @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, Scene);

            Scene.addEvent(@event);
            Assert.IsFalse(Scene.isPlaying);
        }

        [TestMethod]
        public void resumeScrubbingTest()
        {
            int testCase = 0;
            bool scrubStart = true;
            bool scrubbingAudioPlaying = true;
            bool isScrubbing = true;
            ulong currentTime = new ulong();
            ulong tempCursorTime = new ulong();
            currentTime = (ulong)1;
            tempCursorTime = (ulong)0;

            Scene scene = new Scene();

            for (int i = 0; i < 2; i++)      //tests all values of the if statement within Scene.resumeScrubbing()
            {
                scrubbingAudioPlaying = true;
                for (int j = 0; j < 2; j++)
                {
                    isScrubbing = true;
                    for (int k = 0; k < 2; k++)
                    {
                        tempCursorTime = 0;
                        for (int l = 0; l < 3; l++)
                        {
                            scene.setIsScrubbing(isScrubbing);
                            scene.setScrubbingAudioPlaying(scrubbingAudioPlaying);
                            scene.setScrubStart(scrubStart);
                            resumeScrubbingTestTester(scene, currentTime, tempCursorTime, testCase);
                            tempCursorTime++;
                            testCase++;
                        }
                        isScrubbing = false;
                    }
                    scrubbingAudioPlaying = false;
                }
                scrubStart = false;
            }

        }

        public void resumeScrubbingTestTester(Scene scene, ulong currentTime, ulong tempCursorTime, int testCase)
        {
            bool expected_scrubbingAudioPlaying;
            if ((testCase < 6) || ((testCase >= 12) && (testCase <= 17)))
            {
                expected_scrubbingAudioPlaying = true;
            }else
            {
                expected_scrubbingAudioPlaying = false;
            }
            scene.resumeScrubbing(currentTime, tempCursorTime);
            Assert.AreEqual(expected_scrubbingAudioPlaying, scene.getScrubbingAudioPlaying());
        }
        

        [TestMethod()]
        public void Fail_CursorCanMoveAfterStop_Test()
        {
            ulong startTime = 10;
            var Scene = new Scene
            {
                name = "Scene 2",
                listItemColor = "LightBlue"
            };
                   
            MusicCommand audioCommand = new MusicCommand(Scene.name + " Audio", new Song(@"Samples\WalloonLilliShort.mp3"));
            MusicEvent Audio = new MusicEvent(audioCommand, startTime, Scene);

            Event @event = Audio;
            Scene.addEvent(@event);

            //Scene.isPlaying = false;

            if (Scene.isPlaying)
                Scene.Stop();

            Scene.setCursorTime(10);

            Assert.IsFalse(Scene.isPlaying);
        }

        [TestMethod()]
        //Test Method to see if an event can be added to a scene
        public void addEventToSceneTest()
        {
            Scene scene = new Scene();
            Event @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event1 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event2 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);

            scene.addEvent(@event);
            scene.addEvent(@event2);
            scene.addEvent(@event1);

            Assert.AreEqual(scene.Events.Count,3);
        }

        [TestMethod()]
        //Test Method to see if an event can be removed from a scene
        public void RemoveEventFromSceneTest()
        {
            Scene scene = new Scene();
            Event @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event1 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event2 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);

            scene.addEvent(@event);
            scene.addEvent(@event2);
            scene.addEvent(@event1);
            scene.removeEvent(@event);

            Assert.AreEqual(scene.Events.Count,2);
        }

        [TestMethod()]
        //Method to test if changed event time will work
        public void changeEventStarttimeTest()
        {
            Scene newScene = new Scene();
            Event newEvent = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, newScene);
            ulong originalStart = newEvent.startTime;
            newEvent.setStartTime(2);
            Assert.AreNotEqual(originalStart, newEvent.startTime);

        }
        [TestMethod()]
        //method to test if a midi event has a color
        public void changeMidiColorTest()
        {
            Scene newScene = new Scene();
            Event newEvent = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, newScene);
            newEvent.listItemColor = "Green";
            
            Assert.AreEqual(newEvent.listItemColor, "#FFFFFFFF");
        }

        [TestMethod()]
        public void TickerTest()
        {
            Scene scene = new Scene();
            Event @event = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event1 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);
            Event @event2 = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, scene);

            scene.addEvent(@event);
            scene.addEvent(@event2);
            scene.addEvent(@event1);
            scene.removeEvent(@event);

            ulong startTime = 10;
            var Scene = new Scene
            {
                name = "Scene 2",
                listItemColor = "LightBlue"
            };

            MusicCommand audioCommand = new MusicCommand(Scene.name + " Audio", new Song("C: /Users/gil/Downloads/5 Ways to Print on WoodDIY Image Transfer.mp3"));
            MusicEvent Audio = new MusicEvent(audioCommand, startTime, Scene);

            scene.addEvent(Audio);

            if (scene.isPlaying && Audio.isPlaying)
            {
                scene.setCursorTime(1);
                scene.setTickerLocation();
            }

            Assert.IsFalse(scene.isPlaying);           
        }

    }

}


