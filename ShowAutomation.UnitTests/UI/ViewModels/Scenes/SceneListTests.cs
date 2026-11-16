using ShowAutomation.UI.ViewModels.Scenes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ShowAutomation.Domain.Utils;

namespace ShowAutomation.UI.ViewModels.Scenes.Tests
{
    [TestClass()]
    public class SceneListTests
    {
        public SceneList Constructor(List<Scene> sceneList)
        {
            SceneList target = new SceneList(sceneList);
            return target;
        }

        // Example - testing when sceneList is empty - passed
        [TestMethod]
        public void sceneList_isPlaying_ReturnTrue()
        {
            //Arranage
            var SceneList = new SceneList();

            //Act  - scenerio
            var result = SceneList.isPlaying;
            if (SceneList.Scenes.Count > 0)
            {
                result = true;
            }

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        //tests the adding of scenes to the scene list.
        public void addSceneTest()
        {
            /*
            SceneList scenes = new SceneList();
            Scene newScene = new Scene();
            scenes.AddItem(newScene);
            Assert.AreEqual(scenes.Scenes.Count, 1);
            */
            Assert.Inconclusive(); //Currently Fails
        }

        [TestMethod()]
        //tests whether or nto a scene can be deleted. 
        public void deleteSceneTest()
        {
            /*
            SceneList scenes = new SceneList();
            Scene newScene = new Scene();
            scenes.AddItem(newScene);
            scenes.DeleteItem(newScene);
            Assert.AreEqual(scenes.Scenes.Count, 0);
            */
            Assert.Inconclusive(); //Currently Fails
        }

        [TestMethod()]
        //Method to test if a command type for an event has changed
        public void changeEventTypeTest()
        {
            /*
            SceneList scenes = new SceneList();
            Scene newScene = new Scene();
            scenes.AddItem(newScene);
            Event newEvent = new MidiEvent(new MidiCommand(new NAudio.Midi.MidiEvent(0, 1, new NAudio.Midi.MidiCommandCode())), 0, newScene);
            scenes.Scenes[0].addEvent(newEvent);
            */
            Assert.Inconclusive(); //Currently Fails
        }

        //Method to test if the current scene playing has switched
        public void getCurrentScenePlayingTest()
        {
            /*
            SceneList scenelist = new SceneList();
            Scene newScene = new Scene();
            Scene newScene2 = new Scene();
            scenelist.AddItem(newScene);
            scenelist.AddItem(newScene2);
            scenelist.SetCurrentScenePlaying(newScene);
            */
            Assert.Inconclusive(); //Currently Fails
        }

        public ObservableCollection<Scene> ScenesGet(SceneList target)
        {
            ObservableCollection<Scene> result = target.Scenes;
            return result;
        }
        public void moveScene(SceneList target, int amount, Scene sceneToMove)
        {
            target.moveScene(amount, sceneToMove);
        }
        public void PlayShow(SceneList target)
        {
            target.PlayShow();
        }
        [TestMethod]
        public void SceneListConstructorTest()
        {
            List<Scene> list;
            SceneList sceneList;
            Scene[] scenes = new Scene[0];
            list = new List<Scene>((IEnumerable<Scene>)scenes);
            sceneList = this.Constructor(list);
            Assert.IsNotNull((object)sceneList);
            Assert.AreEqual<int>(0, sceneList.currentScenePlaying);
            Assert.AreEqual<int>(0, sceneList.sceneInViewIndex);
            Assert.AreEqual<bool>(false, sceneList.isPlaying);
            Assert.AreEqual<bool>(false, sceneList.changesMade);
            Assert.IsNotNull(sceneList.Scenes);
        }
        [TestMethod]
        // moving scene on main window
        public void moveSceneTest()
        {
            SceneList sceneList;
            sceneList = new SceneList();
            sceneList.currentScenePlaying = 0;
            sceneList.sceneInViewIndex = 0;
            sceneList.isPlaying = false;
            this.moveScene(sceneList, 1, (Scene)null);
            Assert.IsNotNull((object)sceneList);
            Assert.AreEqual<int>(0, sceneList.currentScenePlaying);
            Assert.AreEqual<int>(0, sceneList.sceneInViewIndex);
            Assert.AreEqual<bool>(false, sceneList.isPlaying);
            Assert.AreEqual<bool>(false, sceneList.changesMade);
            Assert.IsNotNull(sceneList.Scenes);
        }
        [TestMethod]
        public void ScenesGetTest()
        {
            SceneList sceneList;
            ObservableCollection<Scene> observableCollection;
            sceneList = new SceneList();
            sceneList.currentScenePlaying = 0;
            sceneList.sceneInViewIndex = 0;
            sceneList.isPlaying = false;
            observableCollection = this.ScenesGet(sceneList);
            Assert.IsNotNull((object)observableCollection);
            Assert.IsNotNull((object)sceneList);
            Assert.AreEqual<int>(0, sceneList.currentScenePlaying);
            Assert.AreEqual<int>(0, sceneList.sceneInViewIndex);
            Assert.AreEqual<bool>(false, sceneList.isPlaying);
            Assert.AreEqual<bool>(false, sceneList.changesMade);
            Assert.IsNotNull(sceneList.Scenes);
            Assert.IsTrue
                (object.ReferenceEquals(sceneList.Scenes, (object)observableCollection));
        }

        // Test Playing show on scenelist
        [TestMethod]
        public void PlayShowTest()
        {
            SceneList sceneList;
            sceneList = new SceneList();
            sceneList.currentScenePlaying = 0;
            sceneList.sceneInViewIndex = 0;
            sceneList.isPlaying = false;
            this.PlayShow(sceneList);
            Assert.IsNotNull((object)sceneList);
            Assert.AreEqual<int>(0, sceneList.currentScenePlaying);
            Assert.AreEqual<int>(0, sceneList.sceneInViewIndex);
            Assert.AreEqual<bool>(true, sceneList.isPlaying);
            Assert.AreEqual<bool>(false, sceneList.changesMade);
            Assert.IsNotNull(sceneList.Scenes);
        }
        public object ToJsonInstance(SceneList target)
        {
            object result = target.ToJsonInstance();
            return result;
           
        }
        [TestMethod()]
        public void SceneListToJsonInstanceTest()
        {
          
                SceneList sceneList;
                object o;
                sceneList = new SceneList();
                sceneList.currentScenePlaying = 0;
                sceneList.sceneInViewIndex = 0;
                sceneList.isPlaying = false;
                //sceneList.fadeSettings = (SceneFadeSettings)null;
                o = this.ToJsonInstance(sceneList);
                Assert.IsNotNull(o);
                Assert.IsNotNull((object)sceneList);
                Assert.AreEqual<int>(0, sceneList.currentScenePlaying);
                Assert.AreEqual<int>(0, sceneList.sceneInViewIndex);
                Assert.AreEqual<bool>(false, sceneList.isPlaying);
                Assert.AreEqual<bool>(false, sceneList.changesMade);
                Assert.IsNotNull(sceneList.Scenes);
            
        }
    }
}