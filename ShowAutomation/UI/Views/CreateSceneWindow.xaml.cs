using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Scenes;
using System;
using System.Windows;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateSceneWindow.xaml
    /// </summary>
    public partial class CreateSceneWindow : Window
    {
        private Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction;
        public bool keyPreview;

        public CreateSceneWindow(Func<Domain.Actions.Action, Domain.Actions.Action> doneFunction)
        {
            this.doneFunction = doneFunction;
            InitializeComponent();
            this.keyPreview = true;
        }

        public void btnCreateScene_Click(object sender, RoutedEventArgs e)
        {
            String sceneName = sceneNameTxt.Text;
            Boolean isAutoPlay = false;
            if ((Boolean)isAutoPlayCheckBox.IsChecked)
            {
                isAutoPlay = true;
            }
            Scene newScene = new Scene(sceneName, isAutoPlay);
            this.doneFunction(new NewSceneAction(newScene, MainWindow.main.sceneList));
            this.Close();
        }
       
        public void keyCreate(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                String sceneName = sceneNameTxt.Text;
                Boolean isAutoPlay = false;
                if ((Boolean)isAutoPlayCheckBox.IsChecked)
                {
                    isAutoPlay = true;
                }
                Scene newScene = new Scene(sceneName, isAutoPlay);
                this.doneFunction(new NewSceneAction(newScene, MainWindow.main.sceneList));
                this.Close();
            }
        }
    }
}
