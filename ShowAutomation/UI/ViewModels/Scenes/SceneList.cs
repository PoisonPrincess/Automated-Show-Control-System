using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Commands;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ShowAutomation.UI.ViewModels.Scenes
{
    [Serializable()]
    public class SceneList : INotifyPropertyChanged, ISerializable
    {
        protected ObservableCollection<Scene> _list = new ObservableCollection<Scene>();
        public int currentScenePlaying = 0;
        public int sceneInViewIndex = 0;
        public Boolean isPlaying = false;
        internal SceneFadeSettings fadeSettings = new SceneFadeSettings();

        public Boolean changesMade { get; set; }

        public SceneList(List<Scene> sceneList) : this()
        {
            for (int i = 0; i < sceneList.Count; i++)
            {
                this._list.Add(sceneList[i]);
            }
            if (sceneList.Count > 0)
            {
                MainWindow.main.UpdateSceneTabs(_list);
                MainWindow.main.btnPlay.IsEnabled = true;
                MainWindow.main.btnStop.IsEnabled = true;
            }
            changesMade = false;
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        public SceneList()
        {
            isPlaying = false;
            changesMade = false;
        }

        public SceneList(SerializationInfo info, StreamingContext context) : this()
        {
            _list = (ObservableCollection<Scene>)info.GetValue("scenes", typeof(ObservableCollection<Scene>));

            try
            {
                fadeSettings = (SceneFadeSettings)info.GetValue("fadeSettings", typeof(SceneFadeSettings));
            }
            catch {
                fadeSettings = new SceneFadeSettings();
            }

            if (_list.Count > 0) {
                MainWindow.main.btnPlay.IsEnabled = true;
                MainWindow.main.btnStop.IsEnabled = true;
            }
        }

        /// <summary>
        /// Public list of commands UI accesses to display the list
        /// </summary>
        public ObservableCollection<Scene> Scenes
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
                OnPropertyChanged("Scenes");
            }
        }

        public void InsertItem(int index, Scene scene) {
            MainWindow.main.btnPlay.IsEnabled = true;
            MainWindow.main.btnStop.IsEnabled = true;
            if (currentScenePlaying >= index && (currentScenePlaying != 0 || _list[currentScenePlaying].elapsedMilliseconds != 0))
            {
                currentScenePlaying++;
            }
            _list.Insert(index, scene);
            if (currentScenePlaying == index)
            {
                _list[currentScenePlaying].elapsedMilliseconds = 0;
            }
            MainWindow.main.UpdateSceneTabs(_list);
            changesMade = true;
        }

        public void AddItem(Scene newScene)
        {
            MainWindow.main.btnPlay.IsEnabled = true;
            MainWindow.main.btnStop.IsEnabled = true;
            _list.Add(newScene);
            MainWindow.main.UpdateSceneTabs(_list);
        }

        public void DeleteItem(Scene scene)
        {
            if (currentScenePlaying > _list.IndexOf(scene)) {
                if (!_list[currentScenePlaying].isPlaying)
                {
                    _list[currentScenePlaying].elapsedMilliseconds = 0;
                }
                currentScenePlaying--;
            }
            _list.Remove(scene);
            if (currentScenePlaying >= _list.Count)
            {
                currentScenePlaying = 0;
                if (_list.Count >= 1)
                {
                    _list[currentScenePlaying].elapsedMilliseconds = 0;
                }
            }
            scene.ShowTicker(false);
            MainWindow.main.UpdateSceneTabs(_list);
            if (Scenes.Count==0)
            {
                MainWindow.main.btnPlay.IsEnabled=false;
                MainWindow.main.btnStop.IsEnabled = false;
            }
        }
        /// <summary>
        /// Returns the current index of scene playing 
        /// </summary>
        /// <returns></returns>
        public int getCurrentScenePlaying ()
        {
            MainWindow.main.DrawTimeline();
            return currentScenePlaying;
        }
        /// <summary>
        /// Updated is event is draged on timeline or if edited 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="eventId"></param>
        /// <param name="sceneId"></param>
        /// <returns></returns>
        public Boolean changeEventTime (ulong startTime, Guid eventId, Guid sceneId)
        {
            Scene scene = this.Scenes[this.currentScenePlaying];
            changesMade = true;
            for (var i = 0; i < this.Scenes.Count; i++)
            {
               if (this.Scenes[i].id.Equals(sceneId))
                {
                    Scene sceneToEdit = this.Scenes[i];

                    for (var j = 0; j < sceneToEdit.Events.Count; j++)
                    {
                        if (sceneToEdit.Events[j].id.Equals(eventId))
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                                EventGroup group = sceneToEdit.Events[j].group;
                                if (group != null) {
                                    long timeDiff = (long)startTime - (long)sceneToEdit.Events[j].startTime;
                                    Console.WriteLine("Moved " + timeDiff);
                                    group.updateTimesBy(timeDiff);
                                }
                                sceneToEdit.Events[j].setStartTime(startTime);
                                sceneToEdit.editEvent(sceneToEdit.Events[j]);
                                MainWindow.main.DrawTimeline();
                            });
                            return true;
                            
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Evaluate current scene and checs to see if next scene is auto play , if yes it will play next scene if no it will pause show
        /// </summary>
        /// <param name="autoplayNextScene"></param>
        /// <returns></returns>
        protected Boolean sceneFinishedCallback(Boolean autoplayNextScene)
        {
            if (currentScenePlaying + 1 < Scenes.Count)
            {
                Scenes[currentScenePlaying].ShowTicker(false);
                currentScenePlaying++;
                Scenes[currentScenePlaying].ShowTicker(true);
                if (sceneInViewIndex == currentScenePlaying - 1)
                {
                    sceneInViewIndex = currentScenePlaying;
                }
                if (Scenes[currentScenePlaying].ShouldSkip)
                {
                    sceneFinishedCallback(autoplayNextScene);
                    return autoplayNextScene;
                }
                if (!MainWindow.main.ShowView) MainWindow.main.DrawTimeline();
                if (Scenes[currentScenePlaying].autoPlay == true)
                {
                    MainWindow.main.setCurrentScenePlaying(currentScenePlaying);

                    if (!Scenes[currentScenePlaying].isPlaying)
                    {
                        Scenes[currentScenePlaying].elapsedMilliseconds = 0;
                        Scenes[currentScenePlaying].startPlayAt(sceneFinishedCallback);
                    }
                    return autoplayNextScene;
                }
                else
                {
                    this.PauseShow();
                    MainWindow.main.setCurrentScenePlaying(currentScenePlaying);
                    MainWindow.main.Status = "Resume";
                    return autoplayNextScene;
                }
            }
            this.StopShow();
            MainWindow.main.Status = "PLAY";

            return autoplayNextScene;
        }

        internal ulong GetDuration()
        {
            ulong ShowLength = 0;
            foreach (Scene scene in Scenes) {
                ShowLength += scene.duration;
            }
            return ShowLength;
        }

        public void PauseShow ()
        {
            isPlaying = false;
            if (Scenes.Count > 0)
            {
                Scenes[currentScenePlaying].Pause();
            }
        }

        public void SetCurrentScenePlaying(Scene cur)
        {
            for (int i=0; i < Scenes.Count;i++)
            {         
                if (cur.name.Equals(Scenes[i].name))
                {
                    currentScenePlaying = i;
                    sceneInViewIndex = currentScenePlaying;
                    MainWindow.main.DrawTimeline();
                    return;
                }    
            }          
        }

        public void PlayShow()
        {
            if (isPlaying == true)
            {
                PauseShow();
                return;
            }
            isPlaying = true;
            if (Scenes.Count > 0)
            {
                if (!Mode.isEditMode && !isPlaying) {
                    currentScenePlaying = 0;
                    foreach (Scene scene in Scenes)
                    {
                        if (!scene.ShouldSkip)
                        {
                            currentScenePlaying = Scenes.IndexOf(scene);
                            break;
                        }
                    }
                    Scenes[currentScenePlaying].elapsedMilliseconds = 0;
                }
                if (!Scenes[currentScenePlaying].isPlaying)
                {
                    if (Scenes[currentScenePlaying].numberOfEventsBefore(Types.MIDI) > 0)
                    {
                        Boolean shouldMidiCatchUp = MainWindow.main.ShouldMidiCatchUp();
                        if (shouldMidiCatchUp)
                        {
                            Scenes[currentScenePlaying].MIDICatchUp();
                        }
                    }
                    Scenes[currentScenePlaying].startPlayAt(sceneFinishedCallback);

                }
            }
        }
       
        
        public void StopShow ()
        {
            this.StopShow(true);
        }

        public void StopShow (Boolean executeCallback)
        {
            isPlaying = false;
            int TempScene= getCurrentScenePlaying();
            Scenes[currentScenePlaying].Stop(executeCallback);
            currentScenePlaying = 0;
            foreach (Scene scene in Scenes) {
                if (!scene.ShouldSkip) {
                    currentScenePlaying = Scenes.IndexOf(scene);
                    break;
                }
            }
            sceneInViewIndex = currentScenePlaying;
            Scenes[currentScenePlaying].elapsedMilliseconds = 0;

            MainWindow.main.DrawTimeline();
        }

        public void  moveScene (int amount, Scene sceneToMove)
        {
            int index = Scenes.IndexOf(sceneToMove);
            if ((amount <= -1 && index == 0) || (amount >= 1 && index == Scenes.Count - 1))
            {
                return;
            } else {
                Scenes.Remove(sceneToMove);
                Scenes.Insert(index + amount ,sceneToMove);
                MainWindow.main.DrawTimeline();
            }
        }

        public void editEvent(Scene scene, Event sceneEvent, CommandList commandList)
        {
            CreateEventWindow window = new CreateEventWindow(scene, commandList, sceneEvent, MainWindow.main.DoAction);
            window.Owner = MainWindow.main;
            window.ShowDialog();
        }

        public object ToJsonInstance ()
        {
            return new JsonSceneList(this);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }

        public void DeselectEvents() {
            Scene scene = _list[currentScenePlaying];
            scene.DeselectEvents();
        }

        public LinkedList<Event> getAllEventsLinkedList()
        {
            LinkedList<Event> AllEvents = new LinkedList<Event>();
            for (int i = 0; i < this.Scenes.Count; i++)
            {
                for (int k = 0; k < this.Scenes[i].Events.Count; k++)
                {
                    if (this.Scenes[i].Events[k].OutputDevice.type.Equals(Types.GHOST) && Mode.isEditMode)
                    {
                        //this.Scenes[i].Events[k].outputDevice.id = 0;
                        AllEvents.AddLast(this.Scenes[i].Events[k]);
                    }

                    if (this.Scenes[i].Events[k].OutputDevice.type == Types.AUDIO)
                    {
                        int checkDevice = AudioDevice.getDeviceIndexByName(this.Scenes[i].Events[k].OutputDevice.deviceName);

                        if (checkDevice == -1)
                        {
                            if (this.Scenes[i].Events[k].OutputDevice.OriginalExists())
                            {
                                Console.WriteLine("Duplicate Audio device found");
                                LaneOutput.addDevice(this.Scenes[i].Events[k].OutputDevice);
                                MainWindow.main.UpdateOutputDevices();
                            }
                            else
                            {
                                AllEvents.AddLast(this.Scenes[i].Events[k]);
                            }
                        }
                        else
                        {
                            this.Scenes[i].Events[k].OutputDevice.id = checkDevice;
                            //name = this.Scenes[i].Events[k].outputDevice.deviceName;
                        }
                    }
                    else if (this.Scenes[i].Events[k].OutputDevice.type == Types.VIDEO)
                    {
                        int checkDevice = VideoDevice.getDeviceIndexByName(this.Scenes[i].Events[k].OutputDevice.deviceName);
                        if (checkDevice == -1)
                        {
                            if (this.Scenes[i].Events[k].OutputDevice.OriginalExists())
                            {
                                Console.WriteLine("Duplicate Video device found");
                                LaneOutput.addDevice(this.Scenes[i].Events[k].OutputDevice);
                                MainWindow.main.UpdateOutputDevices();
                            }
                            else
                            {
                                AllEvents.AddLast(this.Scenes[i].Events[k]);
                            }
                        }
                        else
                        {
                            this.Scenes[i].Events[k].OutputDevice.id = checkDevice;
                        }
                    }

                    else if (this.Scenes[i].Events[k].OutputDevice.type == Types.MIDI)
                    {
                        int checkDevice = MidiDevice.getDeviceIndexByName(this.Scenes[i].Events[k].OutputDevice.deviceName);

                        if (checkDevice == -1)
                        {
                            if (this.Scenes[i].Events[k].OutputDevice.OriginalExists())
                            {
                                Console.WriteLine("Duplicate Midi device found");
                                LaneOutput.addDevice(this.Scenes[i].Events[k].OutputDevice);
                                MainWindow.main.UpdateOutputDevices();
                            }
                            else
                            {
                                AllEvents.AddLast(this.Scenes[i].Events[k]);
                            }
                        }
                        else
                        {
                            this.Scenes[i].Events[k].OutputDevice.id = checkDevice;
                        }
                    }
                }
            }

            return AllEvents;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("scenes", _list);
            info.AddValue("fadeSettings", fadeSettings);
        }

        internal void SetCurrentScene(Scene scene)
        {
            Scenes[currentScenePlaying].ShowTicker(false);
            Scenes[currentScenePlaying].DeselectEvents();
            Console.WriteLine("Switch to scene " + scene.name);
            currentScenePlaying = Scenes.IndexOf(scene);
            Scenes[currentScenePlaying].ShowTicker(true);
        }

        internal Scene GetSceneAtTime(ulong time)
        {
            foreach (Scene scene in Scenes) {
                ulong tempDuration = scene.duration;
                if (tempDuration == 0) {
                    tempDuration = MainWindow.main.scaleToTime(50);
                }
                if (time - tempDuration > time || time - tempDuration == 0) { // > used because Ulong overflow when they are negitive
                    return scene;
                }
                time -= tempDuration;
            }
            return null;
        }

        internal ulong getSceneStartTime(Scene scene)
        {
            ulong StartTime = 0;
            foreach (Scene temp in Scenes)
            {
                if (temp == scene) {
                    return StartTime;
                }
                StartTime += temp.duration;
            }
            return StartTime;
        }

        internal void FadeToNextScene()
        {
            Scenes[currentScenePlaying].FadeToEnd(fadeSettings);
        }

        internal void createSceneEvent(Scene scene, CommandList commandList)
        {
            scene.createSceneEvent(commandList);
        }
    }

    public class JsonSceneList
    {
        public List<object> scenes = new List<object>();

        public JsonSceneList(SceneList commandList)
        {

            for (int i = 0; i < commandList.Scenes.Count; i++)
            {
                this.scenes.Add(commandList.Scenes[i].ToJsonInstance());
            }
        }
    }
}
