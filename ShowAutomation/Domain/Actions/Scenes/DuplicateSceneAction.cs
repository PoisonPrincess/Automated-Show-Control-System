using ShowAutomation.Domain.Models.Scenes;

namespace ShowAutomation.Domain.Actions
{
    class DuplicateSceneAction : Action
    {
        private Scene original;
        private Scene duplucate;

        public DuplicateSceneAction(Scene original)
        {
            this.original = original;
            duplucate = (Scene)original.Clone();
        }

        public override object GetFocus()
        {
            return original;
        }

        public override void Perform()
        {
            MainWindow.main.sceneList.InsertItem(MainWindow.main.sceneList.Scenes.IndexOf(original) + 1, duplucate);
            MainWindow.main.DrawTimeline();
        }

        public override string Title()
        {
            return "Duplicate " + original.name + " Scene";
        }

        public override void Undo()
        {
            MainWindow.main.sceneList.DeleteItem(duplucate);
            MainWindow.main.DrawTimeline();
        }
    }
}
