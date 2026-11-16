using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.OutputDevices;
using System.Windows;
using System.Windows.Controls;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for PreviewDevicesWindow.xaml
    /// </summary>
    public partial class PreviewDevicesWindow : Window
    {
        private OutputManager outputs;
        public PreviewDevicesWindow(OutputManager outputs)
        {
            this.outputs = outputs;
            InitializeComponent();
            audioDeviceComboBox.ItemsSource = outputs.getOutputs(Types.AUDIO, false);
            midiDeviceComboBox.ItemsSource = outputs.getOutputs(Types.MIDI, false);
            videoDeviceComboBox.ItemsSource = outputs.getOutputs(Types.VIDEO, false);
            audioDeviceComboBox.SelectedItem = (AudioDevice)outputs.PreviewDevices[Types.AUDIO];
            midiDeviceComboBox.SelectedItem = (MidiDevice)outputs.PreviewDevices[Types.MIDI];
            videoDeviceComboBox.SelectedItem = (VideoDevice)outputs.PreviewDevices[Types.VIDEO];
            isDocked.IsChecked = outputs.PreviewVideoOnPanel;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.DoAction(new UpdatePreviewDevicesAction(audioDeviceComboBox.SelectedItem as AudioDevice, midiDeviceComboBox.SelectedItem as MidiDevice, videoDeviceComboBox.SelectedItem as VideoDevice, (bool)isDocked.IsChecked,  outputs));
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void VideoDocked(object sender, RoutedEventArgs e) {
            videoDeviceComboBox.IsEnabled = !(bool)(sender as CheckBox).IsChecked;
            if ((bool)(sender as CheckBox).IsChecked)
            {
                videoDeviceComboBox.SelectedItem = null;
            }
        }

    }
}
