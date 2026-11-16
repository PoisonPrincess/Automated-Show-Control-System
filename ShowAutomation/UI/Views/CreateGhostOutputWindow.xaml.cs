using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateGhostOutputWindow.xaml
    /// </summary>
    public partial class CreateGhostOutputWindow : Window
    {
        private OutputDevice oldDevice;
        private Func<Domain.Actions.Action, Domain.Actions.Action> callback;
        private bool wasWarned = false;
        private bool keyPreview;
        public CreateGhostOutputWindow(Func<Domain.Actions.Action, Domain.Actions.Action> addGhostCallback)
        {
            InitializeComponent();
            this.keyPreview = true;
            txtName.Text = Types.GHOST + " #" + (GhostOutput.getGhostDevices().Count + 1);
            callback = addGhostCallback;
        }
        public CreateGhostOutputWindow(OutputDevice device, Func<Domain.Actions.Action, Domain.Actions.Action> addGhostCallback) : this(addGhostCallback)
        {
            oldDevice = device;
            matchOldCheckBox.Visibility = Visibility.Visible;
            Grid.SetRow(labelError, 5);
            
        }
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnCreateClick(object sender, RoutedEventArgs e)
        {
            if (CheckInput()) {
                Console.WriteLine("New Ghost " + typeCombo.SelectedValue + " " + txtName.Text);
                String type = (typeCombo.SelectedValue as ComboBoxItem).Content as String;
                String name = txtName.Text;
                OutputDevice device = null;
                if (type.Equals(Types.AUDIO)) {
                    device = new AudioDevice(name);
                } else if (type.Equals(Types.VIDEO))
                {
                    device = new VideoDevice(name);
                }
                else if (type.Equals(Types.MIDI))
                {
                    device = new MidiDevice(name);
                }

                if (device != null)
                {
                    device.ghostType = type;
                    device.type = Types.GHOST;
                    callback(new Domain.Actions.CreateGhostDeviceAction(device));
                    this.Close();
                }
            }
        }

        private void enterKey(object sender, KeyEventArgs e)
        {
            if (CheckInput() && e.Key == Key.Enter)
            {
                Console.WriteLine("New Ghost " + typeCombo.SelectedValue + " " + txtName.Text);
                String type = (typeCombo.SelectedValue as ComboBoxItem).Content as String;
                String name = txtName.Text;
                OutputDevice device = null;
                if (type.Equals(Types.AUDIO))
                {
                    device = new AudioDevice(name);
                }
                else if (type.Equals(Types.VIDEO))
                {
                    device = new VideoDevice(name);
                }
                else if (type.Equals(Types.MIDI))
                {
                    device = new MidiDevice(name);
                }

                if (device != null)
                {
                    device.ghostType = type;
                    device.type = Types.GHOST;
                    callback(new Domain.Actions.CreateGhostDeviceAction(device));
                    this.Close();
                }
            }
        }

        private void MatchNames(object sender, RoutedEventArgs e)
        {
            if (!wasWarned)
            {
                MessageBox.Show(
                            "This automaticly maps events back to the device with this name when it's plugged back in.",
                            "Creating Ghost Output: Match Missing Device Name",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                wasWarned = true;
            }
            txtName.Text = oldDevice.deviceName;
        }

        private void UnmatchNames(object sender, RoutedEventArgs e)
        {
            txtName.Text = Types.GHOST + " #" + (GhostOutput.getGhostDevices().Count + 1);
        }

        private bool CheckInput()
        {
            if (typeCombo.SelectedValue == null)
            {
                labelError.Content = "Must Select a Ghost Type!";
                return false;
            }
            if (txtName.Text == null || txtName.Text.Equals("")) {
                labelError.Content = "Must Enter Vaild Name!";
                return false;
            }
            List<OutputDevice> devices = MainWindow.main.outputs.outputList;
            for (int i = 0; i < devices.Count; i++)
            {
                if (txtName.Text.Equals(devices[i].deviceName))
                {
                    labelError.Content = "Name Already Taken. Must Enter a Unique Name!";
                    return false;
                }
            }
            return true;
        }

        public void SetType(string type)
        {
            typeCombo.Text = type;
            typeCombo.IsEnabled = false;
        }
    }
}
