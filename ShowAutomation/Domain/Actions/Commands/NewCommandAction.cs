using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.UI.ViewModels.Commands;

namespace ShowAutomation.Domain.Actions
{
    internal class NewCommandAction : Action
    {
        private AbstractCommand command;
        private CommandList commandList;

        public NewCommandAction(AbstractCommand command, CommandList commandList)
        {
            this.command = command;
            this.commandList = commandList;
        }

        public override object GetFocus()
        {
            return command;
        }

        public override void Perform()
        {
            if (!commandList.Commands.Contains(command))
            {
                commandList.AddItem(command);
            }
        }

        public override string Title()
        {
            return "Add " + command.type + " Command " + command.name;
        }

        public override void Undo()
        {
            if (commandList.Commands.Contains(command))
            {
                commandList.DeleteItem(command);
            }
        }
    }
}