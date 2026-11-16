using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.OutputDevices;
using ShowAutomation.UI.ViewModels.Scenes;
using System;
using System.Windows;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for ManageOutputsWindow.xaml
    /// </summary>
    public partial class ManageOutputsWindow : Window
    {
        private OutputManager manager;
        private bool previewKey;

        public ManageOutputsWindow(OutputManager manager, SceneList scenes)
        {
            this.manager = manager;
            InitializeComponent();
            this.previewKey = true;
            for (int j = 0; j < manager.outputList.Count; j++) {
                if (manager.outputList[j].type.Equals(Types.GHOST) || (scenes.Scenes.Count < 1))
                {
                    manager.outputList[j].HideEnabled = false;
                }
                else
                {
                    int eventCount = 0;
                    for (int i = 0; i < scenes.Scenes.Count; i++)
                    {
                        eventCount += scenes.Scenes[i].getEventsByOutputDevice(manager.outputList[j]).Count;
                    }
                    manager.outputList[j].HideEnabled = eventCount == 0;
                }
            }
            lvOutputList.ItemsSource = manager.outputList;
            Console.WriteLine("Manager Output: " + manager.outputList[0]);
        }

        private void SaveChanges(object sender, RoutedEventArgs e)
        {
            MainWindow.main.UpdateOutputDevices();
            Close();
        }
        private void enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.main.UpdateOutputDevices();
                Close();
            }
        }

        private void DiscardChanges(object sender, RoutedEventArgs e)
        {
            Close();
        }

      
    }
}
