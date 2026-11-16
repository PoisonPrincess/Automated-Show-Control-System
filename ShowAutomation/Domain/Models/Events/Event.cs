using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Runtime.Serialization;
using ShowAutomation.Domain.Models.Media;
using System.IO;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Actions;

namespace ShowAutomation.Domain.Models.Events
{
    [Serializable()]
    public abstract class Event : INotifyPropertyChanged, ISerializable, ICloneable
    {
        public AbstractCommand command;
        public String name { get; set; }
        public ulong startTime = 0;
        public ulong duration = 0;
        public static ulong endTime = 0;
        public EventGroup group = null;

        public String startTimeFormatted { get; set; }
        public String durationFormatted { get; set; }
        public String endTimeFormatted { get; set; }

        public String type { get; set; }
        protected Media.MediaPlayer.MediaPlayer player;

        protected OutputDevice device;
        public OutputDevice OutputDevice
        {
            get => device;
            set
            {
                device = value;
                if (player != null) player.SetDevice(value);
            }
        }
        public String listItemColor
        {
            get
            {

                if (_isPlaying)
                {
                    return ColorManager.IS_PLAYING_COLOR;
                }
                return ColorManager.BACKGROUND.ToString();
            }
            set { }
        }
        public String listItemTextColor
        {
            get
            {
                return ColorManager.TEXT_COLOR;
            }
            set { }
        }
        protected Scene scene = null;
        protected Func<Event, Event> finishPlaybackCallback = null;
        protected Boolean _isPlaying = false;
        public Boolean isPlaying {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
                OnPropertyChanged("isPlaying");
                OnPropertyChanged("listItemColor");
            }
        }
        protected Timer timer = new Timer();

        //UI elements
        private Rectangle eventRect = new Rectangle();
        protected Canvas TimelineEvent = new Canvas();
        private Boolean beingDraged = false;
        private Boolean moved = false;
        private Point mousePosStart;
        private Boolean isSelected = false;
        internal bool loaded = false;
        public bool Played = false;

        //private MenuItem groupEvents;

        public Guid id { get; set; }

        public Event(AbstractCommand command, ulong startTime)
        {
            Console.WriteLine("load command " + command);
            isPlaying = false;
            this.command = command;
            name = command.name;
            setStartTime(startTime);
            timer.Interval = duration;
            timer.Elapsed += finishPlaying;
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
            }

