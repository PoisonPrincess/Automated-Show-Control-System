using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NAudio.Midi;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.Domain.Utils.Midi;
using System.Timers;
using ShowAutomation.Domain.Actions;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateCommandWindow.xaml
    /// </summary>
    public partial class CreateCommandWindow : Window
    {
        // Fields

        private AbstractCommand Command { get; set; }
        private Domain.Models.Events.Event previewEvent;
        private Timer previewTimer = new Timer();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private bool IsEditing = false;
        private String filePath;
        private String fileName;
        private String musicFileFilter = "Song files (*.wav, *.mp3,*.pcm, *.mp4, *.aiff, *.mpeg, *.wma, *.m4a) | *.wav; *.mp3; *.pcm; *.mp4; *.aiff; *.mpeg; *.wma; *.m4a";
        private String videoFileFilter = "Video files (*.avi, *.mp4, *.mpeg, *.wmv) | *.avi; *.mp4; *.mpeg; *.wmv";
        private MidiDeviceManager midiDeviceManager = MidiDeviceManager.Instance;
        private ObservableCollection<MidiEvent> MidiEvents { get; set; } = new ObservableCollection<MidiEvent>();
        private Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction;
        private String _errorMessages = null;
        public Boolean _hasErrors = false;
        private static readonly int COLUMS = 3;
        private Color midiColor;
        private bool previewKey;

        // Constructors
        //called for every creating command
        public CreateCommandWindow(Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction)
        {
            this.doneFunction = doneFunction;
            InitializeComponent();
            this.previewKey = true;

            // Default to MIDI.
            DeviceProfileComboBox.SelectedIndex = 0;
            CommandCodeComboBox.SelectedIndex = 0;
            ChannelComboBox.SelectedIndex = 0;
            DataByte1ComboBox.SelectedIndex = 0;
            DataByte2ComboBox.SelectedIndex = 0;
            MidiDeviceComboBox.ItemsSource = MainWindow.main.outputs.getOutputs(Types.MIDI, false);
            MidiDeviceComboBox.SelectedItem = MainWindow.main.outputs.GetPreviewDevice(Types.MIDI);
            PreviewButton.IsEnabled = this.IsValidCommand_Midi();
            AddButton.IsEnabled = this.IsValidCommand_Midi();
            DataGrid.ItemsSource = MidiEvents;
            ColorPicker.SelectedColor = Colors.LightBlue;
            PreviewAllButton.IsEnabled = false;
        }

        //called when creating a command of a certan type
        public CreateCommandWindow(String commandType, Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction) : this(doneFunction)
        {
            comboCommandType.Text = commandType;
            comboCommandType.IsEnabled = false;
            setVisibilityCommandType(commandType);
        }

        //called when editing a command
        public CreateCommandWindow(AbstractCommand command, Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction) : this(command.type, doneFunction)
        {
            IsEditing = true;
            Command = command;
            txtCommandName.Text = Command.name;
            ColorPicker.SelectedColor = Command.color;
            PreviewAllButton.IsEnabled = true;

            if (Command.type == Types.AUDIO)
            {
                var song = (Command as MusicCommand).media as Song;
                filePath = txtFilePath.Text = song.songPath;
            }

            if (Command.type == Types.VIDEO)
            {
                var video = (Command as VideoCommand).media as Video;
                filePath = txtFilePath.Text = video.videoPath;
            }

            if (Command.type == Types.MIDI)
            {
                var midiEvents = (Command as MidiCommand).MidiEvents;
                MidiEvents = new ObservableCollection<MidiEvent>(midiEvents);
                DataGrid.ItemsSource = MidiEvents;
            }
        }

        //Import button
        public void btnImportFileClick(object sender, RoutedEventArgs e)
        {
            String commandType = comboCommandType.Text;

            //Open File explore and brings in video file if textbox2 == aud string 
            //creates the aud object and stores in command list
            OpenFileDialog open = new OpenFileDialog();

            if (commandType == Types.AUDIO)
            {
                open.Filter = musicFileFilter;
            }
            else if (commandType == Types.VIDEO)
            {
                open.Filter = videoFileFilter;
            }

            if (open.ShowDialog() == DialogResult)
            {
                return;
            }

            filePath = open.FileName;
            fileName = Path.GetFileName(filePath);
            txtFilePath.Text = filePath;
            txtCommandName.Text = fileName;
            PreviewAllButton.IsEnabled = true;
        }

        //Error messages 
        protected void showErrors()
        {
            if (this._hasErrors)
            {
                txtBlockErrorMessageCmd.Text = this._errorMessages;
                txtBlockErrorMessageCmd.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtBlockErrorMessageCmd.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void setVisibilityCommandType(String commandType)
        {
            if (commandType == Types.AUDIO || commandType == Types.VIDEO)
            {
                labelName.Visibility = Visibility.Visible;
                txtCommandName.Visibility = Visibility.Visible;
                labelFile.Visibility = Visibility.Visible;
                txtFilePath.Visibility = Visibility.Visible;
                importFileBtn.Visibility = Visibility.Visible;
                MidiFormGrid.Visibility = Visibility.Hidden;
                Application.Current.MainWindow = this;
                Application.Current.MainWindow.Height = 220;
                Application.Current.MainWindow.Width = 300;
            }
            else if (commandType == Types.MIDI)
            {
                labelName.Visibility = Visibility.Visible;
                txtCommandName.Visibility = Visibility.Visible;
                labelFile.Visibility = Visibility.Hidden;
                txtFilePath.Visibility = Visibility.Hidden;
                importFileBtn.Visibility = Visibility.Hidden;
                MidiFormGrid.Visibility = Visibility.Visible;
                Application.Current.MainWindow = this;
                Application.Current.MainWindow.Height = 530;
                Application.Current.MainWindow.Width = 700;
            }
        }

        private void comboCommandTypeOnChange(object sender, EventArgs e)
        {
            String commandType = comboCommandType.Text;
            setVisibilityCommandType(commandType);
            filePath = "";
            txtFilePath.Text = "";

            PreviewAllButton.IsEnabled = false;
        }

        // MIDI

        private void DeviceProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0:
                    this.DeviceProfileComboBox_SelectionChanged_Midi();
                    break;

                // Behringer X32
                case 1:
                    this.DeviceProfileComboBox_SelectionChanged_BehringerX32();
                    break;

                // Behringer X Air
                case 2:
                    this.DeviceProfileComboBox_SelectionChanged_BehringerXAir();
                    break;

                default:
                    return;
            }
        }

        private void CommandCodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0:
                    this.CommandCodeComboBox_SelectionChanged_Midi();
                    break;

                // Behringer X32
                case 1:
                    this.CommandCodeComboBox_SelectionChanged_BehringerX32();
                    break;

                // Behringer X Air
                case 2:
                    this.CommandCodeComboBox_SelectionChanged_BehringerXAir();
                    break;

                default:
                    return;
            }
        }

        private void ChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0:
                    this.ChannelComboBox_SelectionChanged_Midi();
                    break;

                // Behringer X32
                case 1:
                    this.ChannelComboBox_SelectionChanged_BehringerX32();
                    break;

                // Behringer X Air
                case 2:
                    this.ChannelComboBox_SelectionChanged_BehringerXAir();
                    break;

                default:
                    return;
            }
        }

        private void DataByte1ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0:
                    this.DataByte1ComboBox_SelectionChanged_Midi();
                    break;

                // Behringer X32
                case 1:
                    this.DataByte1ComboBox_SelectionChanged_BehringerX32();
                    break;

                // Behringer X Air
                case 2:
                    this.DataByte1ComboBox_SelectionChanged_BehringerXAir();
                    break;

                default:
                    return;
            }
        }

        private void DataByte2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0:
                    this.DataByte2ComboBox_SelectionChanged_Midi();
                    break;

                // Behringer X32
                case 1:
                    this.DataByte2ComboBox_SelectionChanged_BehringerX32();
                    break;

                // Behringer X Air
                case 2:
                    this.DataByte2ComboBox_SelectionChanged_BehringerXAir();
                    break;

                default:
                    return;
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var midiEvent = GetMidiEvent();
            var midiCommand = new MidiCommand(midiEvent);
            var midiEvents = new List<MidiEvent>(MidiEvents);
            Domain.Models.Events.MidiEvent ev = new Domain.Models.Events.MidiEvent(midiCommand, 0, null, MidiDeviceComboBox.SelectedItem as MidiDevice);
            ev.LoadAt(0);
            ev.Play();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var midiEvent = GetMidiEvent();
            MidiEvents.Add(midiEvent);
            PreviewAllButton.IsEnabled = true;

        }

        private void DataGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var midiEvent = dataGrid.SelectedItem as MidiEvent;
            var midiCommand = new MidiCommand(midiEvent);
            Domain.Models.Events.MidiEvent ev = new Domain.Models.Events.MidiEvent(midiCommand, 0, null, MidiDeviceComboBox.SelectedItem as MidiDevice);
            ev.Play();
        }

        private void PreviewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var dataGrid = contextMenu.PlacementTarget as DataGrid;
            var midiEvents = new List<MidiEvent>();

            foreach (MidiEvent midiEvent in dataGrid.SelectedItems)
            {
                midiEvents.Add(midiEvent);
            }

            var midiCommand = new MidiCommand(midiEvents);
            Domain.Models.Events.MidiEvent ev = new Domain.Models.Events.MidiEvent(midiCommand, 0, null, MidiDeviceComboBox.SelectedItem as MidiDevice);
            ev.LoadAt(0);
            ev.Play();
        }

        private void MoveUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var dataGrid = contextMenu.PlacementTarget as DataGrid;
            var midiEvent = dataGrid.SelectedItems[0] as MidiEvent;
            var oldIndex = MidiEvents.IndexOf(midiEvent);
            if (oldIndex > 0)
            {
                MidiEvents.Move(oldIndex, oldIndex - 1);
            }
        }

        private void MoveDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var dataGrid = contextMenu.PlacementTarget as DataGrid;
            var midiEvent = dataGrid.SelectedItems[0] as MidiEvent;
            var oldIndex = MidiEvents.IndexOf(midiEvent);
            if (oldIndex < MidiEvents.Count - 1)
            {
                MidiEvents.Move(oldIndex, oldIndex + 1);
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var dataGrid = contextMenu.PlacementTarget as DataGrid;
            var midiEvents = new List<MidiEvent>();

            foreach (MidiEvent midiEvent in dataGrid.SelectedItems)
            {
                midiEvents.Add(midiEvent);
            }

            foreach (MidiEvent midiEvent in midiEvents)
            {
                MidiEvents.Remove(midiEvent);
            }
        }

        // Preview/Save/Save As Buttons

        private void PreviewAllButton_Click(object sender, RoutedEventArgs e)
        {
            String commandType = comboCommandType.Text;
            Console.WriteLine(sender);
            if (previewEvent != null)
            {
                previewEvent.Stop();
                previewEvent = null;
                previewTimer.Stop();
                PreviewAllButton.Content = "Preview All";
            }
            else if (!MissingData(true))
            {
                PreviewAllButton.Content = "Stop Preview";
                if (commandType == Types.AUDIO)
                {
                    var song = new Song(filePath);
                    previewEvent = new Domain.Models.Events.MusicEvent(new MusicCommand("", song), 0, null, (AudioDevice)MainWindow.main.outputs.GetPreviewDevice(Types.AUDIO));
                    previewEvent.LoadAt(0);
                    previewEvent.Play();
                }
                else if (commandType == Types.VIDEO)
                {
                    var video = new Video(filePath);
                    previewEvent = new Domain.Models.Events.VideoEvent(new VideoCommand("", video), 0, null, (VideoDevice)MainWindow.main.outputs.GetPreviewDevice(Types.VIDEO));
                    previewEvent.LoadAt(0);
                    if (MainWindow.main.VideoViewer.Visibility == Visibility.Collapsed)
                    {
                        MainWindow.main.ShowVideo_Click(null, null);
                        MainWindow.main.HideVideoViewer.IsChecked = true;
                    }
                    previewEvent.Play();
                }
                else if (commandType == Types.MIDI)
                {
                    var midiEvents = new List<MidiEvent>(MidiEvents);
                    previewEvent = new Domain.Models.Events.MidiEvent(new MidiCommand(midiEvents), 0, null, MidiDeviceComboBox.SelectedItem as MidiDevice);
                    previewEvent.LoadAt(0);
                    previewEvent.Play();
                }
                previewTimer.Interval = previewEvent.duration;
                previewTimer.Elapsed += finishPreview;
                previewTimer.Start();
            }
        }

        //called when a command preview finishes
        private void finishPreview(object sender, ElapsedEventArgs e)
        {
            previewEvent = null;
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                PreviewAllButton.Content = "Preview All";
            });
            return;
        }

        //formats the save button for all conditions
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MissingData(false))
            {
                if (!IsEditing)
                {
                    CreateNewCommand();
                }
                else
                {
                    String name = txtCommandName.Text;
                    doneFunction(new EditCommandAction(Command, name, filePath, midiColor, new List<MidiEvent>(MidiEvents)));
                    Close();
                }
            }
        }

        private void CreateNewCommand()
        {
            String commandType = comboCommandType.Text;
            String name = txtCommandName.Text;

            if (commandType == Types.AUDIO)
            {
                var song = new Song(filePath);

                if (Command == null)
                {
                    Command = new MusicCommand(name, song);
                }
                else
                {
                    Command.name = name;
                    (Command as MusicCommand).media = song;
                }
            }
            else if (commandType == Types.VIDEO)
            {
                var video = new Video(filePath);

                if (Command == null)
                {
                    Command = new VideoCommand(name, video);
                }
                else
                {
                    Command.name = name;
                    (Command as VideoCommand).media = video;
                }
            }
            else if (commandType == Types.MIDI)
            {
                var midiEvents = new List<MidiEvent>(MidiEvents);
                Color color = midiColor;
                if (Command == null)
                {
                    Command = new MidiCommand(name, midiEvents);
                    Command.color = color;
                }
                else
                {
                    Command.name = name;
                    Command.color = midiColor;
                    (Command as MidiCommand).MidiEvents = midiEvents;
                }
                MainWindow.main.DrawTimeline();
            }
            doneFunction(new NewCommandAction(Command, MainWindow.main.commandList));
            Close();
        }

        private Boolean MissingData(bool preview)
        {
            String commandType = comboCommandType.Text;
            String name = txtCommandName.Text;
            if (commandType != Types.MIDI && commandType != Types.AUDIO && commandType != Types.VIDEO)
            {
                this._hasErrors = true;
                this._errorMessages = "Must Select Command Type!";
                this.showErrors();
                return true;
            }

            if (!preview && (name == null || name == ""))
            {
                this._hasErrors = true;
                this._errorMessages = "Must enter a name for command";
                this.showErrors();
                return true;
            }

            if (commandType != Types.MIDI && (filePath == null || filePath == "" || !File.Exists(filePath) || FileMissingAudio(filePath)))
            {
                this._hasErrors = true;
                this._errorMessages = "Must have a valid " + commandType + " file for the command";
                this.showErrors();
                return true;
            }
            if (commandType == Types.MIDI && MidiEvents.Count <= 0)
            {
                this._hasErrors = true;
                this._errorMessages = "Must have at least one midi message for the command";
                this.showErrors();
                return true;
            }

            if (commandType == Types.MIDI && midiColor == null)
            {
                this._hasErrors = true;
                this._errorMessages = "Must select a color";
                this.showErrors();
                return true;
            }
            this._hasErrors = false;
            this._errorMessages = "";
            this.showErrors();
            return false;
        }

        private bool FileMissingAudio(string filePath)
        {
            if (comboCommandType.Text.Equals(Types.AUDIO) && (filePath.EndsWith(".avi") || filePath.EndsWith(".mp4") || filePath.EndsWith(".mpeg") || filePath.EndsWith(".wmv"))) {
                 return !new Video(filePath).HasAudio();
            }
            return false;
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            Command = null;
            SaveButton_Click(null, null);
        }

        // Utility Methods

        private MidiEvent GetMidiEvent()
        {
            switch (DeviceProfileComboBox.SelectedIndex)
            {
                // MIDI
                case 0: return this.GetMidiEvent_Midi();

                // Behringer X32
                case 1: return this.GetMidiEvent_BehringerX32();

                // Behringer X Air
                case 2: return this.GetMidiEvent_BehringerXAir();

                default: return null;
            }
        }

        private void SetColor(object sender, RoutedEventArgs e)
        {
            midiColor = (Color)ColorPicker.SelectedColor;
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
