using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ShowAutomation.Domain.Models;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Commands;
using ShowAutomation.UI.ViewModels.Scenes;
using ShowAutomation.UI.Views;
using System.Windows.Threading;
using System.ComponentModel;
using ShowAutomation.Domain.Utils.Midi;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using ShowAutomation.UI.ViewModels;
using System.Windows.Documents;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Interop;
using ShowAutomation.UI.ViewModels.OutputDevices;
using NAudio.CoreAudioApi;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.ViewModels.Adorners;
using System.Globalization;
using System.Timers;
using ShowAutomation.Domain.Actions;

namespace ShowAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CommandList commandList = new CommandList();
        private static readonly String extention = ".ascs";
        public String projectfileName;
        public SceneList sceneList = new SceneList();
        public OutputManager outputs;
        private TimelineInfo timelineInfo;
        public static MainWindow main;
        private bool MidiShouldCatchUp = true;
        private Boolean changesMade = false;
        public String searchTerm;

        //Scene Tabs/Labels
        private StackPanel SceneTabs = new StackPanel();
        private Rectangle keySpacer = new Rectangle();
        private Canvas SceneLabels = new Canvas();
        private List<Event> SelectedEvents;

        //drag and drop vars
        private bool draggingCommand = false;
        private bool draggingEvent = false;
        private bool draggingScene = false;
        private ListViewItem DragListItem;
        private Event DragEvent;
        private Scene DragScene;
        private Point dragStartLocation = new Point(-1000, -1000);
        private OutputDevice StartingOutput = null;
        private int StartingIndex = -1;
        private DragAdorner hover = null;
        public bool KeyPreview;

        //Video Preview Vars
        private VideoDevice previewDevice;
        private bool previewMute = false;

        private bool CONSOLE_VISIBLE = true; //SETS CONSOLE VISIBILITY
        private int countDevice = 0;

        //Undo-Redo Vars
        private ActionManager actionManager = new ActionManager();

        //Scrolling vars
        private double zoomValue = 64.7;
        private bool zooming = false;
        private bool scrolledThisInterval = false;
        private bool redrawAtInterval = false;

        //Command list Vars
        private Timer CommandTimer;
        private Button playingButton;
        private MenuItem playingMenuItem;
        private Event previewEvent;

        public MainWindow()
        {

            InitializeComponent();
            if (CONSOLE_VISIBLE)
            {
                ConsoleManager.Show();
            }
            else
            {
                ConsoleManager.Hide();
            }
            main = this;
            outputs = new OutputManager();
            timelineInfo = new TimelineInfo();
            this.KeyPreview = true;
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = false;
            UpdateOutputDevices();
            setDataContexts();
            SetUpTimelineTabs();
            this.KeyDown += new KeyEventHandler(keyPlay);
            this.AddHotKeys();
            this.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
            Timeline.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);

            UpdateSceneTabs(new ObservableCollection<Scene>());

            //add crash listeners
            AppDomain.CurrentDomain.UnhandledException += handleCrash;
        }

        //on Application started add Window Update Handeler
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WndUpdates);
            //should add source to device notification stuff
        }

        private int screens = System.Windows.Forms.Screen.AllScreens.Length;
        private int speakers = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Count;
        private int midiOutputs = NAudio.Midi.MidiOut.NumberOfDevices;

        public bool ShowView = false;

        //gets called on window updates
        private IntPtr WndUpdates(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            //screens change tested and working
            if (screens != System.Windows.Forms.Screen.AllScreens.Length)
            {
                screens = System.Windows.Forms.Screen.AllScreens.Length;
                Console.WriteLine("Screens changed");
                UpdateOutputDevices();
            }
            //speakers change tested and working
            if (speakers != new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Count)
            {
                speakers = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Count;
                Console.WriteLine("Speakers changed");
                UpdateOutputDevices();
            }
            //midi change tested and sometimes creates multiple devices for a device
            if (midiOutputs != NAudio.Midi.MidiOut.NumberOfDevices)
            {
                midiOutputs = NAudio.Midi.MidiOut.NumberOfDevices;
                Console.WriteLine("Midi Devices changed");
                MidiDeviceManager.Instance.RefreshMidiOutDevices();
                UpdateOutputDevices();
            }
            return IntPtr.Zero;
        }

        private void SetUpTimelineTabs()
        {
            if (keySpacer.Parent != null) (keySpacer.Parent as Grid).Children.Remove(keySpacer);
            if (SceneLabels.Parent != null) (SceneLabels.Parent as Grid).Children.Remove(SceneLabels);

            Grid.SetRow(SceneTabs, 0);
            SceneTabs.Orientation = Orientation.Horizontal;
            SceneTabs.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
            TimelinePanel.Children.Add(SceneTabs);
        }

        private void SetUpTimelineLables()
        {

            TimelinePanel.Children.Remove(SceneTabs);

            keySpacer.Height = 30;
            Grid.SetRow(keySpacer, 0);
            OutputKeyArea.Children.Add(keySpacer);

            SceneLabels.Height = 30;
            Grid.SetRow(SceneLabels, 0);
            SceneLabels.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
            KeyArea.Children.Add(SceneLabels);
        }

        private void setDataContexts()
        {
            //sets to null to force window to redraw
            lvCommandList.DataContext = null;
            lvSceneList.DataContext = null;
            lvOutputList.ItemsSource = null;

            lvCommandList.DataContext = commandList;
            lvSceneList.DataContext = sceneList;
            lvOutputList.ItemsSource = outputs.DisplayList;
            DrawTimeline();
        }

        public void UpdateOutputDevices()
        {
            outputs.UpdateList();
            if (previewDevice == null && getMainScreen() != null)
            {
                previewDevice = (VideoDevice)outputs.getOutputByName(getMainScreen().DeviceName);
            }
            setDataContexts();
        }

        //this method scans for connected devices and creates new outputs from them
        //DON'T CALL UNLESS YOU WANT DUPLICATE OUTPUT DEVICES FLOATING AROUND
        public static List<OutputDevice> GetOutputDevices()
        {
            List<OutputDevice> devices = new List<OutputDevice>();
            devices.AddRange(AudioDevice.getAudioDevices(true, true));
            devices.AddRange(VideoDevice.getVideoDevices(true, true));
            devices.AddRange(MidiDeviceManager.Instance.getMidiDevices(true, true));
            return devices;
        }

        public void UpdateSceneTabs(ObservableCollection<Scene> list)
        {
            SceneTabs.Children.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Button sceneButton = new Button();
                sceneButton.Content = " " + list[i].name + " ";
                sceneButton.Click += ShowScene;
                sceneButton.DataContext = list[i];
                sceneButton.Height = 30;
                sceneButton.ToolTip = "Show " + list[i].name + " on Timeline";
                SceneTabs.Children.Add(sceneButton);
            }
            if (Mode.isEditMode)
            {
                Button addSceneButton = new Button();
                addSceneButton.Content = "\u2795"; // unicode for large plus sign
                addSceneButton.Height = addSceneButton.Width = 20;
                addSceneButton.Margin = new Thickness(5);
                addSceneButton.ToolTip = "Add Scene";
                addSceneButton.Click += btnCreateScene_Click;
                SceneTabs.Children.Add(addSceneButton);
            }
        }

        public void UpdateSceneLabels(ObservableCollection<Scene> list)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                SceneLabels.Children.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    TextBlock sceneName = new TextBlock();
                    //sceneName.Text = list[i].name;
                    System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(ColorManager.TEXT_COLOR);
                    sceneName.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                    sceneName.TextTrimming = TextTrimming.CharacterEllipsis;
                    sceneName.Width = scaleToScreen(list[i].duration);
                    sceneName.Text = list[i].name;
                    sceneName.FontWeight = FontWeights.Bold;
                    sceneName.FontSize = 15;
                    sceneName.ToolTip = list[i].name;
                    Canvas.SetLeft(sceneName, scaleToScreen(sceneList.getSceneStartTime(list[i])) + 4);
                    SceneLabels.Children.Add(sceneName);

                    Line line = new Line();
                    line.X1 = line.X2 = scaleToScreen(sceneList.getSceneStartTime(list[i]) + list[i].duration);
                    line.Y1 = 55;
                    line.Y2 = 55 + outputs.DisplayList.Count * 55;
                    line.Stroke = new SolidColorBrush(ColorManager.FOREGROUND);
                    SceneLabels.Children.Add(line);
                }
                if (Mode.isEditMode)
                {
                    Button addSceneButton = new Button();
                    addSceneButton.Content = "\u2795"; // unicode for large plus sign
                    addSceneButton.Height = addSceneButton.Width = 20;
                    addSceneButton.Margin = new Thickness(5);
                    addSceneButton.ToolTip = "Add Scene";
                    addSceneButton.Click += btnCreateScene_Click;
                    Canvas.SetLeft(addSceneButton, scaleToScreen(sceneList.GetDuration()));
                    SceneLabels.Children.Add(addSceneButton);
                }
            });
        }

        private void AddHotKeys()
        {
            try
            {
                RoutedCommand openKey = new RoutedCommand();
                openKey.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control)); //Ctrl+o = opem
                CommandBindings.Add(new CommandBinding(openKey, btnOpenClick));

                RoutedCommand saveKey = new RoutedCommand();
                saveKey.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control)); //Ctrl+s = save
                CommandBindings.Add(new CommandBinding(saveKey, btnSaveProject_Click));

                RoutedCommand exitKey = new RoutedCommand();
                exitKey.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control)); //Ctrl+q = quit
                CommandBindings.Add(new CommandBinding(exitKey, MenuItem_Exit));

                RoutedCommand eventKey = new RoutedCommand();
                eventKey.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));    //Ctrl+e = new Event in current Scene
                CommandBindings.Add(new CommandBinding(eventKey, btnCreateEvent_Click));

                RoutedCommand groupKey = new RoutedCommand();
                groupKey.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));    //Ctrl+g = new group from selected Events
                CommandBindings.Add(new CommandBinding(groupKey, CtrlG_Shortcut));

                RoutedCommand newKey = new RoutedCommand();
                newKey.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));      //Ctrl+n = new Show
                CommandBindings.Add(new CommandBinding(newKey, btnNewShowClick));

                RoutedCommand modeKey = new RoutedCommand();
                modeKey.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));     //Ctrl+m = switch mode (Performance/Edit mode
                CommandBindings.Add(new CommandBinding(modeKey, btnSwitchModeClick));

                RoutedCommand laneKey = new RoutedCommand();
                laneKey.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));     //Ctrl+l = new Ghost output lane
                CommandBindings.Add(new CommandBinding(laneKey, AddGhostItemClick));

                RoutedCommand helpKey = new RoutedCommand();
                helpKey.InputGestures.Add(new KeyGesture(Key.H, ModifierKeys.Control));     //Ctrl+h = open help guide
                CommandBindings.Add(new CommandBinding(helpKey, btnHelp));

                RoutedCommand UndoKey = new RoutedCommand();
                UndoKey.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));     //Ctrl+h = open help guide
                CommandBindings.Add(new CommandBinding(UndoKey, UndoClick));

                RoutedCommand RedoKey = new RoutedCommand();
                RedoKey.InputGestures.Add(new KeyGesture(Key.Y, ModifierKeys.Control));     //Ctrl+h = open help guide
                CommandBindings.Add(new CommandBinding(RedoKey, RedoClick));

                RoutedCommand spacebarKey = new RoutedCommand();
                spacebarKey.InputGestures.Add(new KeyGesture(Key.Space));                   //Spacebar = start playback
                CommandBindings.Add(new CommandBinding(spacebarKey, btnPlay_Click));
            }
            catch
            {
                return;
            }
        }

        private void CtrlG_Shortcut(object sender, ExecutedRoutedEventArgs e)     //From Ctrl+g
        {
            if (sceneList.Scenes[sceneList.currentScenePlaying].getSelected().Count > 0)
            {
                GroupEvents_Shortcut(sceneList.Scenes[sceneList.currentScenePlaying].getSelected());
            }
            else if ((SelectedEvents != null) && (SelectedEvents.Count > 1))
            {
                GroupEvents_Shortcut(SelectedEvents);
            }
        }

        private void GroupEvents_Shortcut(List<Event> selected)
        {
            EventGroup group = new EventGroup();
            group.AddAll(selected);
            actionManager.PerformAction(new CreateGroupAction(group));
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void exitKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
                e.Handled = true;
            }
        }

        public string Status
        {
            get { return btnPlay.Content.ToString(); }
            set
            {
                Dispatcher.Invoke(new System.Action(() =>
                {
                    btnPlay.Content = value;
                    if (value.Equals("PLAY") || value.Equals("Resume"))
                    {
                        btnFade.IsEnabled = false;
                    }
                }));
            }
        }

        public Domain.Actions.Action DoAction(Domain.Actions.Action command)
        {
            Console.WriteLine("DoAction: " + command.Title());
            actionManager.PerformAction(command);
            return command;
        }

        private void DeviceMute(object sender, RoutedEventArgs e)
        {
            OutputDevice device = (sender as Button).DataContext as OutputDevice;
            if ((sceneList.currentScenePlaying >= 0) && (sceneList.currentScenePlaying < sceneList.Scenes.Count))
            {
                actionManager.PerformAction(new MuteLaneAction(device, !device.getDeviceMute(), sender as Button));
            }
        }

        private void DeviceSolo(object sender, RoutedEventArgs e)
        {
            OutputDevice device = (sender as Button).DataContext as OutputDevice;
            if ((sceneList.currentScenePlaying >= 0) && (sceneList.currentScenePlaying < sceneList.Scenes.Count))
            {
                actionManager.PerformAction(new SoloLaneAction(device, !device.getDeviceSolo(), sender as Button));
            }
        }

        //Opens create command window
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateCommandWindow subWindow = new CreateCommandWindow(DoAction);
            subWindow.Owner = this;
            subWindow.ShowDialog();
        }

        private void btnCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            if (sceneList.Scenes.Count > 0)
            {
                Scene scene = sceneList.Scenes[sceneList.currentScenePlaying];
                sceneList.createSceneEvent(scene, commandList);
                changesMade = true;
            }
        }

        private void btnSceneFadeSettings_Click(object sender, RoutedEventArgs e)
        {
            SceneFadeSettingsWindow fadeWindow = new SceneFadeSettingsWindow(sceneList.fadeSettings, commandList);
            fadeWindow.Owner = this;
            fadeWindow.ShowDialog();
        }

        private void btnPreviewSettings_Click(object sender, RoutedEventArgs e) {
            PreviewDevicesWindow deviceWindow = new PreviewDevicesWindow(outputs);
            deviceWindow.Owner = this;
            deviceWindow.ShowDialog();
        }

        //Opens create scene window
        private void btnCreateScene_Click(object sender, RoutedEventArgs e)
        {
            CreateSceneWindow subWindow = new CreateSceneWindow(DoAction);
            subWindow.Owner = this;
            subWindow.ShowDialog();
        }

        //Play selected scene
        private void btnPlaySceneClick(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }

            if (!scene.isPlaying)
            {
                int temp = sceneList.getCurrentScenePlaying();
                sceneList.Scenes[temp].Stop();
                sceneList.SetCurrentScenePlaying(scene);
                sceneList.PlayShow();
                Status = "Pause";
            }
            else
            {
                scene.Pause();
                Status = "Resume";
            }
        }

        public void setCurrentScenePlaying(int ScenePlaying)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                        () => lvSceneList.SelectedItem = lvSceneList.Items[ScenePlaying]
                , DispatcherPriority.Normal);
                Dispatcher.Invoke(
                       () => lvSceneList.ScrollIntoView(lvSceneList.SelectedItem)
               , DispatcherPriority.Normal);

            }
            else
            {
                lvSceneList.SelectedItem = lvSceneList.Items[ScenePlaying];
                lvSceneList.ScrollIntoView(lvSceneList.SelectedItem);
            }
        }

        //Create an event
        private void btnCreateSceneEvent(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            sceneList.createSceneEvent(scene, commandList);
        }

        private void btnDuplicateScene(object sender, RoutedEventArgs e)
        {
            Scene original;
            MenuItem button = sender as MenuItem;
            original = button.DataContext as Scene;
            actionManager.PerformAction(new DuplicateSceneAction(original));
        }

        //Deletes a scene
        private void btnDeleteScene(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            if (MessageBox.Show("Are you sure you want to delete scene " + scene.name + "?", "Delete Scene", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                actionManager.PerformAction(new DeleteSceneAction(scene, sceneList));
            }
        }

        //Opens edit event window (create event)
        private void btnEditEventClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Event ev = item.DataContext as Event;
            Scene scene = ev.getScene();
            EditEvent(scene, ev);
        }

        public void EditEvent(Scene scene, Event ev)
        {
            sceneList.editEvent(scene, ev, commandList);
        }

        //Delete an event
        public void btnDeleteEventClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Event ev = item.DataContext as Event;
            if (MessageBox.Show("Are you sure you want to delete event " + ev.name + "?", "Delete Event", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Scene scene = ev.getScene();
                actionManager.PerformAction(new DeleteEventAction(ev, scene));
            }
        }

        private void EventContextMenuCreation(object sender, ContextMenuEventArgs e)
        {
            ContextMenu menu = (sender as Grid).ContextMenu;
            for (int i = 0; i < menu.Items.Count; i++)
            {
                MenuItem oldItem = menu.Items[i] as MenuItem;
                if (oldItem != null && oldItem.Header.Equals("Ungroup Event"))
                {
                    menu.Items.RemoveAt(i);
                }
            }
            MenuItem item = menu.Items[1] as MenuItem;
            Event ev = (sender as Grid).DataContext as Event;
            item.IsEnabled = false;
            item.Click -= DissolveGroup_Event;
            item.Click -= GroupEvents_Event;

            if (ev.group != null)
            {
                Console.Write("Group: ");
                for (int j = 0; j < ev.group.GetElements().Count; j++)
                {
                    Console.WriteLine(ev.group.GetElements()[j].name);
                }
            }

            if (ev.IsSelected() && SelectedEvents.Count > 1 && (ev.group == null || (ev.group != null && !ev.group.Matches(SelectedEvents))))
            {
                item.Header = "Group Events";
                item.Click += GroupEvents_Event;
                item.IsEnabled = true;
            }
            else if (SelectedEvents != null && ev.group != null && ev.group.Matches(SelectedEvents))
            {
                item.Header = "Dissolve Group";
                item.Click += DissolveGroup_Event;
                if (!ev.group.IsLocked())
                {
                    item.IsEnabled = true;
                }
            }

            if (ev.group != null)
            {
                MenuItem removeItem = new MenuItem();
                removeItem.Header = "Ungroup Event";
                removeItem.IsEnabled = !ev.group.IsLocked();
                removeItem.Click += UngroupEvent_Event;
                menu.Items.Insert(2, removeItem);
            }
            item.DataContext = ev;

            if (!Mode.isEditMode)
            {
                item.IsEnabled = false;
            }
        }

        private void SceneContextMenuCreation(object sender, ContextMenuEventArgs e)
        {
            ContextMenu menu = (sender as Grid).ContextMenu;
            menu.Items.Clear();
            Console.WriteLine("Scene: " + (sender as Grid).DataContext);
            Scene scene = (sender as Grid).DataContext as Scene;

            //Control
            MenuItem playMenuItem = new MenuItem();
            playMenuItem.Header = "Play Scene";
            playMenuItem.DataContext = scene;
            playMenuItem.Click += btnPlaySceneClick;
            menu.Items.Add(playMenuItem);

            MenuItem duplicateScene = new MenuItem();
            duplicateScene.Header = "Duplicate Scene";
            duplicateScene.DataContext = scene;
            duplicateScene.Click += btnDuplicateScene;
            menu.Items.Add(duplicateScene);

            MenuItem AddEventMenuItem = new MenuItem();
            AddEventMenuItem.Header = "Add Event to Scene";
            AddEventMenuItem.IsEnabled = Mode.isEditMode;
            AddEventMenuItem.DataContext = scene;
            AddEventMenuItem.Click += btnCreateSceneEvent;
            menu.Items.Add(AddEventMenuItem);

            menu.Items.Add(new Separator());
            //Skip Scene
            if (!Mode.isEditMode)
            {
                MenuItem SkipMenuItem = new MenuItem();
                SkipMenuItem.Header = scene.ShouldSkip ? "Include Scene" : "Skip Scene";
                SkipMenuItem.DataContext = scene;
                SkipMenuItem.Click += SkipSceneEvent;
                menu.Items.Add(SkipMenuItem);
            }

            //Movement
            MenuItem MoveMenu = new MenuItem();
            MoveMenu.Header = "Move Scene";
            menu.Items.Add(MoveMenu);

            MenuItem MoveTopMenuItem = new MenuItem();
            MoveTopMenuItem.Header = "Move to Beginning";
            MoveTopMenuItem.DataContext = scene;
            MoveTopMenuItem.IsEnabled = sceneList.Scenes.IndexOf(scene) > 0;
            MoveTopMenuItem.Click += btnMoveSceneToTop;
            MoveMenu.Items.Add(MoveTopMenuItem);

            MenuItem MoveUpMenuItem = new MenuItem();
            MoveUpMenuItem.Header = "Move Up";
            MoveUpMenuItem.DataContext = scene;
            MoveUpMenuItem.IsEnabled = sceneList.Scenes.IndexOf(scene) > 0;
            MoveUpMenuItem.Click += btnMoveSceneUp;
            MoveMenu.Items.Add(MoveUpMenuItem);

            MenuItem MoveDownMenuItem = new MenuItem();
            MoveDownMenuItem.Header = "Move Down";
            MoveDownMenuItem.DataContext = scene;
            MoveDownMenuItem.IsEnabled = sceneList.Scenes.IndexOf(scene) < sceneList.Scenes.Count - 1;
            MoveDownMenuItem.Click += btnMoveSceneDown;
            MoveMenu.Items.Add(MoveDownMenuItem);

            MenuItem MoveBottomMenuItem = new MenuItem();
            MoveBottomMenuItem.Header = "Move to End";
            MoveBottomMenuItem.DataContext = scene;
            MoveBottomMenuItem.IsEnabled = sceneList.Scenes.IndexOf(scene) < sceneList.Scenes.Count - 1;
            MoveBottomMenuItem.Click += btnMoveSceneToBottom;
            MoveMenu.Items.Add(MoveBottomMenuItem);

            menu.Items.Add(new Separator());

            //Delete
            MenuItem DeleteMenuItem = new MenuItem();
            DeleteMenuItem.Header = "Delete Scene";
            DeleteMenuItem.DataContext = scene;
            DeleteMenuItem.Click += btnDeleteScene;
            menu.Items.Add(DeleteMenuItem);
        }

        private void SkipSceneEvent(object sender, RoutedEventArgs e)
        {
            MenuItem button = sender as MenuItem;
            Scene scene = button.DataContext as Scene;
            scene.ShouldSkip = !scene.ShouldSkip;
            if (sceneList.Scenes[sceneList.currentScenePlaying].Equals(scene) && scene.ShouldSkip)
            {
                Console.WriteLine("Skip Current scene");
                if (sceneList.currentScenePlaying + 1 < sceneList.Scenes.Count)
                {
                    sceneList.SetCurrentScenePlaying(sceneList.Scenes[sceneList.currentScenePlaying + 1]);
                }
                else
                {
                    sceneList.StopShow();
                }
            }
        }

        private void UngroupEvent_Event(object sender, RoutedEventArgs e)
        {
            Event ev = (sender as MenuItem).DataContext as Event;
            ev.UngroupEvent_Event(sender, e);
        }

        private void UngroupEvents_Event(object sender, RoutedEventArgs e)
        {
            actionManager.PerformAction(new DissolveAllGroupsAction(sceneList));
        }

        private void GroupEvents_Event(object sender, RoutedEventArgs e)
        {
            Event ev = (sender as MenuItem).DataContext as Event;
            (sender as MenuItem).DataContext = SelectedEvents;
            ev.GroupEvents_Event(sender, e);
        }

        private void DissolveGroup_Event(object sender, RoutedEventArgs e)
        {
            Event ev = (sender as MenuItem).DataContext as Event;
            (sender as MenuItem).DataContext = SelectedEvents;
            ev.DissolveGroup_Event(sender, e);
        }

        private void EventSelected(object sender, RoutedEventArgs e)
        {
            SelectedEvents = new List<Event>();
            SelectedEvents = ((sender as ListView).SelectedItems).Cast<Event>().ToList(); ;
            ObservableCollection<Event> events = sceneList.Scenes[sceneList.currentScenePlaying].Events;
            for (int i = 0; i < events.Count; i++)
            {
                events[i].Select(SelectedEvents.Contains(events[i]));
            }

            for (int i = 0; i < SelectedEvents.Count; i++)
            {
                if (SelectedEvents[i].group != null)
                {
                    bool select = SelectedEvents[i].IsSelected();
                    for (int j = 0; j < SelectedEvents[i].group.GetElements().Count; j++)
                    {
                        SelectedEvents[i].group.GetElements()[j].Select(select);
                        (sender as ListView).SelectedItems.Add(SelectedEvents[i].group.GetElements()[j]);
                    }
                }
            }
            SelectedEvents = ((sender as ListView).SelectedItems).Cast<Event>().ToList();
        }

        internal void ShowOutputDevice(OutputDevice outputDevice)
        {
            outputs.HideOutput(false, outputDevice);
            setDataContexts();
        }

        private void btnFade_Click(object sender, RoutedEventArgs e)
        {
            sceneList.FadeToNextScene();
        }

        //Master play
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sceneList.isPlaying)
            {
                sceneList.PauseShow();
                btnFade.IsEnabled = false;
                foreach (Event ev in sceneList.Scenes[sceneList.currentScenePlaying].Events)
                {
                    ev.loaded = false;
                }
            }
            else
            {
                sceneList.PlayShow();
                btnFade.IsEnabled = true;
            }
            btnPlay.Content = (sceneList.isPlaying) ? "Pause" : "Resume";
        }

        // All keys must use the same KeyDown Event Argument 
        public void keyPlay(object sender, KeyEventArgs e)
        {
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) & e.Key == Key.M)
            {
                btnSwitchModeClick(sender, (RoutedEventArgs)e);
            }
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) & e.Key == Key.S)
            {
                btnSaveProject_Click(sender, new CancelEventArgs());
            }
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) & e.Key == Key.O)
            {
                btnOpenClick(sender, (RoutedEventArgs)e);
            }
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) & e.Key == Key.N)
            {
                btnNewShowClick(sender, (RoutedEventArgs)e);
            }
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) & e.Key == Key.O)
            {
                btnOpenClick(sender, (RoutedEventArgs)e);
            }
            if (e.Key == Key.Delete)
            {
                btnDeletEvents_Click(sender, (RoutedEventArgs)e);
            }
        }


        private void btnDeletEvents_Click(object send, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete event the selected Events?", "Delete Events", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (sceneList != null)
                {
                    foreach (Event ev in sceneList.Scenes[sceneList.currentScenePlaying].getSelected())
                    {
                        ev.deleteSelected();
                    }
                }
            }
            changesMade = true;
        }
        //Master stop
        private void btnStop_Click(object s, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Stop Show", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                sceneList.StopShow(false);
                foreach (Event ev in sceneList.Scenes[sceneList.currentScenePlaying].Events)
                {
                    ev.loaded = false;
                }
                btnPlay.Content = "PLAY";
                btnFade.IsEnabled = false;
            }
        }

        //Reorder scenes
        private void btnMoveSceneToTop(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            actionManager.PerformAction(new RearangeSceneAction(scene, -sceneList.Scenes.IndexOf(scene), sceneList));
        }

        private void btnMoveSceneUp(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            actionManager.PerformAction(new RearangeSceneAction(scene, -1, sceneList));
        }

        private void btnMoveSceneDown(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            actionManager.PerformAction(new RearangeSceneAction(scene, 1, sceneList));
        }

        private void btnMoveSceneToBottom(object sender, RoutedEventArgs e)
        {
            Scene scene;
            if (sender is Button)
            {
                Button button = sender as Button;
                scene = button.DataContext as Scene;
            }
            else
            {
                MenuItem button = sender as MenuItem;
                scene = button.DataContext as Scene;
            }
            actionManager.PerformAction(new RearangeSceneAction(scene, sceneList.Scenes.Count - sceneList.Scenes.IndexOf(scene) - 1, sceneList));
        }

        //Switch between edit/play mode - uses Mode.cs
        private void btnSwitchModeClick(object sender, RoutedEventArgs e)
        {
            if (sceneList.Scenes.Count > 0)
            {
                sceneList.PauseShow();
                btnPlay.Content = "Resume";
            }
            Mode.setMode(!Mode.isEditMode);
            foreach (OutputDevice Device in outputs.getOutputs(Types.VIDEO, false))
            {
                if (Device is VideoDevice)
                {
                    int eventCount = 0;
                    for (int i = 0; i < sceneList.Scenes.Count; i++)
                    {
                        eventCount += sceneList.Scenes[i].getEventsByOutputDevice(Device).Count;
                    }
                    if (eventCount != 0)
                    {
                        (Device as VideoDevice).ReopenVideoPanel();
                    }
                }
            }
            UpdateSceneTabs(sceneList.Scenes);

            if (!Mode.isEditMode)
            {
                SwitchModeButton.Header = "Switch to Edit _Mode";

                if (sceneList.Scenes.Count > 0)
                {
                    btnPlay.Content = "PLAY";
                    foreach (Scene scene in sceneList.Scenes)
                    {
                        scene.elapsedMilliseconds = 0;
                    }
                }
            }
            else if (Mode.isEditMode)
            {
                SwitchModeButton.Header = "Switch to Performance _Mode";
            }
            MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(false, sceneList);
            mapDevicesWindow.Show();
            mapDevicesWindow.mapDevices();
            foreach (Scene scene in sceneList.Scenes)
            {
                scene.ShouldSkip = false;
            }
            ((SolidColorBrush)Resources["Background"]).Color = ColorManager.BACKGROUND;
            ((SolidColorBrush)Resources["Forground"]).Color = ColorManager.FOREGROUND;
            ((SolidColorBrush)Resources["Accent"]).Color = ColorManager.ACCENT;
            setDataContexts();

        }

        public void keySwitchmode(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                if (sceneList.Scenes.Count > 0)
                {
                    sceneList.PauseShow();
                    btnPlay.Content = "Resume";
                }
                Mode.setMode(!Mode.isEditMode);
                foreach (OutputDevice Device in outputs.getOutputs(Types.VIDEO, false))
                {
                    if (Device is VideoDevice)
                    {
                        (Device as VideoDevice).ReopenVideoPanel();
                    }
                }
                UpdateSceneTabs(sceneList.Scenes);

                if (!Mode.isEditMode)
                {
                    SwitchModeButton.Header = "Switch to Edit _Mode";

                    if (sceneList.Scenes.Count > 0)
                    {
                        sceneList.StopShow();
                        btnPlay.Content = "PLAY";
                        foreach (Scene scene in sceneList.Scenes)
                        {
                            scene.elapsedMilliseconds = 0;
                        }
                    }
                }
                else if (Mode.isEditMode)
                {
                    SwitchModeButton.Header = "Switch to Performance _Mode";
                }
                MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(false, sceneList);
                mapDevicesWindow.Show();
                mapDevicesWindow.mapDevices();
                foreach (Scene scene in sceneList.Scenes)
                {
                    scene.ShouldSkip = false;
                }
            ((SolidColorBrush)Resources["Background"]).Color = ColorManager.BACKGROUND;
                ((SolidColorBrush)Resources["Forground"]).Color = ColorManager.FOREGROUND;
                ((SolidColorBrush)Resources["Accent"]).Color = ColorManager.ACCENT;
                setDataContexts();
            }
        }

        private void SetColor(string brushName, Color color)
        {
            SolidColorBrush brush = (SolidColorBrush)Resources[brushName];
            if (brush.IsFrozen)
            {
                Console.WriteLine("Frozen brush " + brush);
            }
            else
            {
                ((SolidColorBrush)Resources[brushName]).Color = color;
            }
        }

        //Save current project using JSON 
        private void btnSaveProject_Click(object sender, RoutedEventArgs e)
        {
            btnSaveProject_Click(sender, new CancelEventArgs());
        }

        private void ctrlSave(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                btnSaveProject_Click(sender, new CancelEventArgs());
            }
        }

        private void btnSaveProject_Click(object sender, CancelEventArgs e)
        {
            String fileName = (this.projectfileName != null) ? this.projectfileName : System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\data\\project" + extention;
            if (File.Exists(fileName))
            {
                Stream stream = File.Open(fileName, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();

                bformatter.Serialize(stream, new Project(commandList, sceneList, outputs));
                stream.Close();
                actionManager.Save();
            }
            else
            {
                btnSaveAsClick(sender, e);
            }
        }

        //formats the button that switches waveform type
        private void btnSwitchWaveformType(object sender, RoutedEventArgs e)
        {
            if (MusicEvent.waveformType)
            {
                waveformSwitcher.Header = "Switch _Waveform to Rectangle";
                MusicEvent.waveformType = false;
            }
            else
            {
                waveformSwitcher.Header = "Switch _Waveform to Wave (Slower)";
                MusicEvent.waveformType = true;
            }
            DrawTimeline();
        }

        private void btnSwitchTimelineView(object sender, RoutedEventArgs e)
        {
            if (ShowView)
            {
                TimelineViewButton.Header = "Switch to Show _View";
                if (sceneList.Scenes.Count > 0)
                    timelineInfo.zoomToShow(sceneList.Scenes[sceneList.currentScenePlaying], Timeline);
                ShowView = false;
                UpdateSceneTabs(sceneList.Scenes);
                SetUpTimelineTabs();
            }
            else
            {
                TimelineViewButton.Header = "Switch to Scene _View";
                timelineInfo.zoomToShow(sceneList, Timeline);
                if (sceneList.Scenes.Count > 0)
                {
                    Scene currentScene = sceneList.Scenes[sceneList.currentScenePlaying];
                    if (currentScene.elapsedMilliseconds > currentScene.duration)
                    {
                        currentScene.elapsedMilliseconds = 0;
                    }
                }
                ShowView = true;
                SetUpTimelineLables();
            }
            DrawTimeline();
        }

        public SceneList mapDevicesCallback(SceneList sceneList)
        {
            outputs.UpdateList();
            setDataContexts();
            return sceneList;
        }

        private void OutputContextMenuCreation(object sender, ContextMenuEventArgs e)
        {
            if ((sender as ListView).ContextMenu.Items.Count > 1)
            {
                int removeNum = (sender as ListView).ContextMenu.Items.Count - 1;
                for (int i = 0; i < removeNum; i++)
                {
                    (sender as ListView).ContextMenu.Items.RemoveAt(0);
                }
            }
            MenuItem addGhostItem = new MenuItem();
            addGhostItem.Click += AddGhostItemClick;
            addGhostItem.Header = "Add Ghost Output";
            (sender as ListView).ContextMenu.Items.Insert(0, new Separator());
            (sender as ListView).ContextMenu.Items.Insert(0, addGhostItem);
            if (e.CursorTop < 55)
            {
                if (((sender as ListView).SelectedItem as OutputDevice) == null)
                    return;
                if (((sender as ListView).SelectedItem as OutputDevice).type.Equals(Types.GHOST))
                {
                    MenuItem removeGhostItem = new MenuItem();
                    removeGhostItem.Click += RemoveGhostItemClick;
                    removeGhostItem.DataContext = (sender as ListView).SelectedItem;
                    removeGhostItem.Header = "Remove Ghost Output";
                    (sender as ListView).ContextMenu.Items.Insert(0, removeGhostItem);
                }
                else
                {
                    Console.WriteLine((sender as ListView).SelectedItem);
                    MenuItem hideItem = new MenuItem();
                    if (sceneList.currentScenePlaying >= 0 && sceneList.currentScenePlaying < sceneList.Scenes.Count)
                    {
                        int eventCount = 0;
                        for (int i = 0; i < sceneList.Scenes.Count; i++)
                        {
                            eventCount += sceneList.Scenes[i].getEventsByOutputDevice((sender as ListView).SelectedItem as OutputDevice).Count;
                        }
                        hideItem.IsEnabled = eventCount == 0;
                    }
                    else
                    {
                        hideItem.IsEnabled = sceneList.Scenes.Count == 0;
                    }
                    hideItem.DataContext = (sender as ListView).SelectedItem;
                    hideItem.Click += HideOutput;
                    hideItem.Header = "Hide Output";
                    (sender as ListView).ContextMenu.Items.Insert(0, new Separator());
                    (sender as ListView).ContextMenu.Items.Insert(0, hideItem);
                }
                if (LaneOutput.getLaneDevices().Contains(((sender as ListView).SelectedItem as OutputDevice)))
                {
                    MenuItem removeLaneItem = new MenuItem();
                    removeLaneItem.Click += removeLaneItemClick;
                    removeLaneItem.DataContext = (sender as ListView).SelectedItem;
                    removeLaneItem.Header = "Remove Lane";
                    (sender as ListView).ContextMenu.Items.Insert(0, new Separator());
                    (sender as ListView).ContextMenu.Items.Insert(0, removeLaneItem);
                }
                else if (!((sender as ListView).SelectedItem as OutputDevice).type.Equals(Types.GHOST))
                {
                    MenuItem addLaneItem = new MenuItem();
                    addLaneItem.Click += AddLaneItemClick;
                    addLaneItem.DataContext = (sender as ListView).SelectedItem;
                    addLaneItem.Header = "Duplicate Lane";
                    (sender as ListView).ContextMenu.Items.Insert(0, new Separator());
                    (sender as ListView).ContextMenu.Items.Insert(0, addLaneItem);
                }
            }
        }

        private void removeLaneItemClick(object sender, RoutedEventArgs e)
        {
            int eventCount = 0;
            for (int i = 0; i < sceneList.Scenes.Count; i++)
            {
                eventCount += sceneList.Scenes[i].getEventsByOutputDevice((sender as MenuItem).DataContext as OutputDevice).Count;
            }
            if (eventCount > 0)
            {
                MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(false, sceneList);
                mapDevicesWindow.allOutputs = false;
                mapDevicesWindow.ghostPortBtn.IsEnabled = false;
                mapDevicesWindow.mapDevice(((sender as MenuItem).DataContext as OutputDevice));
            }
            else
            {
                actionManager.PerformAction(new RemoveDuplicateOutputAction(((sender as MenuItem).DataContext as OutputDevice)));
            }
        }

        private void AddLaneItemClick(object sender, RoutedEventArgs e)
        {
            if (sender != null && (sender as MenuItem).DataContext != null)
            {

                var test = ((sender as MenuItem).DataContext as OutputDevice);
                actionManager.PerformAction(new DuplicateOutputActioncs(test));
            }
        }

        private void HideOutput(object sender, RoutedEventArgs e)
        {
            if (sender != null && (sender as MenuItem).DataContext != null)
            {
                outputs.HideOutput(true, ((sender as MenuItem).DataContext as OutputDevice));
                setDataContexts();
            }
        }

        private void RemoveGhostItemClick(object sender, RoutedEventArgs e)
        {
            int eventCount = 0;
            OutputDevice device = (sender as MenuItem).DataContext as OutputDevice;
            for (int i = 0; i < sceneList.Scenes.Count; i++)
            {
                eventCount += sceneList.Scenes[i].getEventsByOutputDevice(device).Count;
            }
            if (eventCount > 0)
            {
                MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(false, sceneList);
                mapDevicesWindow.allOutputs = false;
                mapDevicesWindow.ghostPortBtn.IsEnabled = false;
                mapDevicesWindow.mapDevice(device);
            }
            else
            {
                actionManager.PerformAction(new RemoveGhostDeviceAction(device));
                outputs.UpdateList();
                setDataContexts();
            }
        }

        private void AddGhostItemClick(object sender, RoutedEventArgs e)
        {
            CreateGhostOutputWindow createGhostOutputWindow = new CreateGhostOutputWindow(DoAction);
            createGhostOutputWindow.Owner = this;
            createGhostOutputWindow.ShowDialog();
        }

        public OutputDevice AddGhostCallback(OutputDevice device)
        {
            outputs.UpdateList();
            setDataContexts();
            return device;
        }

        private void BtnOutputSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Open Output Settings");
            ManageOutputsWindow outputsWindow = new ManageOutputsWindow(outputs, sceneList);
            outputsWindow.Owner = this;
            outputsWindow.ShowDialog();
        }

        private void BtnDeviceLanes(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Open Output Settings");
            ManageOutputsWindow outputsWindow = new ManageOutputsWindow(outputs, sceneList);
            outputsWindow.Owner = this;
            outputsWindow.ShowDialog();
        }

        internal Event GetEventDragging()
        {
            return DragEvent;
        }

        //Open an existing project - extension .show(Legacy) or .ascs
        private void btnOpenClick(object sender, RoutedEventArgs e)
        {
            if (sceneList.isPlaying)
            {
                sceneList.PauseShow();
            }
            doYouWantToSaveDialog(sender, "Do you want to Save before opening a new show?", new CancelEventArgs());

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Project Files (*" + extention + ") | *.show; *" + extention;
            open.InitialDirectory = System.IO.Directory.GetCurrentDirectory();

            if (open.ShowDialog() == true)
            {
                String filePath = open.FileName;
                if (filePath.EndsWith(".show"))
                {
                    OpenLegacyShow(filePath);
                    UpdateSceneTabs(sceneList.Scenes);
                    return;
                }
                sceneList = null;

                //Open the file written above and read values from it.
                GhostOutput.clearDevices();
                LaneOutput.clearDevices();
                Stream stream = File.Open(filePath, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();
                Project project = (Project)bformatter.Deserialize(stream);

                stream.Close();

                this.projectfileName = filePath;
                this.commandList = project.commandListObject;
                this.sceneList = project.sceneListObject;
                BuildOutputListFromSave(project.outputManager);

                CommandListFilterComboBox.SelectedIndex = 0;
                MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(sceneList);
                mapDevicesWindow.Show();
                mapDevicesWindow.mapDevices();
                changesMade = false;


                UpdateSceneTabs(sceneList.Scenes);
                if (sceneList.Scenes.Count > 0 && sceneList.currentScenePlaying >= 0 && sceneList.currentScenePlaying < sceneList.Scenes.Count)
                {
                    if (ShowView)
                    {
                        timelineInfo.zoomToShow(sceneList, Timeline);
                    }
                    else
                    {
                        timelineInfo.zoomToShow(sceneList.Scenes[sceneList.currentScenePlaying], Timeline);
                    }
                }
                DrawTimeline();
                return;
            }
            previewDevice = (VideoDevice)outputs.getOutputByName(getMainScreen().DeviceName);
            UpdateOutputDevices();
        }

        private void BuildOutputListFromSave(OutputManager outputManager)
        {
            previewDevice = (VideoDevice)outputs.getOutputByName(getMainScreen().DeviceName);
            UpdateOutputDevices();

            foreach (OutputDevice device in outputManager.outputList)
            {
                OutputDevice newDevice = outputs.getOutputByName(device.deviceName);
                if (newDevice != null)
                {
                    Console.Write("Sync Device: " + device);
                    newDevice.setDeviceMute(device.getDeviceMute());
                    sceneList.Scenes[sceneList.currentScenePlaying].GreyOutLane(outputs.DisplayList.IndexOf(newDevice), device.getDeviceMute());
                    Console.Write(" Mute: " + device.getDeviceMute());
                    newDevice.setDeviceMuteBySolo(device.getDeviceMuteBySolo());
                    Console.Write(" Mute by Solo: " + device.getDeviceMuteBySolo());
                    newDevice.setDeviceSolo(device.getDeviceSolo());
                    Console.WriteLine(" Solo: " + device.getDeviceSolo());
                    newDevice.Hidden = false;
                }
            }
            setDataContexts();
        }


        //load an existing project from filepath - extension .show(Legacy)
        private void OpenLegacyShow(string filePath)
        {
            this.projectfileName = filePath.Replace(".show", extention);
            Project proj = new Project(System.IO.File.ReadAllText(filePath));
            this.commandList = proj.ConvertCommandList();
            this.sceneList = proj.ConvertSceneList();
            setDataContexts();

            CommandListFilterComboBox.SelectedIndex = 0;
            GhostOutput.clearDevices();
            //LaneOutput.clearDevices();

            MapDevicesWindow mapDevicesWindow = new MapDevicesWindow(sceneList);
            mapDevicesWindow.Show();
            mapDevicesWindow.mapDevices();
            changesMade = false;

            MessageBox.Show(
                        "Save format has changed please save this show to update.",
                        "Show Automation",
                        MessageBoxButton.OK,
                        MessageBoxImage.None);
            return;
        }

        //Saves show as a new production - extension .show
        private void btnSaveAsClick(object sender, RoutedEventArgs e)
        {
            btnSaveAsClick(sender, new CancelEventArgs());
        }

        //formats what happens with the save as buttion
        private void btnSaveAsClick(object sender, CancelEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Project Files (*" + extention + ") | *" + extention;
            if (saveFileDialog.ShowDialog() == true)
            {
                this.projectfileName = saveFileDialog.FileName;
                Stream stream = File.Open(saveFileDialog.FileName, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();

                bformatter.Serialize(stream, new Project(commandList, sceneList, outputs));
                stream.Close();
                actionManager.Save();
            }
            else
            {
                e.Cancel = true;
            }
        }

        //creates a new show by closing the application and then restarting it. 
        private void btnNewShowClick(object sender, RoutedEventArgs e)
        {
            CancelEventArgs cancelArgs = new CancelEventArgs();
            doYouWantToSaveDialog(sender, "Do you want to Save before opening a blank show?", cancelArgs);
            if (!cancelArgs.Cancel)
            {
                foreach (MidiDevice device in MidiDeviceManager.Instance.getMidiDevices(false, false))
                {
                    Console.WriteLine("Dispose of " + device.deviceName);
                    if (device.device != null)
                    {
                        device.device.Dispose();
                    }
                }
                Application.Current.Shutdown();
                System.Windows.Forms.Application.Restart();
            }
        }

        //change scene shown on timeline
        private void ShowScene(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Scene scene = button.DataContext as Scene;

            if (button.Content.Equals(" " + scene.name + " "))
            {
                int index = sceneList.Scenes.IndexOf(scene);
                sceneList.sceneInViewIndex = index;
                if (Mode.isEditMode)
                {
                    if (sceneList.Scenes[sceneList.currentScenePlaying].isPlaying)
                    {
                        sceneList.PauseShow();
                        MainWindow.main.Status = "Resume";
                    }
                    sceneList.currentScenePlaying = index;
                }
                DrawTimeline();
            }
        }

        public void lvEventList_Click(object sender, MouseEventArgs e)
        {
            if (Mode.isEditMode && !ShowView)
            {
                Event @event = (sender as Grid).DataContext as Event;
                if (@event != null)
                {
                    var song = @event.command.media as Song;
                    var video = @event.command.media as Video;
                    var midiType = @event.type;

                    if ((song != null && File.Exists(song.songPath)) || (video != null && File.Exists(video.videoPath)) || (midiType == Types.MIDI))
                    {
                        if (@event.type.Equals(Types.AUDIO))
                        {
                            DragEvent = new MusicEvent((MusicCommand)@event.command, 0, @event.getScene(), (AudioDevice)@event.OutputDevice);
                        }
                        else if (@event.type.Equals(Types.VIDEO))
                        {
                            DragEvent = new VideoEvent((VideoCommand)@event.command, 0, @event.getScene(), (VideoDevice)@event.OutputDevice);
                        }
                        else if (@event.type.Equals(Types.MIDI))
                        {
                            DragEvent = new MidiEvent((MidiCommand)@event.command, 0, @event.getScene(), (MidiDevice)@event.OutputDevice);
                        }
                        dragStartLocation = e.GetPosition(sender as Grid);
                        StartingOutput = null;
                    }
                }
            }
        }

        //Filter box for command list
        private void CommandListFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var commandListFilter = new CommandListFilter(commandList);
            string type = (string)((ComboBoxItem)CommandListFilterComboBox.SelectedValue).Content;

            if (type.Equals("All"))
            {
                lvCommandList.DataContext = commandList;
                return;
            }
            commandListFilter.setFilter(type);
            lvCommandList.DataContext = commandListFilter;
            var commands = new List<AbstractCommand>(commandListFilter.getCommands());

        }

        public void lvCommandList_Click(object sender, MouseEventArgs e)
        {
            if (Mode.isEditMode)
            {
                AbstractCommand command = (sender as Grid).DataContext as AbstractCommand;

                if (command != null)
                {
                    var song = command.media as Song;
                    var video = command.media as Video;
                    var midiType = command.type;
                    if ((song != null && (File.Exists((String)song.songPath))) || (video != null && (File.Exists((String)video.videoPath))) || (midiType == Types.MIDI))
                    {
                        DragListItem = (ListViewItem)lvCommandList.ItemContainerGenerator.ContainerFromItem((sender as Grid).DataContext);
                        dragStartLocation = e.GetPosition(sender as Grid);
                    }
                }
            }
        }

        public void btnEditCommandClick(object sender, RoutedEventArgs e)
        {
            var editCommandWindow = new CreateCommandWindow((sender as MenuItem).DataContext as AbstractCommand, DoAction);
            editCommandWindow.Owner = this;
            editCommandWindow.ShowDialog();
        }

        public void btnRemoveCommandClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete command " + ((sender as MenuItem).DataContext as AbstractCommand).name + "?", "Remove Command", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                actionManager.PerformAction(new DeleteCommandAction((sender as MenuItem).DataContext as AbstractCommand, commandList));
            }
        }

        //Drag a Scene while in Show View
        private void SceneOnTimelineClick(object sender, MouseButtonEventArgs e)
        {
            if (ShowView)
            {
                Scene scene = (sender as Grid).DataContext as Scene;
                if (scene != null && !scene.isPlaying)
                {
                    DragScene = (sender as Grid).DataContext as Scene;
                    dragStartLocation = e.GetPosition(Content);
                }
            }
        }

        private void ListItem_Release(object sender, MouseEventArgs e)
        {
            if (draggingCommand || draggingEvent || draggingScene)
            {
                //if Drag is released over the scene list
                if (lvSceneList.IsMouseOver && draggingCommand)
                {
                    double mouseY = e.GetPosition(lvSceneList).Y;
                    double top = 0;
                    for (int i = 0; i < lvSceneList.Items.Count; i++)
                    {
                        Console.WriteLine("Drop on Scene " + i);
                        ListViewItem tempItem = (ListViewItem)lvSceneList.ItemContainerGenerator.ContainerFromItem(lvSceneList.Items[i]);
                        Console.WriteLine("Drop on Scene " + lvSceneList.Items[i]);
                        Console.WriteLine("Drop on Scene " + lvSceneList.ItemContainerGenerator.ContainerFromItem(lvSceneList.Items[i]));
                        double bottom = tempItem.ActualHeight + top;
                        if (mouseY >= top && mouseY < bottom)
                        {
                            Scene over = (lvSceneList.Items[i] as Scene);
                            String type = (DragListItem.Content as AbstractCommand).type;
                            Event newE = null;
                            if (type.Equals(Types.AUDIO))
                            {
                                newE = new MusicEvent(DragListItem.Content as MusicCommand, 0, over, (AudioDevice)GetDefaultDevice(Types.AUDIO));
                            }
                            else if (type.Equals(Types.VIDEO))
                            {
                                newE = new VideoEvent(DragListItem.Content as VideoCommand, 0, over, (VideoDevice)GetDefaultDevice(Types.VIDEO));
                            }
                            else if (type.Equals(Types.MIDI))
                            {
                                newE = new MidiEvent(DragListItem.Content as MidiCommand, 0, over, (MidiDevice)GetDefaultDevice(Types.MIDI));
                            }
                            if (newE != null)
                            {
                                CreateEventWindow window = new CreateEventWindow(over, commandList, newE, DoAction);
                                window.setEditing(false);
                                window.Owner = MainWindow.main;
                                window.ShowDialog();
                                break;
                            }
                        }
                        top = bottom;
                    }

                }
                //if Drag is released over the timeline
                else if (LaneGrid.IsMouseOver && !ShowView)
                {
                    if (draggingCommand || draggingEvent)
                    {
                        double mouseX = e.GetPosition(LaneGrid).X;
                        double mouseY = e.GetPosition(LaneGrid).Y;
                        int row = 0;
                        mouseY -= 2.5; // offset to account for seperator at the top
                        while (mouseY >= 55)
                        {
                            row++;
                            mouseY -= 55;
                        }
                        Event @event = null;
                        AbstractCommand commandDragging;
                        if (draggingEvent)
                        {
                            commandDragging = DragEvent.command;
                        }
                        else
                        {
                            commandDragging = DragListItem.Content as AbstractCommand;
                        }
                        String type = commandDragging.type;
                        Scene droppedOn = sceneList.Scenes[sceneList.currentScenePlaying];
                        if (ShowView)
                        {
                            droppedOn = sceneList.GetSceneAtTime(scaleToTime(mouseX));
                        }
                        if (draggingEvent)
                        {
                            @event = DragEvent;
                        }
                        else if (type.Equals(Types.AUDIO))
                        {
                            @event = new MusicEvent(commandDragging as MusicCommand, scaleToTime(mouseX), droppedOn, (AudioDevice)GetDefaultDevice(Types.AUDIO));
                        }
                        else if (type.Equals(Types.VIDEO))
                        {
                            @event = new VideoEvent(commandDragging as VideoCommand, scaleToTime(mouseX), droppedOn, (VideoDevice)GetDefaultDevice(Types.VIDEO));
                        }
                        else if (type.Equals(Types.MIDI))
                        {
                            @event = new MidiEvent(commandDragging as MidiCommand, scaleToTime(mouseX), droppedOn, (MidiDevice)GetDefaultDevice(Types.MIDI));
                        }

                        if (@event != null)
                        {
                            if ((commandDragging as AbstractCommand).type.Equals(outputs.DisplayList[row].type) || (commandDragging as AbstractCommand).type.Equals(outputs.DisplayList[row].ghostType))
                            {
                                if (StartingOutput == null)
                                {
                                    @event.OutputDevice = outputs.DisplayList[row];
                                    droppedOn.ChangeEventTime(@event, scaleToTime(mouseX));
                                    actionManager.PerformAction(new NewEventAction(@event, droppedOn));
                                }
                                else
                                {
                                    droppedOn.addEvent(@event);
                                    actionManager.PerformAction(new EditEventAction(@event, outputs.DisplayList[row], scaleToTime(mouseX)));
                                    StartingOutput = null;
                                }
                            }
                            else if (StartingOutput != null)
                            {
                                droppedOn.addEvent(@event);
                                actionManager.PerformAction(new EditEventAction(@event, scaleToTime(mouseX)));
                                StartingOutput = null;
                            }
                            else
                            {
                                if (ShowView) @event.setStartTime(@event.startTime - sceneList.getSceneStartTime(droppedOn));
                                CreateEventWindow window = new CreateEventWindow(droppedOn, commandList, @event, DoAction);
                                window.setEditing(false);
                                window.Owner = MainWindow.main;
                                window.ShowDialog();
                            }
                        }
                    }
                }
                else if (LaneGrid.IsMouseOver && ShowView)
                {
                    if (draggingEvent || draggingCommand)
                    {
                        MessageBoxResult result =
                          MessageBox.Show(
                            "Events can't be dragged to the timeline while in Show View",
                            "Show View",
                            MessageBoxButton.OK,
                            MessageBoxImage.None);
                    }
                    else if (draggingScene)
                    {
                        double mouseX = e.GetPosition(LaneGrid).X;
                        Scene droppedOn = sceneList.GetSceneAtTime(scaleToTime(mouseX));
                        int i = sceneList.Scenes.IndexOf(droppedOn);
                        if (StartingIndex < sceneList.Scenes.Count)
                        {
                            sceneList.InsertItem(StartingIndex, DragScene);
                        }
                        else
                        {
                            sceneList.Scenes.Add(DragScene);
                        }

                        //move scene to new location
                        if (droppedOn != null)
                        {
                            if (i - StartingIndex != 0)
                            {
                                actionManager.PerformAction(new RearangeSceneAction(DragScene, i - StartingIndex, sceneList));
                            }
                        }
                        else if (StartingIndex + 1 < sceneList.Scenes.Count)
                        {
                            actionManager.PerformAction(new RearangeSceneAction(DragScene, sceneList.Scenes.Count - StartingIndex - 1, sceneList));
                        }
                        DrawTimeline();
                    }
                }
                else if (draggingScene)
                {
                    //replace scene if dropped in an invalid place
                    if (StartingIndex < sceneList.Scenes.Count)
                    {
                        sceneList.InsertItem(StartingIndex, DragScene);
                    }
                    else
                    {
                        sceneList.Scenes.Add(DragScene);
                    }
                    DrawTimeline();
                }

                AdornerLayer adorner = AdornerLayer.GetAdornerLayer(Content);
                adorner.Remove(hover);
                hover = null;
            }
            if (StartingOutput != null)
            {
                actionManager.PerformAction(new DeleteEventAction(DragEvent, sceneList.Scenes[sceneList.currentScenePlaying]));
            }
            DragListItem = null;
            DragEvent = null;
            DragScene = null;
            StartingOutput = null;
            draggingEvent = false;
            draggingCommand = false;
            Console.WriteLine("Release Scene");
            draggingScene = false;
            StartingIndex = -1;
            dragStartLocation = new Point(-1000, -1000);
        }

        //selects a default device for the events to output to 
        public OutputDevice GetDefaultDevice(String type)
        {
            return outputs.GetDefaultDevice(type);
        }

        private void ListItem_Dragged(object sender, MouseEventArgs e)
        {
            if (dragStartLocation.X > 0)
            {
                double width = SystemParameters.MinimumHorizontalDragDistance;
                double height = SystemParameters.MinimumVerticalDragDistance;
                AdornerLayer adorner = AdornerLayer.GetAdornerLayer(Content);
                if (!draggingCommand && DragListItem != null && Math.Abs(dragStartLocation.X - e.GetPosition(DragListItem).X) > width && Math.Abs(dragStartLocation.Y - e.GetPosition(DragListItem).Y) > height)
                {
                    draggingCommand = true;
                    hover = new ListViewItemAdorner(DragListItem, Content);
                    adorner.Add(hover);

                }
                if (!draggingEvent && DragEvent != null && Math.Abs(dragStartLocation.X - e.GetPosition(this).X) > width && Math.Abs(dragStartLocation.Y - e.GetPosition(this).Y) > height)
                {
                    draggingEvent = true;
                    hover = new EventAdorner(DragEvent, Content);
                    adorner.Add(hover);

                }
                if (!draggingScene && DragScene != null && Math.Abs(dragStartLocation.X - e.GetPosition(Content).X) > width && Math.Abs(dragStartLocation.Y - e.GetPosition(Content).Y) > height)
                {
                    if (sceneList.Scenes.Count > 1)
                    {
                        Console.WriteLine("Start dragging " + DragScene.name);
                        draggingScene = true;
                        StartingIndex = sceneList.Scenes.IndexOf(DragScene);
                        hover = new SceneAdorner(DragScene, Content);
                        sceneList.DeleteItem(DragScene);
                        DrawTimeline();
                        adorner.Add(hover);
                    }
                }
                if (draggingCommand || draggingEvent || draggingScene)
                {
                    if (hover != null)
                    {
                        hover.SetOffsets(e.GetPosition(Content).X, e.GetPosition(Content).Y);
                    }
                }
            }
        }

        public void EventDragged(Event @event, MouseEventArgs e)
        {
            sceneList.Scenes[sceneList.currentScenePlaying].removeEvent(@event);
            DrawTimeline();
            AdornerLayer adorner = AdornerLayer.GetAdornerLayer(Content);
            draggingEvent = true;
            DragEvent = @event;
            StartingOutput = @event.OutputDevice;
            dragStartLocation = e.GetPosition(Content);
            hover = new EventAdorner(@event, Content);
            (hover as EventAdorner).SetOffsets(e.GetPosition(Content).X, e.GetPosition(main.Content).Y);
            adorner.Add(hover);
        }

        //highlight a lane when an output on the timeline key is selected
        private void OutputList_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            int index = (sender as ListView).SelectedIndex;
            if (item != null && sceneList.currentScenePlaying >= 0 && sceneList.currentScenePlaying < sceneList.Scenes.Count)
            {
                if (ShowView)
                {
                    foreach (Scene scene in sceneList.Scenes)
                    {
                        scene.highlightLane(index);
                    }
                }
                else
                {
                    sceneList.Scenes[sceneList.currentScenePlaying].highlightLane(index);
                }
            }
        }

        // Closing main window event
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string msg = "Save changes before closing?";
            doYouWantToSaveDialog(sender, msg, e);
        }

        private void doYouWantToSaveDialog(object sender, string message, System.ComponentModel.CancelEventArgs e)
        {
            // changes made prompt to save
            if (changesMade || sceneList.changesMade || actionManager.ChangesMade())
            {
                while (e.Cancel == false)
                {
                    MessageBoxResult result =
                      MessageBox.Show(
                        message,
                        "Show Automation",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.None);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                    if (result == MessageBoxResult.Yes)
                    {
                        btnSaveProject_Click(sender, e);
                        if (e.Cancel == true)
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        //called when ASCS crashes
        private void handleCrash(object sender, UnhandledExceptionEventArgs e)
        {
            String msg = "Application is Crashing!\r\nDo you want to save changes?\r\nWARNING: saving might corrupt data";
            doYouWantToSaveDialog(sender, msg, new CancelEventArgs());
        }

        internal bool ShouldMidiCatchUp()
        {
            if (MidiShouldCatchUp)
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "Do you want to play all MIDI events upto the current time?",
                        "Show Automation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                        );
                return result == MessageBoxResult.Yes;
            }
            else
            {
                return false;
            }
        }

        //changes screen cordinates to times in milliseconds
        public ulong scaleToTime(double xLocation)
        {
            return (ulong)Math.Floor(xLocation / timelineInfo.getScale());//todo calculate conversion
        }

        //changes times in milliseconds to screen cordinates
        public double scaleToScreen(ulong time)
        {
            return time * timelineInfo.getScale();
        }

        //draw current scene on timeline
        public void DrawTimeline()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                LaneGrid.Children.Clear();
                LaneGrid.RowDefinitions.Clear();
                LaneGrid.ColumnDefinitions.Clear();
            });

            if (ShowView)
            {
                DrawShow();
                UpdateSceneLabels(sceneList.Scenes);
            }
            else
            {
                DrawScene();
            }
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                TimelineKey.ScrollToHorizontalOffset(Timeline.HorizontalOffset);
            });
        }

        public void DrawShow()
        {
            if (sceneList.Scenes.Count > 0 && sceneList.sceneInViewIndex >= 0 && sceneList.sceneInViewIndex < sceneList.Scenes.Count)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    int i = 0;
                    timelineInfo.SetUpInfo(sceneList.Scenes[0], null);

                    foreach (Scene scene in sceneList.Scenes)
                    {
                        ColumnDefinition SceneCol = new ColumnDefinition();
                        SceneCol.Width = GridLength.Auto;
                        SceneCol.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
                        LaneGrid.ColumnDefinitions.Add(SceneCol);

                        Grid SceneGrid = new Grid();
                        SceneGrid.DataContext = scene;
                        SceneGrid.MouseDown += SceneOnTimelineClick;
                        Grid.SetColumn(SceneGrid, i);
                        SceneGrid.MouseWheel += new MouseWheelEventHandler(Timeline_ScrollChanged);
                        LaneGrid.Children.Add(SceneGrid);

                        scene.drawLanes(SceneGrid, KeyGrid, Timeline, outputs.DisplayList);
                        i++;
                    }

                    LaneGrid.MouseMove += updateTimelineInfo;
                    LaneGrid.MouseLeave += clearCursorLocation;
                });
            }
        }

        public void DrawScene()
        {
            if (sceneList.Scenes.Count > 0 && sceneList.sceneInViewIndex >= 0 && sceneList.sceneInViewIndex < sceneList.Scenes.Count)
            {
                Scene scene = sceneList.Scenes[sceneList.sceneInViewIndex];
                System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    timelineInfo.SetUpInfo(scene, null);
                    scene.drawLanes(LaneGrid, KeyGrid, Timeline, outputs.DisplayList);
                    LaneGrid.MouseMove += updateTimelineInfo;
                    LaneGrid.MouseLeave += clearCursorLocation;
                });
            }
        }

        //scroll Timeline to show timestamp
        internal void scrollTimelineToTime(ulong elapsedMilliseconds, Scene scene)
        {
            ulong time = elapsedMilliseconds;
            if (ShowView)
            {
                time += sceneList.getSceneStartTime(scene);
            }
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                Timeline.ScrollToHorizontalOffset(scaleToScreen(time) - Timeline.ViewportWidth / 2);
            });
        }

        //clear cursor time from timeline info
        private void clearCursorLocation(object sender, MouseEventArgs e)
        {
            timelineInfo.clearCursorLocation();
            updateTimelineInfo(sender, null);
        }

        //updates timeline info
        public void updateTimelineInfo(object sender, MouseEventArgs e)
        {
            Scene scene = sceneList.Scenes[sceneList.currentScenePlaying];
            timelineInfo.updateTimeInfo(scene, e);
        }

        //timelineArea size change and scroll change methods used to keep horizontal timeline scroll bar on screen
        private void TimelineArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                ScrollViewer area = sender as ScrollViewer;
                Timeline.VerticalAlignment = VerticalAlignment.Top;
                double newHeight = area.VerticalOffset + area.ViewportHeight;
                if (newHeight > lvOutputList.RenderSize.Height + 37)
                {
                    newHeight = lvOutputList.RenderSize.Height + 37;
                }
                if (ShowView) newHeight += 30;
                Timeline.Height = newHeight;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                ScrollViewer area = sender as ScrollViewer;
                Timeline.VerticalAlignment = VerticalAlignment.Top;
                double newHeight = area.VerticalOffset + area.ViewportHeight;
                if (newHeight > lvOutputList.RenderSize.Height + 37)
                {
                    newHeight = lvOutputList.RenderSize.Height + 37;
                }
                if (ShowView) newHeight += 30;
                Timeline.Height = newHeight;


            }

        }

        //called when scroll wheel changed
        public void Timeline_ScrollChanged(object sender, MouseWheelEventArgs e)
        {
            int scrollSideSpeed = 3;
            if (!zooming)
            {
                zoomValue = ((timelineInfo.getScale() * 100000.0));
            }
            double previousZoom = zoomValue;
            if (sceneList.Scenes.Count > 0)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (e.Delta > 0)
                    {
                        scrolledThisInterval = true;
                        zoomValue += 100.0;
                        if (zoomValue >= 24000)
                        {
                            zoomValue = 24000;
                        }
                        if (!zooming)
                        {
                            zooming = true;
                            stillZooming();
                        }
                    }
                    else
                    {
                        scrolledThisInterval = true;
                        zoomValue -= 100.0;
                        if (zoomValue <= timelineInfo.getZoomToShowVal(sceneList, Timeline))
                        {
                            zoomValue = timelineInfo.getZoomToShowVal(sceneList, Timeline);
                        }
                        if (!zooming)
                        {
                            zooming = true;
                            stillZooming();
                        }
                    }
                    if (zooming && zoomValue < 1000)
                    {
                        redrawAtInterval = false;
                        timelineInfo.updateTimeInfoWithWheel(zoomValue);
                    }
                    else if (zoomValue >= 1000)
                    {
                        redrawAtInterval = true;
                    }
                }
                else if ((System.Windows.Forms.Form.ModifierKeys == System.Windows.Forms.Keys.Shift))
                {
                    if (e.Delta > 0)
                    {
                        for (int i = 0; i < scrollSideSpeed; i++)
                        {
                            Timeline.LineLeft();
                            TimelineKey.LineLeft();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < scrollSideSpeed; i++)
                        {
                            Timeline.LineRight();
                            TimelineKey.LineLeft();
                        }
                    }
                    e.Handled = true;
                }
                else // Only mouse wheeling
                {
                    if (e.Delta > 0)
                    {
                        TimelineArea.LineUp();
                    }
                    else
                    {
                        TimelineArea.LineDown();
                    }
                    e.Handled = true;

                }
            }
        }

        private void stretchToZoom(double stretch)
        {
            LaneGrid.LayoutTransform = new ScaleTransform(stretch, 1);
        }

        private void stillZooming()     //starts timer to check whether or not the user is still zooming in
        {
            Timer timer = new Timer();
            timer.Interval = 450;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)   //every interval, it checks if the user has scrolled at all
        {
            if (redrawAtInterval)
            {
                timelineInfo.updateTimeInfoWithWheel(zoomValue);
            }
            if (scrolledThisInterval == false)
            {
                zooming = false;
                (sender as Timer).Stop();
                (sender as Timer).Dispose();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LaneGrid.LayoutTransform = new ScaleTransform(1, 1);
                });
                timelineInfo.updateTimeInfoWithWheel(zoomValue);
            }
            else
            {
                scrolledThisInterval = false;
            }
        }

        public bool redrawTicker()
        {
            return !zooming;
        }


        private void SearchBox_Resize(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                double width = MeasureSearchInput(((TextBox)sender).Text, (TextBox)sender).Width + 50;
                if (width < 300)
                {
                    ((TextBox)sender).Width = width;
                }
            }
        }

        private Size MeasureSearchInput(string input, TextBox Search)   //Measures String to change size
        {
            var formattedText = new FormattedText(input, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(Search.FontFamily, Search.FontStyle, Search.FontWeight, Search.FontStretch), Search.FontSize, Brushes.Black);
            return new Size(formattedText.Width, formattedText.Height);
        }

        public void btnSearchClick(object sender, RoutedEventArgs e)    //Searches Help guide for inputted term
        {
            searchTerm = SearchBox.Text;
            SearchBox.Clear();
            Console.WriteLine("Search: (" + searchTerm + ")");
            HelpDatabaseManager.openHelpWindow();
            HelpDatabaseManager.searchDatabase(searchTerm);
        }

        public void btnHelp(object sender, RoutedEventArgs e)       //Opens Help Guide
        {
            HelpDatabaseManager.openHelpWindow();
            HelpDatabaseManager.searchCategories("", 0);
        }

        private void ChangeNameOfScene(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).IsReadOnly = false;
        }

        private void FinishSceneNameChange(object sender, EventArgs e)
        {
            if (e is KeyEventArgs)
            {
                if ((e as KeyEventArgs).Key == Key.Enter)
                {
                    actionManager.PerformAction(new RenameSceneAction((sender as TextBox).DataContext as Scene, (sender as TextBox).Text));
                }
                else
                {
                    return;
                }
            }
            (sender as TextBox).Text = ((sender as TextBox).DataContext as Scene).name;
            (sender as TextBox).IsReadOnly = true;
        }

        private void ShowCommandList_Click(object sender, RoutedEventArgs e)
        {
            if (CommandList.Visibility == Visibility.Visible)
            {
                CommandList.Visibility = Visibility.Collapsed;
                ListSplitter.Visibility = Visibility.Collapsed;
                CommandListCol.Width = new GridLength(0);
            }
            else
            {
                CommandList.Visibility = Visibility.Visible;
                ListSplitter.Visibility = Visibility.Visible;
                CommandListCol.Width = new GridLength(250);
            }
        }

        public void ShowVideo_Click(object sender, RoutedEventArgs e)
        {
            if (VideoViewer.Visibility == Visibility.Visible)
            {
                VideoViewer.Visibility = Visibility.Collapsed;
                VideoSplitter.Visibility = Visibility.Collapsed;
                VideoViewerCol.Width = new GridLength(0);
            }
            else
            {
                previewDevice = (VideoDevice)outputs.getOutputByName(getMainScreen().DeviceName);
                UpdateMonitorTabs();

                VideoViewer.Visibility = Visibility.Visible;
                VideoSplitter.Visibility = Visibility.Visible;
                VideoViewerCol.Width = new GridLength(400);
            }
        }

        internal void UpdateMonitorTabs()
        {
            MonitorTabs.Children.Clear();
            foreach (VideoDevice device in outputs.getOutputs(Types.VIDEO, Mode.isEditMode))
            {
                if (!device.IsDuplicate)
                {
                    Button deviceButton = new Button();
                    deviceButton.Content = " " + device.deviceName + " ";
                    deviceButton.Height = 25;
                    deviceButton.DataContext = device;
                    deviceButton.Click += changePreviewDevice;
                    deviceButton.IsEnabled = device != previewDevice;
                    MonitorTabs.Children.Add(deviceButton);
                }
            }
        }

        private void mutePreview(object sender, RoutedEventArgs e)
        {
            previewMute = !previewMute;
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                if (VideoPlayer != null)
                {
                    if (previewMute)
                    {
                        VideoPlayer.Volume = 0;
                    }
                    else
                    {
                        VideoPlayer.Volume = 1;
                    }
                }
            });
        }

        private void changePreviewDevice(object sender, RoutedEventArgs e)
        {
            previewDevice = (sender as Button).DataContext as VideoDevice;
            foreach (Button button in MonitorTabs.Children)
            {
                button.IsEnabled = button.DataContext != previewDevice;
            }
            Console.WriteLine("\nShow video for " + previewDevice.deviceName + "\n");
            setPreviewViewer(previewDevice, previewDevice.getPreviewViewer());
        }

        internal void setPreviewViewer(VideoDevice source, MediaElement previewVideoViewer)
        {
            if (source == previewDevice)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    if (VideoPlayer != null)
                    {
                        VideoPlayer.Pause();
                        VideoViewer.Children.Remove(VideoPlayer);
                    }
                    if (previewVideoViewer != null)
                    {
                        Grid.SetRow(previewVideoViewer, 1);
                        VideoViewer.Children.Add(previewVideoViewer);
                    }
                    VideoPlayer = previewVideoViewer;
                    if (previewMute && VideoPlayer != null)
                    {
                        VideoPlayer.Volume = 0;
                    }
                    else if (VideoPlayer != null)
                    {
                        VideoPlayer.Volume = 1;
                    }
                });
            }
        }

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            if (actionManager.CanUndo())
            {
                actionManager.UndoLastAction();
            }
        }

        private void RedoClick(object sender, RoutedEventArgs e)
        {
            if (actionManager.CanRedo())
            {
                actionManager.RedoLastAction();
            }
        }


        public System.Windows.Forms.Screen getMainScreen()
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                rect.X = (int)this.Left;
                rect.Y = (int)this.Top;
                rect.Width = (int)this.Width;
                rect.Height = (int)this.Height;
            });

            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.Bounds.IntersectsWith(rect))
                    return screen;
            }
            return null;
        }

        private void Timeline_Scrolled(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                TimelineKey.ScrollToHorizontalOffset(Timeline.HorizontalOffset);
            }
        }

        private void setMaxCommandListWidth(object sender, SizeChangedEventArgs e)
        {
            double width = Window.Width - (70 + VideoViewerCol.Width.Value + VideoSplitter.Width + ListSplitter.Width);
            CommandListCol.MaxWidth = width > 0 ? width : 0;
            VideoViewerCol.MaxWidth = Window.Width - (70 + VideoSplitter.Width + ListSplitter.Width);
        }

        public void btnPlayCommand(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.Equals("Play"))
            {
                if (playingButton != null && playingButton.Content.Equals("Stop"))
                {
                    btnPlayCommand(playingButton, e);
                }
                if (playingMenuItem != null && playingMenuItem.Header.Equals("Stop Playing Command"))
                {
                    btnPlayCommandMenuClick(playingMenuItem, e);
                }
                Button button = sender as Button;
                AbstractCommand command = button.DataContext as AbstractCommand;
                Guid id = command.id;
                (sender as Button).Content = "Stop";
                //command.execute();
                playCommand(command);

                playingButton = button;
                CommandTimer = new Timer();
                CommandTimer.AutoReset = false;
                CommandTimer.Interval = command.getDuration();
                CommandTimer.Elapsed += CommandEnded;
                CommandTimer.Start();
            }
            else
            {
                Button button = sender as Button;
                AbstractCommand command = button.DataContext as AbstractCommand;
                Guid id = command.id;
                (sender as Button).Content = "Play";
                //command.stop();
                previewEvent.Stop();

                CommandTimer.Stop();
            }
        }

        private void CommandEnded(object sender, ElapsedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                if (playingButton != null)
                {
                    playingButton.Content = "Play";
                }
                if (playingMenuItem != null)
                {
                    playingMenuItem.Header = "Play Command";
                }
            });
            CommandTimer.Stop();
        }

        public void btnPlayCommandMenuClick(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Header.Equals("Play Command"))
            {
                if (playingButton != null && playingButton.Content.Equals("Stop"))
                {
                    btnPlayCommand(playingButton, e);
                }
                if (playingMenuItem != null && playingMenuItem.Header.Equals("Stop Playing Command"))
                {
                    btnPlayCommandMenuClick(playingMenuItem, e);
                }
                (sender as MenuItem).Header = "Stop Playing Command";
                playingMenuItem = (sender as MenuItem);
                CommandTimer = new Timer();
                CommandTimer.AutoReset = false;
                CommandTimer.Interval = ((sender as MenuItem).DataContext as AbstractCommand).getDuration();
                CommandTimer.Elapsed += CommandEnded;
                CommandTimer.Start();
                //((sender as MenuItem).DataContext as AbstractCommand).execute();
                playCommand((sender as MenuItem).DataContext as AbstractCommand);
            }
            else
            {
                (sender as MenuItem).Header = "Play Command";
                //((sender as MenuItem).DataContext as AbstractCommand).stop();
                previewEvent.Stop();
                CommandTimer.Stop();
            }
        }

        private void playCommand(AbstractCommand command)
        {
            String commandType = command.type;
            if (commandType == Types.AUDIO)
            {
                previewEvent = new MusicEvent((MusicCommand)command, 0, null, (AudioDevice)ShowAutomation.MainWindow.main.outputs.GetPreviewDevice(Types.AUDIO));
                previewEvent.LoadAt(0);
                previewEvent.Play();
            }
            else if (commandType == Types.VIDEO)
            {
                previewEvent = new Domain.Models.Events.VideoEvent((VideoCommand)command, 0, null, (VideoDevice)ShowAutomation.MainWindow.main.outputs.GetPreviewDevice(Types.VIDEO));
                previewEvent.LoadAt(0);
                if (ShowAutomation.MainWindow.main.VideoViewer.Visibility == Visibility.Collapsed)
                {
                    ShowAutomation.MainWindow.main.ShowVideo_Click(null, null);
                    ShowAutomation.MainWindow.main.HideVideoViewer.IsChecked = true;
                }
                previewEvent.Play();
            }
            else if (commandType == Types.MIDI)
            {
                previewEvent = new Domain.Models.Events.MidiEvent((MidiCommand)command, 0, null, (MidiDevice)ShowAutomation.MainWindow.main.outputs.GetPreviewDevice(Types.MIDI));
                previewEvent.LoadAt(0);
                previewEvent.Play();
            }
        }

        private void OutputKeyResized(object sender, SizeChangedEventArgs e)
        {
            Grid item = sender as Grid;
            TimelineKeyArea.ColumnDefinitions[0].Width = new GridLength(lvOutputList.ActualWidth);
            if (item.Children.Count >= 2)
            {
                (item.Children[0] as TextBlock).Width = lvOutputList.ActualWidth - (item.ColumnDefinitions[1].ActualWidth);
                (item.Children[1] as TextBlock).Width = lvOutputList.ActualWidth - (item.ColumnDefinitions[1].ActualWidth);
            }
        }

        private void CommandListResized(object sender, SizeChangedEventArgs e)
        {
            Grid item = sender as Grid;
            (item.Children[0] as TextBlock).Width = ShowAutomation.MainWindow.main.lvCommandList.ActualWidth - (item.ColumnDefinitions[1].ActualWidth + ShowAutomation.MainWindow.main.lvCommandList.Margin.Right + ShowAutomation.MainWindow.main.lvCommandList.Margin.Left);
        }
    }
}