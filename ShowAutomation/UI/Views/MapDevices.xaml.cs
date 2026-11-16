using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.Domain.Utils.Midi;
using ShowAutomation.UI.ViewModels.Scenes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for MapDevices.xaml
    /// </summary>
    public partial class MapDevicesWindow : Window
    {
        List<OutputDevice> MidiOutdevices = new List<OutputDevice>();
        LinkedList<Event> AllEvents = new LinkedList<Event>();
        SceneList sceneList;
        public bool allOutputs = true;
        bool onOpen;
        public bool PreviewKey;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneList"></param>
        /// <param name="doneFunction"></param>
        public MapDevicesWindow(SceneList sceneList) : this(true, sceneList)
        {      

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="D"></param>
        /// <param name="sceneList"></param>
        /// <param name="doneFunction"></param>
        public MapDevicesWindow(bool onOpen, SceneList sceneList)
        {
            InitializeComponent();
            this.PreviewKey = true;
            assignBtn.IsEnabled = false;
            this.sceneList = sceneList;
            this.AllEvents = sceneList.getAllEventsLinkedList();
            this.onOpen = onOpen;
        }
        /// <summary>
        /// 1 updates list in each scene
        /// 2 exits map device widow and displays main window
        /// 3 if a device that is not conected is still assigned to an event , the type is evaluated and options for new devices are given 
        /// </summary>
        public void mapDevices()
        {
            // 1
            if (AllEvents.Count <= 0)
            {
                SceneList sceneList = MainWindow.main.sceneList;
             
                for (int z = 0; z < sceneList.Scenes.Count; z++)
                {
                    if (sceneList.Scenes[z].Events.Count > 0)
                    {
                        Event temp = sceneList.Scenes[z].Events[0];
                        sceneList.Scenes[z].removeEvent(sceneList.Scenes[z].Events[0]);
                        sceneList.Scenes[z].addEvent(temp);
                    }
                }
            // 2
                while (1 == 1)
                {
                    if (this != null)
                    {
                        Console.WriteLine("Nothing to map");
                        Close();
                        MainWindow.main.Show();
                        break;
                    }
                }
            }
            // 3
            else
            {
                mapDevice(AllEvents.First.Value.OutputDevice);
            }
        }

        public void mapDevice(OutputDevice device) {
            Console.WriteLine("Map device: " + device.deviceName);
            int eventCount = 0;
            if (sceneList != null && sceneList.currentScenePlaying >= 0 && sceneList.currentScenePlaying < sceneList.Scenes.Count)
            {
                for (int i = 0; i < sceneList.Scenes.Count; i++)
                {
                    eventCount += sceneList.Scenes[i].getEventsByOutputDevice(device).Count;
                }
            }
            if (eventCount > 0)
            {
                textBox.Text = device.deviceName;
                SetMapToListFromDeviceType(device);

                MainWindow.main.Hide();
                assignBtn.IsEnabled = false;
                Show();
            }
        }

        private void SetMapToListFromDeviceType(OutputDevice device) {
            string type = (!device.type.Equals(Types.GHOST)) ? device.type : device.ghostType;
            List<OutputDevice> devices = MainWindow.main.outputs.getOutputs(type, false);
            if (devices.Contains(device)) {
                devices.Remove(device);
            }
            devicesToMapListView.ItemsSource = devices;
        }

        /// <summary>
        /// Assigns default devices by type is clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ghostPortBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = AllEvents.Count;
            for (int l = 0; l < count; l++)
            {
                if (AllEvents.First.Value.OutputDevice.deviceName.Equals(textBox.Text))
                {
                    if (!AllEvents.First.Value.OutputDevice.type.Equals(Types.GHOST))
                    {
                        CreateGhostOutputWindow createGhostOutputWindow = new CreateGhostOutputWindow(AllEvents.First.Value.OutputDevice, AddGhostDeviceCallback);
                        createGhostOutputWindow.SetType(AllEvents.First.Value.OutputDevice.type);
                        createGhostOutputWindow.Owner = this;
                        createGhostOutputWindow.ShowDialog();
                        return;
                    }
                    else
                    {
                        OutputDevice device = AllEvents.First.Value.OutputDevice;
                        if (!MainWindow.main.outputs.outputList.Contains(device))
                        {
                            new CreateGhostDeviceAction(device).Perform();
                        }
                    }
                    AllEvents.RemoveFirst();
                }
                else
                {
                    Event temp;
                    temp = AllEvents.First();
                    AllEvents.RemoveFirst();
                    AllEvents.AddLast(temp);
                }
            }
            assignBtn.IsEnabled = false;
            if (!allOutputs)
            {
                AllEvents = new LinkedList<Event>();
            }
            Hide();
            mapDevices();
        }

        private Domain.Actions.Action AddGhostDeviceCallback(Domain.Actions.Action action)
        {
            MainWindow.main.DoAction(action);
            OutputDevice device = AllEvents.First.Value.OutputDevice; //MainWindow.main.outputs.getOutputByName(textBox.Text);
            MainWindow.main.DoAction(CreateMapAction(device, action.GetFocus() as OutputDevice));

            assignBtn.IsEnabled = false;
            if (!allOutputs)
            {
                AllEvents = new LinkedList<Event>();
            }
            Hide();
            mapDevices();
            return action;
        }

        /// <summary>
        /// 1 Loops through all events and changes all the output devices of a given name to the one selected that is conected 
        ///2  if a device is evaluated that is not of given name it is removed fom temp list and added to the back of list 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assignBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Assign device: " + textBox.Text);
            // 1

            OutputDevice device = MainWindow.main.outputs.getOutputByName(textBox.Text);
            if (device == null)
            {
                int count = AllEvents.Count;
                for (int l = 0; l < count; l++) //System.NullReferenceException: 'Object reference not set to an instance of an object.' System.Collections.Generic.LinkedList<T>.First.get returned null.
                {
                    if (AllEvents != null && AllEvents.First != null)
                    {
                        if (AllEvents.First.Value.OutputDevice.deviceName.Equals(textBox.Text))
                        {
                            device = AllEvents.First.Value.OutputDevice;
                            CreateMapAction(device).Perform();
                        }
                    }
                }
            }
            else {
                MainWindow.main.DoAction(CreateMapAction(device));
            }

            if (MainWindow.main.outputs.getOutputByName(textBox.Text) != null && MainWindow.main.outputs.getOutputByName(textBox.Text).type.Equals(Types.GHOST))
            {
                MainWindow.main.DoAction(new RemoveGhostDeviceAction(MainWindow.main.outputs.getOutputByName(textBox.Text)));
            }
            assignBtn.IsEnabled = true;
            if (!allOutputs)
            {
                AllEvents = new LinkedList<Event>();
            }
            Hide();
            mapDevices();
        }

        private MapEventsToNewDeviceAction CreateMapAction(OutputDevice device)
        {
            return CreateMapAction(device, devicesToMapListView.SelectedItems[0] as OutputDevice);
        }

        private MapEventsToNewDeviceAction CreateMapAction(OutputDevice device, OutputDevice newDevice)
        {
            List<Event> events = new List<Event>();
            for (int i = 0; i < sceneList.Scenes.Count; i++)
            {
                ObservableCollection<Event> sceneEvents = sceneList.Scenes[i].getEventsByOutputDevice(device);
                events.AddRange(sceneEvents);
                foreach (Event e in sceneEvents)
                {
                    AllEvents.Remove(e);
                }
            }
            return new MapEventsToNewDeviceAction(events, device, newDevice);
        }

        //Closes and opens new show
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (onOpen)
            {
                foreach (MidiDevice device in MidiDeviceManager.Instance.getMidiDevices(false, false))
                {
                    Console.WriteLine("Dispose of " + device.deviceName);
                    if (device.device != null)
                    {
                        device.device.Dispose();
                    }
                }
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            }else
            {
                sceneList = MainWindow.main.sceneList;
                Close();
                MainWindow.main.Show();
            }
        }

        private void devicesToMapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            assignBtn.IsEnabled = true;
        }
    }
}
