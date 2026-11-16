using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.UI.ViewModels.Scenes;

namespace ShowAutomation.Domain.Actions
{
    internal class DeleteSceneAction : Action
    {
        private Scene scene;
        private SceneList list;

        public DeleteSceneAction(Scene newScene, SceneList list)
        {
            this.scene = newScene;
            this.list = list;
        }

        public override object GetFocus()
        {
            return scene;
        }

        public override void Perform()
        {
            list.DeleteItem(scene);
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
           return "Delete Scene " + scene.name;
        }

        public override void Undo()
        {
            list.AddItem(scene);
            MainWindow.main.DrawTimeline();
        }
    }
}