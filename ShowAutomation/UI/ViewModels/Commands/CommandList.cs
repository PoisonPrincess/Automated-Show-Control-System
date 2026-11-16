using System;
using System.Collections.Generic;
using System.ComponentModel;
using ShowAutomation.Domain.Models.Commands;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ShowAutomation.UI.ViewModels.Commands
{
    /// <summary>
    /// Command List ViewModel interacts with an ObservableCollection of Commands to display within a ListView element
    /// </summary>
    [Serializable]
    public class CommandList : INotifyPropertyChanged, ISerializable
    {
        protected ObservableCollection<AbstractCommand> _list = new ObservableCollection<AbstractCommand>();
        /// <summary>
        /// Public Constructor
        /// </summary>
        /// <param name="commandList">List of commands to initialize CommandList with</param>
       public CommandList(List<AbstractCommand> commandList)
       {
           for (int i = 0; i < commandList.Count; i++)
            {
                this._list.Add(commandList[i]);
            }
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        public CommandList()
        {
        }

        public CommandList(SerializationInfo info, StreamingContext context)
        {
            _list = (ObservableCollection<AbstractCommand>)info.GetValue("commands", typeof(ObservableCollection<AbstractCommand>));
        }

        /// <summary>
        /// Public list of commands UI accesses to display the list
        /// </summary>
        public ObservableCollection<AbstractCommand> Commands
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
                OnPropertyChanged("Commands");
            }
        }

        /// <summary>
        /// Adds a new Command to the List
        /// </summary>
        /// <param name="newCommand"></param>
        public void AddItem(AbstractCommand newCommand)
        {
            _list.Add(newCommand);
            Console.WriteLine("AddItem in CommandList named: " + newCommand.name);
        }

        /// <summary>
        /// Removes a command from the list
        /// </summary>
        /// <param name="midiCommand"></param>
        public void DeleteItem(AbstractCommand midiCommand)
        {
            _list.Remove(midiCommand);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }

        public JsonCommandList ToJsonInstance ()
        {
            return new JsonCommandList(this);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("commands", _list);
        }
    }

    public class JsonCommandList
    {
        public List<object> commands = new List<object>();

        public JsonCommandList (CommandList commandList)
        {

            for (int i = 0; i < commandList.Commands.Count; i++)
            {
                this.commands.Add(commandList.Commands[i].ToJsonInstance());
            }
        }
    }
}
