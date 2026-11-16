using System;
using ShowAutomation.Domain.Models.Media;
using System.Runtime.Serialization;
using System.Windows.Media;
using ShowAutomation.Domain.Utils;

/// <summary>
/// The Base for all comands, all types audio video and midi will have acsess to these methods and veriables
/// </summary>
namespace ShowAutomation.Domain.Models.Commands
{
    [Serializable()]
    public abstract class AbstractCommand : ISerializable
    {
        public Guid id { get; set; }

        public String name { get; set; }

        public Color color { get; set; }
        public String listItemTextColor
        {
            get
            {
                return ColorManager.TEXT_COLOR;
            }
            set { }
        }
        public MediaModel media { get; set; }                           

        public String type { get; set; }
        public abstract ulong getDuration();

        public AbstractCommand()
        {
            this.id = Guid.NewGuid();
        }
        /// <summary>
        /// Constructer for a Command that takes in 3 Parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        /// <param name="id"></param>
        public AbstractCommand(String name, MediaModel media, Guid id)
        {
            this.name = name;
            this.id = id;
            this.media = media;
        }
        /// <summary>
        /// Constructer that takes in to parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        public AbstractCommand(String name, MediaModel media)
        {
            this.name = name;
            this.media = media;
            this.id = Guid.NewGuid();
        }

        public AbstractCommand(SerializationInfo info, StreamingContext context)
            : this ((String)info.GetValue("name", typeof(String)), (MediaModel)info.GetValue("media", typeof(MediaModel)), (Guid)info.GetValue("id", typeof(Guid)))
        {
            type = (String)info.GetValue("type", typeof(String));
            try
            {
                color = ((SavedColor)info.GetValue("color", typeof(SavedColor))).getColor();
            }
            catch (SerializationException e)
            {
                color = Colors.White;
            }
        }
        /// <summary>
        /// Function needed to save each command to file
        /// </summary>
        /// <returns></returns>
        abstract public object ToJsonInstance();

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);
            info.AddValue("name", name);
            info.AddValue("type", type);
            info.AddValue("media", media);
            info.AddValue("color", new SavedColor(color));
        }
    }
        /// <summary>
        /// Used to save command information to file
        /// </summary>
    public abstract class JsonCommand
    {
        public String id;
        public String name;
        public String type;
        public JsonCommand (AbstractCommand command)
        {
            this.id = command.id.ToString();
            this.name = command.name;
            this.type = command.type;
        }
    }
}