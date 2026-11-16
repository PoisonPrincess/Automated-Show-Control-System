using ShowAutomation.Domain.Models.Scenes;

namespace ShowAutomation.Domain.Actions
{
    class RenameSceneAction : Action
    {
        private Scene scene;
        private string name;
        private string oldName;

        public RenameSceneAction(Scene scene, string name)
        {
            this.scene = scene;
            this.name = name;
            oldName = scene.name;
        }

        public override object GetFocus()
        {
            return scene;
        }

        public override void Perform()
        {
            scene.name = name;
            MainWindow.main.UpdateSceneTabs(MainWindow.main.sceneList.Scenes);
        }

        public override string Title()
        {
            return "Rename Scene " + oldName + " to " + name;
        }

        public override void Undo()
        {
            scene.name = oldName;
            MainWindow.main.UpdateSceneTabs(MainWindow.main.sceneList.Scenes);
        }
    }
}