            endTime = this.startTime + this.duration;
            this.endTimeFormatted = TimelineTimeConverter.formatTime(endTime);
        }

        public Event(AbstractCommand command, ulong startTime, Scene scene) : this(command, startTime)
        {
            this.scene = scene;
        }

        public Event(AbstractCommand command, ulong startTime, Scene scene, Guid id) : this(command, startTime, scene)
        {
            this.id = id;
        }

        public Event(SerializationInfo info, StreamingContext context) : this((AbstractCommand)info.GetValue("command", typeof(AbstractCommand)), (ulong)info.GetValue("startTime", typeof(ulong)), (Scene)info.GetValue("scene", typeof(Scene)), (Guid)info.GetValue("id", typeof(Guid)))
        {
            type = (String)info.GetValue("type", typeof(string));
            duration = (ulong)info.GetValue("duration", typeof(ulong));
            group = (EventGroup)info.GetValue("group", typeof(EventGroup));
            OutputDevice = (OutputDevice)info.GetValue("outputDevice", typeof(OutputDevice));
            if (this.OutputDevice != null && !MainWindow.main.outputs.outputList.Contains(this.OutputDevice))
            {
                //Remove old devices and replace with new to clean out duplicate devices and remove any issues with remembering old hardware setups
                foreach (OutputDevice device in MainWindow.main.outputs.outputList)
                {
                    if (OutputDevice.deviceName.Equals(device.deviceName) && OutputDevice.type.Equals(device.type))
                    {
                        OutputDevice = device;
                        break;
                    }
                }
            }
            Console.WriteLine("Load Event with name: " + name + " and command " + command.name + "on output: " + OutputDevice.deviceName);
        }

        private void finishPlaying(Object source, ElapsedEventArgs e)
        {
            if (finishPlaybackCallback != null)
            {
                finishPlaybackCallback(this);
            }
            this.Stop();
            return;
        }

        public void setStartTime(ulong startTime)
        {
            this.startTime = startTime;
            this.startTimeFormatted = TimelineTimeConverter.formatTime(startTime);
            this.duration = this.command.getDuration();
            this.durationFormatted = TimelineTimeConverter.formatTime(this.duration);
            endTime = this.startTime + this.duration;
            this.endTimeFormatted = TimelineTimeConverter.formatTime(endTime);
        }

        public void setCommand(AbstractCommand command)
        {
            this.command = command;
            this.name = command.name;
            this.setStartTime(startTime);
        }

        public void setCallback(Func<Event, Event> callback)
        {
            finishPlaybackCallback = callback;
        }

        public Boolean isConflicting(Event eventToTest)
        {
            //Sorry for anyone who has to read this, I truly am (Bring out those truth tables)
            if (eventToTest.type == Types.VIDEO)
            {
                return false;
            }
            return (
                    (!eventToTest.id.Equals(this.id))
                &&
                (
                    (this.OutputDevice.deviceName.Equals(eventToTest.OutputDevice.deviceName))
                    &&
                    (
                        (
                            (startTime < eventToTest.startTime)
                            &&
                            (
                                (eventToTest.getEndTime() < this.getEndTime()) || (eventToTest.startTime < this.getEndTime())
                            )
                        ) || (startTime > eventToTest.startTime && eventToTest.getEndTime() > startTime)
                        || (startTime == eventToTest.startTime)
                    )
                )
            );
        }

        public void LoadAt(ulong time) {
            ulong eventTime = (startTime <= time) ? time - startTime : 0;
            OutputDevice.LoadCommandAt(this, eventTime);
            loaded = true;
        }

        public void PlayAt(ulong time)
        {
            if (loaded)
            {
                if ((!OutputDevice.getDeviceMuteBySolo() && !OutputDevice.getDeviceMute()) || OutputDevice.getDeviceSolo())
                {
                    this.isPlaying = true;
                    ulong eventTime = (startTime < time) ? time - startTime : 0;

                    OutputDevice.PlayCommandAt(this, eventTime);

                    timer.Interval = (this.duration - eventTime > 0) ? (this.duration - eventTime < Int32.MaxValue) ? this.duration - eventTime : 1 : 1;
                    timer.Start();
                    Played = true;
                }
            }
            else {
                throw new Exception("Event being told to Play and it isn't loaded Yet");
            }
        }

        public void Play()
        {
            if ((!OutputDevice.getDeviceMuteBySolo() && !OutputDevice.getDeviceMute()) || OutputDevice.getDeviceSolo())
            {
                this.isPlaying = true;
                OutputDevice.PlayCommandAt(this, 0);
                timer.Start();
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                //this.command.pause();
                player.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.PAUSE));
                isPlaying = false;
            }
            loaded = false;
        }

        public void Stop()
        {
            this.isPlaying = false;
            //this.command.stop();
            player.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.STOP));
            this.timer.Stop();
            loaded = false;
        }

        public ulong getEndTime()
        {
            return this.startTime + this.duration;
        }

        public Scene getScene()
        {
            return this.scene;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }

        public abstract object ToJsonInstance();

        //draw event as shown on timeline
        public virtual Canvas drawEvent() {
            TimelineEvent = new Canvas();//.Children.Clear();
            eventRect = new Rectangle();
            //draw background rectangle
            eventRect.Height = 50;
            if (type == Types.MIDI)
            {
                eventRect.Width = 10;
            }
            else
            {
                if (MainWindow.main.scaleToScreen(duration) > 0) eventRect.Width = MainWindow.main.scaleToScreen(duration);
            }
            eventRect.Stroke = Brushes.Black;
            eventRect.Fill = new SolidColorBrush(getColor());
            TimelineEvent.Children.Add(eventRect);

            TextBlock name = new TextBlock();
            name.Text = this.name;
            Canvas.SetLeft(name, 4);
            name.Width = MainWindow.main.scaleToScreen(duration) > eventRect.Height ? MainWindow.main.scaleToScreen(duration) - 8 : eventRect.Height - 8;
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            if (MainWindow.main.scaleToScreen(duration) < eventRect.Height)
            {
                name.RenderTransform = new RotateTransform(90);
                Canvas.SetLeft(name, 15);
                Canvas.SetTop(name, 4);
            }
            double backgroundBrightness = Math.Sqrt(0.299 * Math.Pow(getColor().R, 2) + 0.587 * Math.Pow(getColor().G, 2) + 0.114 * Math.Pow(getColor().B, 2));
            name.Foreground = backgroundBrightness > 140 ? Brushes.Black : Brushes.White;
            TimelineEvent.Children.Add(name);

            ContextMenu eventContext = new ContextMenu();
            MenuItem editEvent = new MenuItem();
            editEvent.Header = "Edit Event";
            editEvent.Click += EditEvent_Event;
            eventContext.Items.Add(editEvent);

            MenuItem groupEvents = new MenuItem();
            groupEvents.Header = "Group Events";
            groupEvents.InputGestureText = "Ctrl-G";
            groupEvents.IsEnabled = false;
            eventContext.Items.Add(groupEvents);

            AddEventSpecificContextMenuItems(eventContext);

            eventContext.Items.Add(new Separator());

            MenuItem deleteEvent = new MenuItem();
            deleteEvent.Header = "Delete Event";
            deleteEvent.DataContext = this;
            deleteEvent.Click += DeleteEvent_Event;
            eventContext.Items.Add(deleteEvent);

            TimelineEvent.ContextMenu = eventContext;
            TimelineEvent.ContextMenuOpening += assembleContextMenu;
            TimelineEvent.ToolTip = this.name;
            TimelineEvent.MouseLeftButtonDown += TimelineEvent_ClickDown;
            TimelineEvent.MouseLeftButtonUp += TimelineEvent_ClickUp;
            TimelineEvent.MouseMove += TimelineEvent_Drag;

            return TimelineEvent;
        }

        protected abstract void AddEventSpecificContextMenuItems(ContextMenu contextMenu);

        private void assembleContextMenu(object sender, ContextMenuEventArgs e)
        {
            ContextMenu menu = (sender as Canvas).ContextMenu;
            for (int i = 0; i < menu.Items.Count; i++) {
                MenuItem item = menu.Items[i] as MenuItem;
                if (item != null && item.Header.Equals("Ungroup Event")) {
                    menu.Items.RemoveAt(i);
                }
            }

            MenuItem groupEvents = menu.Items[1] as MenuItem;
            groupEvents.IsEnabled = false;
            groupEvents.Click -= DissolveGroup_Event;
            groupEvents.Click -= GroupEvents_Event;

            List<Event> selected = scene.getSelected();
            if (isSelected && selected.Count > 1 && (group == null || (group != null && !group.Matches(selected))))
            {
                groupEvents.Header = "Group Events";
                groupEvents.Click += GroupEvents_Event;
                groupEvents.IsEnabled = true;
            }
            else if (group != null && group.Matches(selected))
            {
                groupEvents.Header = "Dissolve Group";
                groupEvents.Click += DissolveGroup_Event;
                if (!group.IsLocked())
                {
                    groupEvents.IsEnabled = true;
                }
            }

            if (group != null)
            {
                MenuItem removeItem = new MenuItem();
                removeItem.Header = "Ungroup Event";
                removeItem.IsEnabled = !group.IsLocked();
                removeItem.Click += UngroupEvent_Event;
                menu.Items.Insert(2, removeItem);
            }
            groupEvents.DataContext = selected;
            if (!Mode.isEditMode) {
                for (int i = 0; i < menu.Items.Count; i++) {
                    if (menu.Items[i] is MenuItem) {
                        (menu.Items[i] as MenuItem).IsEnabled = false;
                    }
                }
            }
        }

        public void Select(Boolean select) {
            if (select)
            {
                eventRect.StrokeThickness = 2;
                isSelected = true;
            }
            else {
                eventRect.StrokeThickness = 1;
                isSelected = false;
            }
        }

        public Boolean IsSelected() {
            return isSelected;
        }

        public void deleteSelected()
        {
            if (this.isSelected)
            {
                Scene scene = this.getScene();
                scene.removeEvent(this);
                MainWindow.main.DrawTimeline();
                MainWindow.main.sceneList.changesMade = true;
            }
        }

        public Canvas GetCanvas() {
            return TimelineEvent;
        }

        private void EditEvent_Event(object sender, RoutedEventArgs e)
        {
            MainWindow.main.EditEvent(scene, this);
        }

        public void GroupEvents_Event(object sender, RoutedEventArgs e)
        {
            List<Event> selected = (sender as MenuItem).DataContext as List<Event>;
            group = new EventGroup();
            group.AddAll(selected);
            MainWindow.main.DoAction(new CreateGroupAction(group));
        }

        public void UngroupEvent_Event(object sender, RoutedEventArgs e)
        {
            MainWindow.main.DoAction(new UngroupAction(this, group));
        }

        public void DissolveGroup_Event(object sender, RoutedEventArgs e)
        {
            MainWindow.main.DoAction(new DissolveGroupAction(group));
        }

        private void DeleteEvent_Event(object sender, RoutedEventArgs e)
        {
            MainWindow.main.btnDeleteEventClick(sender, e);
        }

        public virtual void Ungroup()
        {
            group = null;
        }

        public Color getColor()
        {
            return command.color;
        }

        //on event mouse clicked
        private void TimelineEvent_ClickDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode.isEditMode && !MainWindow.main.sceneList.isPlaying && !MainWindow.main.ShowView)
            {
                var video = command.media as Video;
                var song = command.media as Song;
                // Don't allow event to be grouped or dragged if files doesnt exist
                if (video != null && !File.Exists(video.videoPath))
                {
                    return;
                }
                if (song != null && !File.Exists(song.songPath))
                {
                    return;
                }
                scene.ignoreSingleClick();
                beingDraged = true;
                SimplifiyRendering();
                moved = false;
                mousePosStart = e.GetPosition((sender as Canvas).Parent as Canvas);
                (sender as Canvas).CaptureMouse();
                if (!(System.Windows.Forms.Form.ModifierKeys == System.Windows.Forms.Keys.Shift)) {
                    MainWindow.main.sceneList.DeselectEvents();
                }
                if (group != null)
                {
                    bool select = isSelected;
                    for (int i = 0; i < group.GetElements().Count; i++)
                    {
                        group.GetElements()[i].Select(!select);
                    }
                }
                else
                {
                    Select(!isSelected);
                }

                scene.ignoreSingleClick();
            }
        }

        //on event mouse released
        private void TimelineEvent_ClickUp(object sender, MouseButtonEventArgs e)
        {
            if (beingDraged)
            {
                beingDraged = false;
                (sender as Canvas).ReleaseMouseCapture();
                if (moved)
                {
                    double Xpos = e.GetPosition((sender as Canvas).Parent as Canvas).X;
                    if (Xpos < 0)
                    {
                        Xpos = 0;
                    }
                    if (group != null)
                    {
                        double minTime = MainWindow.main.scaleToScreen(startTime - group.GetMinStartTime());
                        if (Xpos < minTime)
                        {
                            Xpos = minTime;
                        }
                        double movedAmount = Xpos - Canvas.GetLeft(sender as Canvas);
                        group.DragOffset(movedAmount);
                    }
                    MainWindow.main.DoAction(new EditEventAction(this, MainWindow.main.scaleToTime(Xpos)));
                }
            }
        }

        //on event mouse moved
        private void TimelineEvent_Drag(object sender, MouseEventArgs e)
        {
            scene.ignoreSingleClick();
            if (beingDraged) {
                double Xpos = e.GetPosition((sender as Canvas).Parent as Canvas).X;
                double Ypos = e.GetPosition((sender as Canvas).Parent as Canvas).Y;
                if (Math.Abs(mousePosStart.X - Xpos) > 1) {
                    moved = true;
                }
                if (Ypos < 0 || Ypos > ((sender as Canvas).Parent as Canvas).Height) {
                    beingDraged = false;
                    (sender as Canvas).ReleaseMouseCapture();
                    MainWindow.main.EventDragged(this, e);
                }
                if (moved)
                {
                    if (Xpos < 0)
                    {
                        Xpos = 0;
                    }
                    if (group != null) {
                        double minTime = MainWindow.main.scaleToScreen(startTime - group.GetMinStartTime());
                        if (Xpos < minTime)
                        {
                            Xpos = minTime;
                        }
                        double movedAmount = Xpos - Canvas.GetLeft(sender as Canvas);
                        group.DragOffset(movedAmount);
                    }
                    Canvas.SetLeft(sender as Canvas, Xpos);
                    if (MainWindow.main.ShowView)
                    {
                        ((sender as Canvas).Parent as Canvas).Width = Xpos + MainWindow.main.scaleToScreen(group != null ? group.GetMaxEndTime() - startTime : duration);
                    }
                }
            }
        }
        internal Media.MediaPlayer.MediaPlayer getPlayer()
        {
            return player;
        }

        protected virtual void SimplifiyRendering()
        {

        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("command", command);
            info.AddValue("type", type);
            info.AddValue("startTime", startTime);
            info.AddValue("duration", duration);
            info.AddValue("scene", scene);
            info.AddValue("id", id);
            info.AddValue("group", group);
            info.AddValue("outputDevice", device);
        }

        public Event CloneTo(Scene scene) {
            Event clone = (Event)this.Clone();
            clone.scene = scene;
            return clone;
        }

        public abstract object Clone();
    }

    public abstract class JsonEvent
    {
        public object command;
        public String type;
        public ulong startTime;
        public ulong duration;
        public string id;
        public object group;
        public JsonEvent (Event ev)
        {
            id = ev.id.ToString();
            this.type = ev.type;
            this.startTime = ev.startTime;
            this.duration = ev.duration;
        }
    }
}
