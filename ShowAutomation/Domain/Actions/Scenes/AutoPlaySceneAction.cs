using ShowAutomation.Domain.Models.Scenes;

namespace ShowAutomation.Domain.Actions
{
    class AutoPlaySceneAction : Action
    {
        private Scene scene;
        private bool value;

        public AutoPlaySceneAction(Scene scene, bool value)
        {
            this.scene = scene;
            this.value = value;
        }

        public override object GetFocus()
        {
            return scene;
        }

        public override void Perform()
        {
            scene._autoPlay = value;

            //to force redraw of scene list
            MainWindow.main.lvSceneList.DataContext = null;
            MainWindow.main.lvSceneList.DataContext = MainWindow.main.sceneList;
        }

        public override string Title()
        {
            return "Set Scene " +scene.name+ " to" + ((value) ? " " : " not ") + "Autoplay";
        }

        public override void Undo()
        {
            scene._autoPlay = !value;

            //to force redraw of scene list
            MainWindow.main.lvSceneList.DataContext = null;
            MainWindow.main.lvSceneList.DataContext = MainWindow.main.sceneList;
        }
    }
}
