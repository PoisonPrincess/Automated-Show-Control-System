using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.UI.ViewModels.Scenes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShowAutomation.UI.Views
{
    public partial class MapToNewDeviceWindow : Window
    {
        private OutputDevice newDevice;
        private SceneList sceneList;
        private bool PreviewKey;

        public MapToNewDeviceWindow(OutputDevice device, SceneList sceneList)
        {
            InitializeComponent();
            this.PreviewKey = true;
            assignBtn.IsEnabled = false;
            this.sceneList = sceneList;
            newDevice = device;
            textBox.Text = device.deviceName;

            List<OutputDevice> ghostDevices = GhostOutput.getGhostDevices().ToList();
            for (int i = 0; i < ghostDevices.Count; i++)
            {
                if (ghostDevices[i].ghostType.Equals(device.type))
                {
                    devicesToMapListView.Items.Add(ghostDevices[i]);
                    if (ghostDevices[i].deviceName.Equals(device.deviceName)) {
                        devicesToMapListView.SelectedValue = ghostDevices[i];
                        AssignBtn_Click(assignBtn, new RoutedEventArgs());
                        devicesToMapListView.Items.Clear();
                        break;
                    }
                }
            }
            if (devicesToMapListView.Items.Count == 0) {
                Close();
            }
        }

        private void AssignBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Event> events = new List<Event>();
            foreach (OutputDevice device in devicesToMapListView.SelectedItems)
            {
                for (int i = 0; i < sceneList.Scenes.Count; i++)
                {
                    events.AddRange(sceneList.Scenes[i].getEventsByOutputDevice(device));
                }
                GhostOutput.removeDevice(device);
            }
            MainWindow.main.DoAction(new MapEventsToNewDeviceAction(events, devicesToMapListView.SelectedItems[0] as OutputDevice, newDevice));
            Close();
        }

        //Closes and opens new show
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void devicesToMapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            assignBtn.IsEnabled = true;
        }
    }
}
