using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.UI.ViewModels.Commands;

namespace ShowAutomation.Domain.Actions
{
    internal class DeleteCommandAction : Action
    {
        private AbstractCommand command;
        private CommandList commandList;

        public DeleteCommandAction(AbstractCommand abstractCommand, CommandList commandList)
        {
            this.command = abstractCommand;
            this.commandList = commandList;
        }

        public override object GetFocus()
        {
            return command;
        }

        public override void Perform()
        {
            if (commandList.Commands.Contains(command))
            {
                commandList.DeleteItem(command);
            }
        }

        public override string Title()
        {
            return "Remove " + command.type + " Command " + command.name;
        }

        public override void Undo()
        {
            if (!commandList.Commands.Contains(command))
            {
                commandList.AddItem(command);
            }
        }
    }
}