using ShowAutomation.Domain.Models.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ShowAutomation.UI.ViewModels.Commands
{
    public class CommandListFilter : INotifyPropertyChanged
    {
        private CommandList commandList;
        public String filter;
        /// <summary>
        /// This sorts the comands that are used in the show by type , for creating and for filtering 
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<AbstractCommand> getCommands ()
        {
            ObservableCollection<AbstractCommand> commands = new ObservableCollection<AbstractCommand>();
            for (int i = 0; i < commandList.Commands.Count; i++)
            {
                if (!this.isMatch(commandList.Commands[i]))
                {
                    commands.Add(commandList.Commands[i]);
                }
            }
            return commands;
        }

        public ObservableCollection<AbstractCommand> Commands
        {
            get
            {
               return getCommands();
            }
        }

        public void setFilter(String type)
        {
            filter = type;
            OnPropertyChanged("Commands");
        }

        public CommandListFilter (CommandList commandList)
        {
            this.commandList = commandList;
        }

        public Boolean isMatch (AbstractCommand command)
        {
            if (filter == null)
            {
                return true;
            }
            if (command.type == filter)
            {
                return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
