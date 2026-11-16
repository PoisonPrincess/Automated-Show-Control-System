using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Commands;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for SceneFadeSettingsWindow.xaml
    /// </summary>
    public partial class SceneFadeSettingsWindow : Window
    {
        private SceneFadeSettings settings;

        public SceneFadeSettingsWindow(SceneFadeSettings settings, CommandList commandList)
        {
            InitializeComponent();
            this.settings = settings;
            CommandListFilter clFilter = new CommandListFilter(commandList);
            clFilter.setFilter(Types.MIDI);
            txtHours.Text = ((settings.Duration / (1000 * 60 * 60)) % 24).ToString();
            txtMinutes.Text = ((settings.Duration / (1000 * 60)) % 60).ToString();
            txtSeconds.Text = ((settings.Duration / 1000) % 60).ToString();
            txtMilliseconds.Text = (settings.Duration % 1000).ToString();
            cmbStartCommands.ItemsSource = clFilter.getCommands();
            cmbEndCommands.ItemsSource = clFilter.getCommands();
            startDeviceComboBox.ItemsSource = MainWindow.main.outputs.getOutputs(Types.MIDI, true);
            startDeviceComboBox.SelectedIndex = 0;
            endDeviceComboBox.ItemsSource = MainWindow.main.outputs.getOutputs(Types.MIDI, true);
            endDeviceComboBox.SelectedIndex = 0;

            if (settings.StartMidiEvent != null)
            {
                foreach (AbstractCommand command in cmbStartCommands.ItemsSource)
                {
                    if (command.id == settings.StartMidiEvent.command.id)
                    {
                        cmbStartCommands.SelectedItem = command;

                    }
                }
                if (settings.StartMidiEvent.OutputDevice != null)
                {
                    startDeviceComboBox.SelectedIndex = settings.StartMidiEvent.OutputDevice.id;
                }
            }

            if (settings.EndMidiEvent != null)
            {
                foreach (AbstractCommand command in cmbEndCommands.ItemsSource)
                {
                    if (command.id == settings.EndMidiEvent.command.id)
                    {
                        cmbEndCommands.SelectedItem = command;

                    }
                }
                if (settings.EndMidiEvent.OutputDevice != null)
                {
                    endDeviceComboBox.SelectedIndex = settings.EndMidiEvent.OutputDevice.id;
                }
            }
        }

        // returns the start time for an event
        private ulong getDuration()
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
            catch (System.FormatException e)
            {
                Error.Text = "Not a valid duration";
                return false;
            }
            if (hours.Contains("-") || minutes.Contains("-") || seconds.Contains("-") || milliseconds.Contains("-") ||
                (hours.Length == 0) || (minutes.Length == 0) || (seconds.Length == 0) || (milliseconds.Length == 0) ||
                !int.TryParse(hours, out int temp) || !int.TryParse(minutes, out temp) || !int.TryParse(hours, out temp) || !int.TryParse(seconds, out temp) || !int.TryParse(milliseconds, out temp) ||
                hourInt > 23 || minuteInt > 59 || secondInt > 59 || millisecondInt > 999)
            {
                Error.Text = "Not a valid duration";
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            if (!validateTimespan())
            {
                return;
            }
            MainWindow.main.DoAction(new UpdateSceneFadeSettingsAction(settings, getDuration(), cmbStartCommands.SelectedItem as MidiCommand, startDeviceComboBox.SelectedItem as MidiDevice, cmbEndCommands.SelectedItem as MidiCommand, endDeviceComboBox.SelectedItem as MidiDevice));
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }


        private void cmbStartCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStartCommands.SelectedIndex < 0)
            {
                StartDevice.Visibility = Visibility.Collapsed;
            }
            else {
                StartDevice.Visibility = Visibility.Visible;
            }
        }

        private void cmbEndCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEndCommands.SelectedIndex < 0)
            {
                EndDevice.Visibility = Visibility.Collapsed;
            }
            else
            {
                EndDevice.Visibility = Visibility.Visible;
            }
        }
    }
}
