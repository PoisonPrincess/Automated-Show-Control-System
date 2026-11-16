using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels;
using ShowAutomation.UI.ViewModels.Commands;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShowAutomation.Domain.Models.Scenes
{
    [Serializable()]
    public class Scene : INotifyPropertyChanged, ISerializable, ICloneable
    {
        public ulong duration = 0;
        private int timerInterval = 250; // range of error in milliseconds of event timeing.
        private Func<Boolean, Boolean> sceneFinishedCallback = null;

        public ObservableCollection<Event> Events
        {
            get
            {
                return _events;
            }
            set
            {
                _events = value;
                OnPropertyChanged("Events");
            }
        }

        protected ObservableCollection<Event> _events = new ObservableCollection<Event>();
        public List<Event> eventsPlaying = new List<Event>();
        public String name { get; set; }
        public ulong elapsedMilliseconds = 0;

        public Boolean _autoPlay;
        public Boolean autoPlay { get { return _autoPlay; } set { MainWindow.main.DoAction(new Actions.AutoPlaySceneAction(this, value)); } }
        public Guid id;
        private Timer timer = new Timer();
        private Boolean isScrubbing = false;            //to check if the cursor is being held for scrubbing

        private ulong cursorTime = 0;                   //to scrub until
        private Boolean scrubbingAudioPlaying = false;  //to see if audio is playing while isScrubbing is true
        private ulong startPoint = 0;                   //to start playing audio from when scrubbing
        private Boolean scrubStart = false;             //to inform when scrubbing has begun

        private bool fading = false;
        private ulong fadeStart;
        private SceneFadeSettings fadeSettings;

        private List<Rectangle> lanes;
        private Line tickBar;
        private Polygon handle;
        private Line scrubBar;
        private Canvas timeKey;
        private Timer clickTimer = new Timer();
        private double clickX;
        public String listItemColor { get; set; }
        public String listItemTextColor
        {
            get
            {
                return ColorManager.TEXT_COLOR;
            }
            set { }
        }
        protected Boolean _isPlaying = false;

        public Boolean isPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
                if (value == true)
                {
                    this.listItemColor = ColorManager.IS_PLAYING_COLOR;
                }
                else
                {
                    if (ShouldSkip)
                    {
                        this.listItemColor = ColorManager.SKIP_COLOR;
                    }
                    else
                    {
                        this.listItemColor = "#00000000";
                    }
                }
                OnPropertyChanged("isPlaying");
                OnPropertyChanged("listItemColor");
            }
        }

        public bool _ShouldSkip = false;
        public bool ShouldSkip
        {
            get
            {
                return _ShouldSkip;
            }
            set
            {
                _ShouldSkip = value;
                if (value == true)
                {
                    this.listItemColor = ColorManager.SKIP_COLOR;
                }
                else
                {
                    if (isPlaying)
                    {
                        this.listItemColor = ColorManager.IS_PLAYING_COLOR;
                    }
                    else
                    {
                        this.listItemColor = "#00000000";
                    }
                }
                OnPropertyChanged("listItemColor");
            }
        }

        public Scene()
        {
            timer.Interval = timerInterval;
            timer.Elapsed += elapsedTimerEvent;
            if (this.id == Guid.Empty)
            {
                this.id = Guid.NewGuid();
            }
        }

        public Scene(String name, Boolean autoPlay) : this()
        {
            _autoPlay = autoPlay;
            this.name = name;
        }

        public Scene(String name, Boolean autoPlay, Guid id) : this(name, autoPlay)
        {
            this.id = id;
        }

        public Scene(SerializationInfo info, StreamingContext context) : this()
        {
            id = (Guid)info.GetValue("id", typeof(Guid));
            name = (string)info.GetValue("name", typeof(string));
            _autoPlay = (bool)info.GetValue("audoPlay", typeof(bool));
            duration = (ulong)info.GetValue("duration", typeof(ulong));
            _events = (ObservableCollection<Event>)info.GetValue("events", typeof(ObservableCollection<Event>));
        }

        internal void ShowTicker(bool show)
        {
            if (tickBar != null && handle != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    tickBar.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                    handle.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                });
            }
        }

        public void setCursorTime(ulong currentTime)
        {
            ulong tempCursorTime = cursorTime;

            if (tempCursorTime > currentTime)
            {
                startPoint = cursorTime;
            }
            if (cursorTime != currentTime)
            {
                cursorTime = currentTime;
                resumeScrubbing(cursorTime, tempCursorTime);
            }
        }

        //to make scrubbing continue if playback catches up to cursor, but cursor is moved after:
        public void resumeScrubbing(ulong currentTime, ulong tempCursorTime)
        {
            if ((currentTime > tempCursorTime) && isScrubbing && !scrubbingAudioPlaying && scrubStart)
            {
                startPlayAt(startPoint);
                scrubbingAudioPlaying = true;
            }
        }

        protected void setEvents(List<Event> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                events[i].setCallback(eventFinishedPlayingCallback);
                this._events.Add(events[i]);
            }
        }
        /// <summary>
        /// Aranges comands by finish time 
        /// </summary>
        protected void sortEvents()
        {
            List<Event> events = Events.ToList<Event>();
            events.Sort((a, b) => (a.startTime.CompareTo(b.startTime)));
            this._events.Clear();
            this.setEvents(events);
        }
        /// <summary>
        /// Sets total scene Duration 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="autoPlay"></param>
        /// <param name="events"></param>
        public Scene(String name, Boolean autoPlay, List<Event> events) : this(name, autoPlay)
        {
            this.setDuration();
            this.setEvents(events);
        }
        
        public void Stop()
        {
            this.Stop(true);
        }

        public void Stop(Boolean executeCallback)   //checked for bugs
        {
            bool wasNotPlaying = this.isPlaying;
            this.isPlaying = false;
            timer.Stop();
            foreach (OutputDevice device in MainWindow.main.outputs.DisplayList)
            {
                device.StopCommands();
            }
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Played = false;
            }
            this.eventsPlaying.Clear();
            this.elapsedMilliseconds = 0;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                //setTickerLocation();
                MainWindow.main.updateTimelineInfo(null, null);
            });
            if (wasNotPlaying && this.sceneFinishedCallback != null && executeCallback) this.sceneFinishedCallback(this.autoPlay);
        }

        public void Pause()
        {
            this.isPlaying = false;
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Played = false;
            }
            foreach (OutputDevice device in MainWindow.main.outputs.DisplayList) {
                device.PauseCommands();
            }
            if (MainWindow.main != null) //for unit tests
            {
                foreach (OutputDevice Device in MainWindow.main.outputs.getOutputs(Types.VIDEO, false))
                {
                    ((VideoDevice)Device).PauseVideo();
                }
            }
            timer.Stop();
        }

        public ulong setDuration()
        {
            ulong duration = 0;
            for (int i = 0; i < this._events.Count; i++)
            {
                if (this._events[i].getEndTime() > duration)
                {
                    duration = this._events[i].getEndTime();
                }
            }
            this.duration = duration;
            return duration;
        }

        public Boolean canPlaceEvent(Event eventToAdd)
        {

            for (int i = 0; i < this._events.Count; i++)
            {
                if (this._events[i].isConflicting(eventToAdd))
                {
                    return false;
                }
            }
            return true;
        }

        public void removeEvent(Event eventToRemove)
        {
            this._events.Remove(eventToRemove);
            this.sortEvents();
            this.setDuration();
            if (eventsPlaying.Contains(eventToRemove)) {
                eventToRemove.Stop();
                eventsPlaying.Remove(eventToRemove);
            }
        }

        public void ChangeEventTime(Event ev, ulong startTime) {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                EventGroup group = ev.group;
                if (group != null)
                {
                    long timeDiff = (long)startTime - (long)ev.startTime;
                    Console.WriteLine("Moved " + timeDiff);
                    group.updateTimesBy(timeDiff);
                }
                ev.setStartTime(startTime);
                editEvent(ev);
                MainWindow.main.DrawTimeline();
            });
        }

        public void editEvent(Event eventToEdit)
        {
            for (int i = 0; i < this._events.Count; i++)
            {
                if (this._events[i].id == eventToEdit.id)
                {
                    this._events[i].Stop();
                    eventsPlaying.Remove(this._events[i]);
                    this._events[i] = eventToEdit;
                    this.sortEvents();
                    this.setDuration();
                    MainWindow.main.DrawTimeline();
                    return;
                }
            }
        }

        /// Opens Create event window when the timeline is right clicked 
        public void createSceneEventFromTimeline(ulong startTime, string outputType, OutputDevice lane, CommandList commandList)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                CreateEventWindow window = new CreateEventWindow(this, commandList, MainWindow.main.DoAction, startTime, outputType, lane);
                window.Owner = MainWindow.main;
                window.ShowDialog();
            });
        }

        public void createSceneEvent(CommandList commandList)
        {
            CreateEventWindow window = new CreateEventWindow(this, commandList, MainWindow.main.DoAction);
            window.Owner = MainWindow.main;
            window.ShowDialog();
        }

        public void addEvent(Event newCue)
        {
            if (newCue.type.Equals(Types.VIDEO))
            {
                newCue.OutputDevice.OpenVideoPanel();
            }
            this._events.Add(newCue);
            this.sortEvents();
            this.setDuration();
        }

        protected Event eventFinishedPlayingCallback(Event finishedEvent)
        {
            this.eventsPlaying.Remove(finishedEvent); 
            return finishedEvent;
        }

        protected void playAtInterval(ulong time)
        {
            this.elapsedMilliseconds = time;
            for (int i = 0; i < this._events.Count; i++)
            {
                ulong startTime = this._events[i].startTime;
                ulong endTime = this._events[i].getEndTime();
                int preloadDuration = 10000;
                if ((int)time > ((int)startTime - preloadDuration) && time < endTime && !_events[i].loaded && !_events[i].Played)
                {
                    this._events[i].LoadAt(time);
                }
                if ((((int)time >= ((int)startTime - timerInterval / 2) && (int)time <= ((int)startTime + timerInterval / 2)))    
                || (time > startTime && time < endTime && !eventsPlaying.Contains(_events[i])))
                {
                    if (isScrubbing && this._events[i].type != Types.AUDIO)
                    {
                        continue;
                    }
                    if (!_events[i].Played)
                    {
                        if (!fading || (fading && !_events[i].type.Equals(Types.MIDI)))
                        {
                            this._events[i].PlayAt(time);
                            this.eventsPlaying.Add(this._events[i]);
                        }
                        if (fading) {
                            _events[i].getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.FADE, fadeSettings.Duration, (1.0f/ fadeSettings.Duration) * (elapsedMilliseconds-fadeStart)));

                        }
                    }
                }
                else if (time < this._events[i].startTime)
                {
                    break;
                }
            }
        }

        protected void startPlayAt(ulong time)
        {
            this.elapsedMilliseconds = time;
            for (int i = 0; i < this._events.Count; i++)
            {
                ulong startTime = this._events[i].startTime;
                ulong endTime = this._events[i].getEndTime();
                if (time == startTime || ((int)time > ((int)startTime - timerInterval / 2) && (int)time < ((int)endTime + timerInterval / 2)))
                {
                    if (isScrubbing && this._events[i].type != Types.AUDIO)
                    {
                        continue;
                    }
                    if (!_events[i].loaded)
                    {
                        this._events[i].LoadAt(time);
                    }
                    this._events[i].PlayAt(time);
                    this.eventsPlaying.Add(this._events[i]);
                }
            }
            this.isPlaying = true;
            timer.Start();
        }

        internal void DeselectEvents()
        {
            for (int i = 0; i < this._events.Count; i++)
            {
                _events[i].Select(false);
            }
        }

        public List<Event> getSelected() {
            List<Event> selected = new List<Event>();
            for (int i = 0; i < this._events.Count; i++)
            {
                if (_events[i].IsSelected())
                {
                    selected.Add(_events[i]);
                }
            }
            return selected;
        } 

        public void startPlayAt(Func<Boolean, Boolean> callback)
        {
            if (!this.isPlaying)
            {
                this.isPlaying = true;
                this.sceneFinishedCallback = callback;
                this.startPlayAt(this.elapsedMilliseconds);
            }
        }

        public void MIDICatchUp()
        {
            playMidiUntil(elapsedMilliseconds);
        }

        public void playMidiUntil(ulong time)
        {
            ObservableCollection<Event> oldMidi = getEventsThatStartBefore(time, Types.MIDI);
            for (int i = 0; i < oldMidi.Count; i++)
            {
                oldMidi[i].LoadAt(0);
                oldMidi[i].Play();
                System.Threading.Thread.Sleep(2);
            }
        }
        public int numberOfEventsBefore(String type)
        {
            return getEventsThatStartBefore(elapsedMilliseconds, type).Count;
        }

        protected ObservableCollection<Event> getEventsThatStartBefore(ulong time, String type)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            for (int i = 0; i < this._events.Count; i++)
            {
                ulong startTime = this._events[i].startTime;
                if (time > startTime)
                {
                    if (type == null || (type != null && this._events[i].type == type))
                    {
                        events.Add(this._events[i]);
                    }
                }
                else if (time < this._events[i].startTime)
                {
                    break;
                }
            }
            return events;
        }
        public ObservableCollection<Event> getEventsByOutputDevice(OutputDevice device)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            for (int i = 0; i < this._events.Count; i++)
            {
                if (this._events[i].OutputDevice == null)
                {
                    Console.WriteLine("OUTPUT DEVICE == NULL");
                }
                if (this._events[i].OutputDevice.deviceName.Equals(device.deviceName) && this._events[i].OutputDevice.type.Equals(device.type))
                {
                    events.Add(this._events[i]);
                }
            }
            return events;
        }

        /// <summary>
        /// Event Handler for the timer, executes ever 250 milliseconds
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void elapsedTimerEvent(Object source, ElapsedEventArgs e)
        {
            elapsedMilliseconds += (ulong)timerInterval;

            bool windowOpen = true;

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (!isScrubbing && isPlaying)
                {
                    setTickerLocation(elapsedMilliseconds);
                    MainWindow.main.scrollTimelineToTime(elapsedMilliseconds, this);
                }
                else {
                    updateScrubTicker();
                }
                MainWindow.main.updateTimelineInfo(source, null);
            });

            if (isScrubbing)
            {
                scrubStart = true;
                if (this.elapsedMilliseconds >= cursorTime)
                {
                    //stop playback
                    this.Pause();
                    scrubbingAudioPlaying = false;
                    startPoint = cursorTime;
                }
                else if ((this.elapsedMilliseconds < (cursorTime - (ulong)(timerInterval + 1))) && (cursorTime > (ulong)(timerInterval + 1)))
                {
                    //start playback
                    playAtInterval(this.elapsedMilliseconds);
                    scrubbingAudioPlaying = true;
                }
            }
            else if(isPlaying)
            {
                playAtInterval(elapsedMilliseconds);

                if (duration <= elapsedMilliseconds)
                {
                    this.Stop();
                }
            }
            if (!isPlaying && !isScrubbing) {
                timer.Stop();
                //this.Pause();
            }
        }

        private Stopwatch timeElapsed = new Stopwatch(); //for testing speed of fade

        internal void FadeToEnd(SceneFadeSettings fadeSettings)
        {
            Console.WriteLine("Fade out Scene: " + name + " over " + fadeSettings.Duration + " milliseconds");
            fadeSettings.StartFadeOnScene(this);
            fading = true;
            this.fadeSettings = fadeSettings;
            fadeStart = elapsedMilliseconds;

            timeElapsed.Reset();
            timeElapsed.Start();

            foreach (Event ev in eventsPlaying) {
                ev.getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.FADE, fadeSettings.Duration));
            }
            if (fadeSettings.StartMidiEvent != null)
            {
                fadeSettings.StartMidiEvent.getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.LOAD_AT));
                fadeSettings.StartMidiEvent.getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.PLAY));
            }
        }

        internal void StopEvent(Event ev) {
            Console.WriteLine("Finished event " + ev.name + " after " + timeElapsed.ElapsedMilliseconds);
        }

        internal void EndOfFade()
        {
            Console.WriteLine("End of Fade for Scene: " + name);
            if (fadeSettings.StartMidiEvent != null)
            {
                fadeSettings.EndMidiEvent.getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.LOAD_AT));
                fadeSettings.EndMidiEvent.getPlayer().queue.Add(new MediaPlayerCommand(MediaPlayerCommand.PLAY));
            }
            Console.WriteLine("Finished fade after " + timeElapsed.ElapsedMilliseconds);
            if (isPlaying)
            {
                Stop();
            }
            timeElapsed.Stop();
            fading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }

        public object ToJsonInstance()
        {
            return new JsonScene(this);
        }

        public void stopScrub()
        {
            this.Pause();
            scrubStart = false;
            MainWindow.main.Status = "PLAY";
        }

        //draw timeline ticker
        internal void drawTicker(Grid LaneGrid, Grid KeyGrid, List<OutputDevice> outputList)
        {
            Canvas ticker = new Canvas();
            ticker.Height = (outputList.Count * 55) + 5;
            ticker.Width = 5;

            tickBar = new Line();
            tickBar.X1 = tickBar.X2 = 0;
            tickBar.Stroke = Brushes.Black;
            if (!Mode.isEditMode) {
                tickBar.Stroke = Brushes.LightGray;
            }
            tickBar.StrokeThickness = 1.5;
            tickBar.Y1 = 0;
            tickBar.Y2 = ticker.Height;
            Canvas.SetTop(tickBar, 0);
            ticker.Children.Add(tickBar);
            tickBar.IsHitTestVisible = false;

            handle = new Polygon();
            PointCollection points = new PointCollection();
            points.Add(new Point(-5, 0));
            points.Add(new Point(-5, 10));
            points.Add(new Point(0, 15));
            points.Add(new Point(5, 10));
            points.Add(new Point(5, 0));
            handle.Points = points;
            if (Mode.isEditMode && !MainWindow.main.ShowView)
            {
                handle.Fill = Brushes.LightGray;
                handle.Stroke = Brushes.Black;
                handle.StrokeThickness = .5;

            }
            handle.MouseLeftButtonDown += TickerHandle_ClickDown;
            handle.MouseLeftButtonUp += TickerHandle_ClickUp;
            handle.MouseMove += TickerHandle_Drag;
            Canvas.SetTop(handle, 8);
            Canvas handleCanvas = new Canvas();
            handleCanvas.Children.Add(handle);
            if (KeyGrid != null)
            {
                KeyGrid.Children.Add(handleCanvas);
            }
            scrubBar = new Line();
            scrubBar.X1 = scrubBar.X2 = 0;
            scrubBar.StrokeDashArray = new DoubleCollection(new double[] { 3, 5 });
            scrubBar.Stroke = Brushes.Black;
            if (!Mode.isEditMode)
            {
                scrubBar.Stroke = Brushes.LightGray;
            }
            scrubBar.StrokeThickness = 1;
            scrubBar.Y1 = 0;
            scrubBar.Y2 = ticker.Height;
            ticker.Children.Add(scrubBar);
            scrubBar.IsHitTestVisible = false;

            tickBar.Height = ticker.Height;
            tickBar.Width = 1;
            tickBar.Fill = Brushes.Black;

            if (MainWindow.main.redrawTicker())
            {
                setTickerLocation();
            }
            ticker.HorizontalAlignment = HorizontalAlignment.Left;
            LaneGrid.Children.Add(ticker);

        }

        //update timeline ticker location
        public void setTickerLocation()
        {
            setTickerLocation(elapsedMilliseconds);
            if (!isPlaying)
            {
                foreach (OutputDevice vd in MainWindow.main.outputs.getOutputs(Types.VIDEO, false))
                {
                    ((VideoDevice)vd).videoClose();
                    MainWindow.main.setPreviewViewer((VideoDevice)vd, null);
                }
                for (int i = 0; i < this._events.Count; i++)
                {
                    if (elapsedMilliseconds >= _events[i].startTime && elapsedMilliseconds <= _events[i].getEndTime())
                    {
                        _events[i].LoadAt(elapsedMilliseconds);
                    }
                }
            }
        }

        public void setTickerLocation(ulong time)
        {
            if (tickBar != null && handle != null)
            {
                tickBar.Visibility = (MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying] == this) ? Visibility.Visible : Visibility.Hidden;
                handle.Visibility = (MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying] == this) ? Visibility.Visible : Visibility.Hidden;

                Canvas.SetLeft(tickBar, MainWindow.main.scaleToScreen(time));
                Canvas.SetLeft(handle, MainWindow.main.scaleToScreen(time));
                updateScrubTicker();
            }
        }

        private void updateScrubTicker() {
            ulong scrubAt = elapsedMilliseconds;
            if (isScrubbing && cursorTime < elapsedMilliseconds)
            {
                scrubAt = cursorTime;
            }
            scrubBar.Visibility = isScrubbing ? Visibility.Visible : Visibility.Hidden;
            Canvas.SetLeft(scrubBar, MainWindow.main.scaleToScreen(scrubAt));
        }

        //draw output lanes and fill them with events
        internal void drawLanes(Grid LaneGrid, Grid KeyGrid, ScrollViewer Timeline, List<OutputDevice> outputList)
        {
            lanes = new List<Rectangle>();

            double laneLength = MainWindow.main.scaleToScreen(MainWindow.main.ShowView ? duration : (ulong)(duration * TimelineInfo.showRatio));
            if (Events.Count < 1 && MainWindow.main.ShowView) {
                laneLength = 50;
            }
            double visableLaneLength = laneLength;
            if (Timeline != null && Timeline.ViewportWidth > laneLength)
            {
                visableLaneLength = (int)Math.Ceiling(Timeline.ViewportWidth);
            }
            if (!MainWindow.main.ShowView || (MainWindow.main.sceneList.Scenes[0] == this))
            {
                if (Timeline != null && KeyGrid != null)
                {
                    KeyGrid.Children.Clear();
                    timeKey = new Canvas();
                    buildTimeKey(visableLaneLength);
                    Grid.SetRow(timeKey, 0);
                    KeyGrid.Children.Add(timeKey);
                }
            }

            RowDefinition sep = new RowDefinition();
            sep.Height = new GridLength(5);
            LaneGrid.RowDefinitions.Add(sep);
            Separator top = new Separator();
            top.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
            Grid.SetRow(top, 0);
            LaneGrid.Children.Add(top);

            LaneGrid.SizeChanged += TimelineResized;

            for (int j = 1; j <= outputList.Count; j++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50);
                LaneGrid.RowDefinitions.Add(row);

                RowDefinition row2 = new RowDefinition();
                row2.Height = new GridLength(5);
                LaneGrid.RowDefinitions.Add(row2);

                Canvas lane = new Canvas();
                lane.Height = 50;
                if(laneLength > 0) lane.Width = laneLength;
                lane.AllowDrop = true;

                Rectangle background = new Rectangle();
                background.Height = 50;
                background.Width = visableLaneLength;
                background.MouseEnter += HightlightValidLane;
                background.Fill = new SolidColorBrush(ColorManager.BACKGROUND);
                background.DataContext = outputList[j - 1];
                lane.Children.Add(background);

                addEventsToLane(lane, outputList[j - 1]);
                lanes.Add(background);
                lane.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(lane, (j * 2) -1);
                lane.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
                LaneGrid.Children.Add(lane);

                ContextMenu timelineContext = new ContextMenu();
                MenuItem newEvent = new MenuItem();
                newEvent.Header = "Add Event";
                newEvent.Click += LaneRightClick_CreateEvent;
                newEvent.IsEnabled = Mode.isEditMode && !MainWindow.main.ShowView;
                newEvent.DataContext = outputList[j - 1];
                timelineContext.Items.Add(newEvent);
                lane.ContextMenu = timelineContext;
                lane.MouseLeftButtonDown += LaneClicked;
                lane.MouseLeftButtonUp += MoveTicker;
                lane.MouseRightButtonDown += LaneClicked;

                Separator separator = new Separator();
                separator.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
                Grid.SetRow(separator, (j * 2));
                LaneGrid.Children.Add(separator);
            }
            if (Timeline != null)
            {
                Timeline.Height = MainWindow.main.TimelineArea.ViewportHeight + MainWindow.main.TimelineArea.VerticalOffset;
            }
            drawTicker(LaneGrid, KeyGrid, outputList);
        }

        private void Timeline_ScrollChanged(object sender, MouseWheelEventArgs e)
        {
            MainWindow.main.Timeline_ScrollChanged(sender, e);
        }

        //draw timeline time key
        private void buildTimeKey(double laneLength)
        {
            timeKey.Children.Clear();
            timeKey.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
            double sceneLength = MainWindow.main.scaleToScreen((ulong)(this.duration * TimelineInfo.showRatio));
            timeKey.Width = (laneLength > sceneLength) ? laneLength : sceneLength;
            timeKey.HorizontalAlignment = HorizontalAlignment.Left;
            timeKey.MouseLeftButtonUp += MoveTicker;
            double majorMarkerDistance = 120;
            int majorMarkNum = (int)Math.Ceiling(laneLength / majorMarkerDistance);
            int minorMarkNum = 3;

            Line baseLine = new Line();
            baseLine.Stroke = Brushes.Black;
            if (!Mode.isEditMode)
            {
                baseLine.Stroke = Brushes.LightGray;
            }
            baseLine.Y1 = baseLine.Y2 = 23;
            baseLine.X1 = 0;
            baseLine.X2 = laneLength;
            timeKey.Children.Add(baseLine);

            for (int i = 0; i < majorMarkNum; i++)
            {
                Line mark = new Line();
                mark.X1 = mark.X2 = 0.5 + (majorMarkerDistance * i);
                mark.Stroke = Brushes.Black;
                if (!Mode.isEditMode)
                {
                    mark.Stroke = Brushes.LightGray;
                }
                mark.StrokeThickness = 1;
                mark.Y1 = 13;
                mark.Y2 = 23;
                timeKey.Children.Add(mark);
                TextBlock label = new TextBlock();
                label.Text = TimelineTimeConverter.formatTime(MainWindow.main.scaleToTime((majorMarkerDistance * i)));
                label.Foreground = Brushes.Black;
                if (!Mode.isEditMode)
                {
                    label.Foreground = Brushes.LightGray;
                }
                Canvas.SetLeft(label, majorMarkerDistance * i);
                timeKey.Children.Add(label);
                for (int j = 1; j <= minorMarkNum; j++)
                {
                    Line minorMark = new Line();
                    minorMark.X1 = minorMark.X2 = (0.5 + (majorMarkerDistance * i)) + ((majorMarkerDistance / (minorMarkNum + 1)) * j);
                    minorMark.Stroke = Brushes.Black;
                    if (!Mode.isEditMode)
                    {
                        minorMark.Stroke = Brushes.LightGray;
                    }
                    minorMark.StrokeThickness = 1;
                    minorMark.Y1 = 20;
                    minorMark.Y2 = 23;
                    timeKey.Children.Add(minorMark);
                }
            }
        }

        private void HightlightValidLane(object sender, MouseEventArgs e)
        {
            Event @event = MainWindow.main.GetEventDragging();
            if (@event != null)
            {
                String type = (!@event.OutputDevice.type.Equals(Types.GHOST)) ? @event.OutputDevice.type : @event.OutputDevice.ghostType;
                if (((sender as Rectangle).DataContext as OutputDevice).type.Equals(type) || ((sender as Rectangle).DataContext as OutputDevice).ghostType.Equals(type))
                {
                    highlightLane(lanes.IndexOf((sender as Rectangle)));
                    return;
                }
            }
            highlightLane(-1);
        }

        //highlight output lane by index
        internal void highlightLane(int index)
        {
            for (int i = 0; i < lanes.Count; i++)
            {
                if (i == index)
                {
                    if (!MainWindow.main.outputs.DisplayList[i].getDeviceMute())
                    {
                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(ColorManager.HIGHLIGHT);
                        lanes[i].Fill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                    else
                    {
                        lanes[index].Fill = new SolidColorBrush(ColorManager.HIGHLIGHTMUTEDLANE);
                    }
                }
                else
                {
                    GreyOutLane(i, MainWindow.main.outputs.DisplayList[i].getDeviceMute());
                }
            }
        }

        internal void GreyOutLane(int index, bool grey)
        {
            if (grey)
            {
                if (index > 0 && index < lanes.Count)
                {
                    lanes[index].Fill = new SolidColorBrush(ColorManager.ACCENT);
                }
            }
            else
            {
                if (index > 0 && index < lanes.Count)
                {
                    lanes[index].Fill = new SolidColorBrush(ColorManager.BACKGROUND);
                }                                                                  
            }
        }

        //populates output lanes with events
        private void addEventsToLane(Canvas lane, OutputDevice device)
        {
            ObservableCollection<Event> events = getEventsByOutputDevice(device);
            for (int i = 0; i < events.Count; i++)
            {
                Canvas TimelineEvent = events[i].drawEvent();
                Canvas.SetLeft(TimelineEvent, MainWindow.main.scaleToScreen(events[i].startTime));
                if (TimelineEvent.Parent != null)
                {
                    (TimelineEvent.Parent as Canvas).Children.Remove(TimelineEvent);
                }
                lane.Children.Add(TimelineEvent);
            }
            
        }

        //widens lanes to fill screen when scene duration takes up less space then the screen shows
        private void TimelineResized(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged && !MainWindow.main.ShowView)
            {
                double laneLength = MainWindow.main.scaleToScreen((ulong)(this.duration * TimelineInfo.showRatio));
                ScrollViewer scrollArea = ((sender as Grid).Parent as Grid).Parent as ScrollViewer;
                if (scrollArea.ViewportWidth > laneLength)
                {
                    laneLength = scrollArea.ViewportWidth;
                }
                for (int i = 0; i < lanes.Count; i++)
                {
                    lanes[i].Width = laneLength;
                }
                buildTimeKey(laneLength);
            }
        }

        //click lane
        private void MoveTicker(object sender, MouseButtonEventArgs e)
        {
            if (!isPlaying && Mode.isEditMode && e.GetPosition(sender as Canvas).X == clickX && !MainWindow.main.sceneList.isPlaying)
            {
                ignoreSingleClick();
                clickTimer = new Timer();
                clickTimer.Interval = 1;
                clickTimer.Elapsed += onlySingleClick;
                clickX = e.GetPosition(sender as Canvas).X;
                clickTimer.Start();
            }
        }

        public void ignoreSingleClick() {
            clickTimer.Stop();
        }

        private void onlySingleClick(object sender, ElapsedEventArgs e)
        {
            ignoreSingleClick();
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if ((MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying] != this))
                {
                    MainWindow.main.sceneList.SetCurrentScene(this);
                }
                elapsedMilliseconds = MainWindow.main.scaleToTime(clickX);
                setTickerLocation();
                MainWindow.main.updateTimelineInfo(sender, null);
            });
            MainWindow.main.Status = "PLAY";
        }

        private void LaneClicked(object sender, MouseButtonEventArgs e)
        {
            clickX = e.GetPosition(sender as Canvas).X;
        }

        //add event to timeline by double clicking a timeline lane
        private void LaneRightClick_CreateEvent(object sender, EventArgs e)
        {
            if ((MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying] != this)) {
                MainWindow.main.sceneList.SetCurrentScene(this);
            }
            OutputDevice device = (sender as MenuItem).DataContext as OutputDevice;
            ContextMenu menu = (sender as MenuItem).Parent as ContextMenu;
            String type = device.type;
            if (device.type.Equals(Types.GHOST)) {
                type = device.ghostType;
            }
            createSceneEventFromTimeline(MainWindow.main.scaleToTime(clickX), type, device, MainWindow.main.commandList);
            //MainWindow.main.newEvent(MainWindow.main.scaleToTime(clickX), type, device);
        }

        //on timeline handle mouse clicked
        private void TickerHandle_ClickDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode.isEditMode && !MainWindow.main.sceneList.isPlaying)
            {
                isScrubbing = true;
                this.isPlaying = true;
                var pos = e.GetPosition(MainWindow.main.LaneGrid);
                startPlayAt(MainWindow.main.scaleToTime(pos.X));
                (sender as Polygon).CaptureMouse();
            }
        }

        //on timeline handle mouse released
        private void TickerHandle_ClickUp(object sender, MouseButtonEventArgs e)
        {
            if (isScrubbing)
            {
                stopScrub();
                isScrubbing = false;
                (sender as Polygon).ReleaseMouseCapture();
                double Xpos = e.GetPosition((sender as Polygon).Parent as Canvas).X;
                if (Xpos < 0)
                {
                    Xpos = 0;
                }
                if (Xpos > MainWindow.main.scaleToScreen(duration) && MainWindow.main.ShowView)
                {
                    Xpos = MainWindow.main.scaleToScreen(duration);
                }
                Canvas.SetLeft(sender as Polygon, Xpos);
                elapsedMilliseconds = MainWindow.main.scaleToTime(Xpos);
                setTickerLocation();
                MainWindow.main.updateTimelineInfo(sender, null);
            }
        }

        //on timeline handle mouse dragged
        private void TickerHandle_Drag(object sender, MouseEventArgs e)
        {
            if (isScrubbing)
            {
                double Xpos = e.GetPosition((sender as Polygon).Parent as Canvas).X;
                if (Xpos < 0)
                {
                    Xpos = 0;
                }
                if (Xpos > MainWindow.main.scaleToScreen(duration) && MainWindow.main.ShowView)
                {
                    Xpos = MainWindow.main.scaleToScreen(duration);
                }
                ulong time = MainWindow.main.scaleToTime(Xpos);
                setTickerLocation(time);
                setCursorTime(time);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);
            info.AddValue("name", name);
            info.AddValue("audoPlay", autoPlay);
            info.AddValue("duration", duration);
            info.AddValue("events", _events);
        }

        public bool getScrubStart()
        {
            return scrubStart;
        }

        public object Clone()
        {
            Scene clone = new Scene();
            clone.name = this.name + " Copy";
            clone.autoPlay = this.autoPlay;
            clone.duration = this.duration;
            foreach (Event ev in _events)
            {
                clone.Events.Add(ev.CloneTo(clone));
            }
            foreach (Event ev in clone.Events)
            {
                //Fix duplicated events groups
                EventGroup group = ev.group;
                if (group != null)
                {
                    ev.group = null;
                    ev.group = new EventGroup();
                    foreach (Event gr in group.GetElements())
                    {
                        foreach (Event duplicates in clone.Events)
                        {
                            if (duplicates.name.Equals(gr.name) && gr.startTime.Equals(duplicates.startTime))
                            {
                                ev.group.Add(duplicates);
                                break;
                            }
                        }
                    }
                }
            }
            return clone;
        }
    }

    /// <summary>
    /// Saves scene info to file 
    /// </summary>
    public class JsonScene
    {
        public List<object> events = new List<object>();
        public String id;
        public String name;
        public Boolean autoPlay;
        public ulong duration;
        public JsonScene(Scene scene)
        {
            for (var i = 0; i < scene.Events.Count; i++)
            {
                Event ev = scene.Events[i];
                this.events.Add(ev.ToJsonInstance());
            }
            this.duration = scene.duration;
            this.autoPlay = scene.autoPlay;
            this.name = scene.name;
            this.id = scene.id.ToString();
        }
    }
}
