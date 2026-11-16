using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.UI.ViewModels.Scenes;

namespace ShowAutomation.Domain.Actions
{
    internal class RearangeSceneAction : Action
    {
        private Scene scene;
        private int amount;
        private SceneList sceneList;

        public RearangeSceneAction(Scene scene, int amount, SceneList sceneList)
        {
            this.scene = scene;
            this.amount = amount;
            this.sceneList = sceneList;
        }

        public override object GetFocus()
        {
            return scene;
        }

        public override void Perform()
        {
            sceneList.moveScene(amount, scene);
            MainWindow.main.UpdateSceneTabs(sceneList.Scenes);
        }

        public override string Title()
        {
            return "Move Scene " + scene.name;
        }

        public override void Undo()
        {
            sceneList.moveScene(-amount, scene);
            MainWindow.main.UpdateSceneTabs(sceneList.Scenes);
        }
    }
}