using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.UI.ViewModels.Scenes;

namespace ShowAutomation.Domain.Actions
{
    internal class NewSceneAction : Action
    {
        private Scene newScene;
        private SceneList list;

        public NewSceneAction(Scene newScene, SceneList list)
        {
            this.newScene = newScene;
            this.list = list;
        }

        public override object GetFocus()
        {
            return newScene;
        }

        public override void Perform()
        {
            list.AddItem(newScene);
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
           return "Add Scene " + newScene.name;
        }

        public override void Undo()
        {
            list.DeleteItem(newScene);
            MainWindow.main.DrawTimeline();
        }
    }
}