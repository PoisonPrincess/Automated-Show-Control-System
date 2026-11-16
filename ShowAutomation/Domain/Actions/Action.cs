
namespace ShowAutomation.Domain.Actions
{
    public abstract class Action
    {
        public abstract void Undo();
        public abstract void Perform();
        public abstract string Title();
        public abstract object GetFocus();
    }
}
