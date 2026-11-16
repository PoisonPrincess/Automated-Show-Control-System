using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using ShowAutomation.Domain.Utils.Midi;
using ShowAutomation.UI.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateEventWindow.xaml
    /// </summary>
    public partial class CreateEventWindow : Window, INotifyPropertyChanged
    {
        protected Scene scene;
        protected CommandListFilter clFilter;
        protected CommandList commandList;
        protected Boolean _hasErrors = false;
        protected String _errorMessage = "";
        protected Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction;
        protected Boolean isEditing = false;
        protected Event EventToEdit = null;
        protected MidiDeviceManager midiDeviceManager = MidiDeviceManager.Instance;
        private ulong endTime;
        private int hours;
        private int minutes;
        private int seconds;
        private int milliseconds;
        private Boolean Muted;
        private bool keyPreview;
       

        //called whenever a new create event window is made
        public CreateEventWindow(Scene scene, CommandList commandList, Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction)
        {
            InitializeComponent();
            this.keyPreview = true;
            this.scene = scene;
            this.commandList = commandList;
            if (clFilter == null)
            {
                this.clFilter = new CommandListFilter(commandList);
            }
            this.cmbCommands.ItemsSource = clFilter.getCommands();
            this.doneFunction = doneFunction;
           
        }

        //called when creating an event from the timeline
        public CreateEventWindow(Scene scene, CommandList commandList, Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction, ulong startTime, String eventType, OutputDevice lane) : this(scene, commandList, doneFunction)
        {
            this.clFilter = new CommandListFilter(commandList);
            Console.WriteLine("Create event with type " + eventType);
            this.clFilter.setFilter(eventType);

            cmbEventType.Text = eventType;
            cmbEventType.IsEnabled = false;

            txtHours.Text = ((startTime / (1000 * 60 * 60)) % 24).ToString();
            txtMinutes.Text = ((startTime / (1000 * 60)) % 60).ToString();
            txtSeconds.Text = ((startTime / 1000) % 60).ToString();
            txtMilliseconds.Text = (startTime % 1000).ToString();
            foreach (Object device in deviceComboBox.Items)
            {
                if (device == lane)
                {
                    deviceComboBox.SelectedItem = lane;
                }
            }
        }

        //called when editing an event
        public CreateEventWindow(Scene scene, CommandList commandList, Event sceneEvent, Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction) : this(scene, commandList, doneFunction)
        {
            this.isEditing = true;
            cmbEventType.Text = sceneEvent.type;
            this.clFilter = new CommandListFilter(commandList);
            this.clFilter.setFilter(sceneEvent.type);

            EventToEdit = sceneEvent;
            
            cmbEventType.IsEnabled = false;
            SelectCommandFromList(sceneEvent);
            if (sceneEvent.OutputDevice != null)
            {
                foreach (Object device in deviceComboBox.Items) {
                    if (device == sceneEvent.OutputDevice) {
                        deviceComboBox.SelectedItem = sceneEvent.OutputDevice;
                    }
                }
            }
            
            TimeSpan startTime = TimeSpan.Parse(sceneEvent.startTimeFormatted);
            txtHours.Text = startTime.Hours.ToString();
            txtMinutes.Text = startTime.Minutes.ToString();
            txtSeconds.Text = startTime.Seconds.ToString();
            txtMilliseconds.Text = startTime.Milliseconds.ToString();
            if (sceneEvent is VideoEvent) {
                isMute.IsChecked = (((VideoCommand)sceneEvent.command).media as Video).isMute;
            }
            btnCreateEvent.Content = "Edit Event";
        }

        private void SelectCommandFromList(Event sceneEvent)
        {
            bool foundCommand = false;
            foreach (AbstractCommand command in cmbCommands.ItemsSource)
            {
                if (command.id == sceneEvent.command.id)
                {
                    cmbCommands.SelectedItem = command;
                    foundCommand = true;
                    break;
                }
            }
            if (!foundCommand && sceneEvent != null && sceneEvent.command != null)
            {
                List<AbstractCommand> newCommands = new List<AbstractCommand>();
                for (int i = 0; i < commandList.Commands.Count; i++)
                {
                    newCommands.Add(commandList.Commands[i]);
                }
                CommandList newList = new CommandList(newCommands);
                newList.AddItem(sceneEvent.command);
                Console.WriteLine("Add command " + sceneEvent.command);
                commandList = newList;
                this.clFilter = new CommandListFilter(commandList);
                this.clFilter.setFilter(sceneEvent.type);
                this.cmbCommands.ItemsSource = clFilter.getCommands();
                SelectCommandFromList(sceneEvent);
            }
        }

        private Domain.Actions.Action CommandWindowCallback(Domain.Actions.Action command)
        {
            MainWindow.main.DoAction(command);
            cmbCommands.ItemsSource = clFilter.getCommands();
            cmbCommands.SelectedItem = command.GetFocus() as AbstractCommand;
            return command;
        }

        //formats what is needed when create button is clicked
        private void btnCreateCommandClick(object sender, RoutedEventArgs e)
        {
            CreateCommandWindow subWindow;
            if (cmbEventType.Text != null && cmbEventType.Text != "")
            {
                if (cmbCommands.SelectedItem == null)
                {
                    subWindow = new CreateCommandWindow(cmbEventType.Text, CommandWindowCallback);
                }
                else
                {
                    var command = cmbCommands.SelectedItem as AbstractCommand;
                    subWindow = new CreateCommandWindow(command, CommandWindowCallback);
                }
            }
            else
            {
                subWindow = new CreateCommandWindow(CommandWindowCallback);
            }
            subWindow.Owner = this;
            subWindow.ShowDialog();
        }

        private void enterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!validateForm())
                {
                    return;
                }

                if (this.isEditing)
                {
                    editEvent();
                    return;
                }

                if (!validateTimespan())
                {
                    return;
                }

                String type = cmbEventType.Text;
                ulong startTime = getStartTime();

                if (type == Types.AUDIO)
                {
                    if (deviceComboBox.SelectedItem == null)
                    {
                        deviceComboBox.SelectedIndex = 0;
                    }
                    AudioDevice device = (AudioDevice)deviceComboBox.SelectedItem;
                    MusicEvent newEvent = new MusicEvent((MusicCommand)cmbCommands.SelectedItem, startTime, scene, device);

                    device.isHidden = false;
                    MainWindow.main.ShowOutputDevice(device);

                    this.doneFunction(new NewEventAction(newEvent, scene));
                    this.Close();
                    return;

                }
                else if (type == Types.MIDI)
                {
                    if (deviceComboBox.SelectedItem == null)
                    {
                        deviceComboBox.SelectedIndex = 0;
                    }
                    MidiDevice device = (MidiDevice)deviceComboBox.SelectedItem;
                    MidiEvent newEvent = new MidiEvent((MidiCommand)cmbCommands.SelectedItem, startTime, scene, device);

                    device.isHidden = false;
                    MainWindow.main.ShowOutputDevice(device);

                    this.doneFunction(new NewEventAction(newEvent, scene));
                    this.Close();
                    return;

                }
                else if (type == Types.VIDEO)
                {
                    if (deviceComboBox.SelectedItem == null)
                    {
                        deviceComboBox.SelectedIndex = 0;
                    }
                    if ((Boolean)isMute.IsChecked)
                    {
                        (((VideoCommand)cmbCommands.SelectedItem).media as Video).isMute = true;
                    }
                    VideoDevice device = (VideoDevice)deviceComboBox.SelectedItem;
                    VideoEvent newEvent = new VideoEvent((VideoCommand)cmbCommands.SelectedItem, startTime, scene, device);

                    device.isHidden = false;
                    MainWindow.main.ShowOutputDevice(device);

                    this.doneFunction(new NewEventAction(newEvent, scene));
                    this.Close();

                    return;
                }
                return;
            }
        }

        //validates the event type for clicking the create button
        protected Boolean validateEventType(String type)
        {
            if (type != Types.AUDIO && type != Types.VIDEO && type != Types.MIDI)
            {
                this._hasErrors = true;
                this._errorMessage = "Must select an Event type.";
                this.showError();
                return false;
            }
            return true;
        }

        //shows errors to the user
        protected void showError(String errorMessage)
        {
            this._hasErrors = true;
            this._errorMessage = errorMessage;
            this.showError();
        }
        
        //Makes sure the timespan is valid
        protected Boolean validateTimespan()
        {
            String hours = txtHours.Text;
            String minutes = txtMinutes.Text;
            String seconds = txtSeconds.Text;
            String milliseconds = txtMilliseconds.Text;
            int hourInt, minuteInt, secondInt, millisecondInt;
            try
            {
                hourInt = Int32.Parse(hours);
                minuteInt = Int32.Parse(minutes);
                secondInt = Int32.Parse(seconds);
                millisecondInt = Int32.Parse(milliseconds);
            }
            catch (System.FormatException e) {
                this.showError("Not a valid Start/end Time");
                return false;
            }
            if (hours.Contains("-") || minutes.Contains("-") || seconds.Contains("-") || milliseconds.Contains("-") || 
                (hours.Length == 0) || (minutes.Length == 0) || (seconds.Length == 0) || (milliseconds.Length == 0) || 
                !int.TryParse(hours, out int temp) || !int.TryParse(minutes, out temp) || !int.TryParse(hours, out temp) || !int.TryParse(seconds, out temp) || !int.TryParse(milliseconds, out temp) || 
                hourInt > 23 || minuteInt > 59 || secondInt > 59 || millisecondInt > 999)
            {
                this.showError("Not a valid Start/end Time");
                return false;
            }

            return true;
        }

        //unit test for validateTimespan()
        protected Boolean testValidateTimespan()
        {
            //change .Text objects to have invalid input
            String hours = "-";
            String minutes = txtMinutes.Text;
            String seconds = txtSeconds.Text;
            String milliseconds = txtMilliseconds.Text;

            if (hours.Contains("-") || minutes.Contains("-") || seconds.Contains("-") || milliseconds.Contains("-"))
            {
                this.showError("Not a valid Start/end Time");
                return false;
            }

            return true;
        }
        //shows an error
        protected void showError()
        {
            if (this._hasErrors)
            {
                txtBlockErrorMessage.Text = this._errorMessage;
                txtBlockErrorMessage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtBlockErrorMessage.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        //validates the event
        private Boolean validateForm()
        {
            if (cmbEventType.SelectedItem == null)
            {
                return false;
            }

            String type = cmbEventType.Text;

            if (!validateEventType(type))
            {
                return false;
            }

            if (deviceComboBox.SelectedItem == null)
            {
                this.showError("Must have an output device selected.");
                return false;
            }

            if (cmbCommands.SelectedItem == null)
            {
                this.showError("Must select or create a command.");
                return false;
            }
            return true;
        }

        // returns the start time for an event
        private ulong getStartTime()
        {
            String hours = txtHours.Text;
            String minutes = txtMinutes.Text;
            if (minutes.Length < 2) minutes = "0" + minutes;
            String seconds = txtSeconds.Text;
            if (seconds.Length < 2) seconds = "0" + seconds;
            String milliseconds = txtMilliseconds.Text;
            if (milliseconds.Length < 2) milliseconds = "0" + milliseconds;
            if (milliseconds.Length < 3) milliseconds = "0" + milliseconds;
            TimeSpan startTime = TimeSpan.Parse(hours + ":" + minutes + ":" + seconds + "." + milliseconds);
            return (ulong)startTime.TotalMilliseconds;
        }

        //valudate getStartTime() input
        private int testGetStartTime()
        {
            //status is whether or not the test case passed or failed. Passed: 1, Failed: 0
            int status = 0;

            //convert txtbox time fields to ints? NOT SURE IF THIS IS RIGHT
            int numTxtHours = Convert.ToInt32(txtHours.Text);
            numTxtHours = int.Parse(txtHours.Text);
            int numTxtMinutes = Convert.ToInt32(txtMinutes.Text);
            numTxtMinutes = int.Parse(txtMinutes.Text);
            int numTxtSeconds = Convert.ToInt32(txtSeconds.Text);
            numTxtSeconds = int.Parse(txtSeconds.Text);
            int numTxtMilliseconds = Convert.ToInt32(txtMilliseconds.Text);
            numTxtMilliseconds = int.Parse(txtMilliseconds.Text);

            //check if txt fields converted to int are nonnegative
            if (numTxtHours >= 0 && numTxtMinutes >= 0 && numTxtSeconds >= 0 && numTxtMilliseconds >= 0)
            {
                status = 1;
                return status;
            }

            return status;            
        }

        //validates an edited event
        private void editEvent()
        {
            if(!validateTimespan())
            {
                return;
            }
            if (deviceComboBox.SelectedItem == null)
            {
                showError("Must select output Device.");
                return;
            }

            this.doneFunction(new EditEventAction(EventToEdit, cmbCommands.SelectedItem as AbstractCommand, deviceComboBox.SelectedItem as OutputDevice, getStartTime()));
            (deviceComboBox.SelectedItem as OutputDevice).isHidden = false;
            MainWindow.main.ShowOutputDevice(deviceComboBox.SelectedItem as OutputDevice);
            this.Close();
            return;
        }

        //formats what happens when the create event button is clicked
        private void btnCreateEventClick(object sender, RoutedEventArgs e)
        {
            if (!validateForm())
            {
                return;
            }

            if (this.isEditing)
            {
                editEvent();
                return;
            }
            
             if (!validateTimespan())
            {
                return;
            }

            String type = cmbEventType.Text;
            ulong startTime = getStartTime();

            if (type == Types.AUDIO)
            {
                if (deviceComboBox.SelectedItem == null)
                {
                    deviceComboBox.SelectedIndex = 0;
                }
                AudioDevice device = (AudioDevice)deviceComboBox.SelectedItem;
                MusicEvent newEvent = new MusicEvent((MusicCommand)cmbCommands.SelectedItem, startTime, scene, device);
                this.doneFunction(new NewEventAction(newEvent, scene));
                device.isHidden = false;
                MainWindow.main.ShowOutputDevice(device);

                this.Close();
                return;

            }
            else if (type == Types.MIDI)
            {
                if (deviceComboBox.SelectedItem == null)
                {
                    deviceComboBox.SelectedIndex = 0;
                }
                MidiDevice device = (MidiDevice)deviceComboBox.SelectedItem;
                MidiEvent newEvent = new MidiEvent((MidiCommand)cmbCommands.SelectedItem, startTime, scene, device);

                device.isHidden = false;
                MainWindow.main.ShowOutputDevice(device);

                this.doneFunction(new NewEventAction(newEvent, scene));
                this.Close();
                return;

            }
            else if (type == Types.VIDEO)
            {
                if (deviceComboBox.SelectedItem == null)
                {
                    deviceComboBox.SelectedIndex = 0;
                }
                if ((Boolean)isMute.IsChecked)
                {
                    (((VideoCommand)cmbCommands.SelectedItem).media as Video).isMute = true;
                }
                VideoDevice device = (VideoDevice)deviceComboBox.SelectedItem;
                VideoEvent newEvent = new VideoEvent((VideoCommand)cmbCommands.SelectedItem, startTime, scene, device);

                device.isHidden = false;
                MainWindow.main.ShowOutputDevice(device);

                this.doneFunction(new NewEventAction(newEvent, scene));
                this.Close();

                return;
            }
            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void setEditing(bool edit) {
            isEditing = edit;
        }

        //formats what happens when an event type is changed
        private void cmbEventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string type = (string)((ComboBoxItem)cmbEventType.SelectedValue).Content;

            deviceComboBox.ItemsSource = MainWindow.main.outputs.GetDisplayOutputs(type, true);
            deviceComboBox.SelectedIndex = 0;
            clFilter.setFilter(type);
            this.isMute.Visibility = (type.Equals(Types.VIDEO)) ? Visibility.Visible : Visibility.Hidden;
            
            this.cmbCommands.ItemsSource = clFilter.getCommands();
        }
       
        //formats a switch from start time editing to end time editing when the button is pressed. 
        private void btnEndTime(object sender, RoutedEventArgs e)
        {
            endTime = Event.endTime;
            TimeSpan ts = TimeSpan.FromMilliseconds(endTime);

            int hh = ts.Hours;
            int mm = ts.Minutes;
            int ss = ts.Seconds;
            int ms = ts.Milliseconds;
           
            txtHours.Text = hh.ToString();
            txtMinutes.Text = mm.ToString();
            txtSeconds.Text = ss.ToString();
            txtMilliseconds.Text = ms.ToString();
        }

        private void cmbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCommands.SelectedIndex < 0)
            {
                btnCreateCommand.Content = "Create Command";
            }
            else
            {
                btnCreateCommand.Content = "Edit Command";
            }
        }
 
    }
}
