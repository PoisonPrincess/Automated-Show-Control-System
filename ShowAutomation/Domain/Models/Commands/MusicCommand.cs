using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils;
using System;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ShowAutomation.Domain.Models.Commands
{/// <summary>
 /// Audio Commands inherit from Abstract commands 
 /// </summary>
    [Serializable()]
    public class MusicCommand : AbstractCommand, ISerializable
    {
        /// <summary>
        /// Constructer that takes 3 perameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        /// <param name="id"></param>
        public MusicCommand(String name, Song media, Guid id) : base(name, media, id)
        {
            type = Types.AUDIO;
            this.color = Colors.Khaki;
        }
        /// <summary>
        /// Constructer that take 2 parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        public MusicCommand(String name, Song media) : base(name, media)
        {
            type = Types.AUDIO;
            this.color = Colors.Khaki;
        }

        public MusicCommand(SerializationInfo info, StreamingContext context) : base(info, context) {
            this.color = Colors.Khaki;
        }
        /// <summary>
        /// Gets Audio Duration 
        /// </summary>
        /// <returns></returns>
        public override ulong getDuration ()
        {
            return this.media.getDuration();
        }

        /// <summary>
        /// Creats a Json instance 
        /// </summary>
        /// <returns></returns>
        public override object ToJsonInstance()
        {
            return new JsonMusicCommand(this);
        }
    }
    /// <summary>
    /// Saves Audio information to Json
    /// </summary>
    public class JsonMusicCommand : JsonCommand
    {
        public String songPath;
        public JsonMusicCommand(MusicCommand command) : base(command)
        {
            Song media = (Song)command.media;
            this.songPath = media.songPath;
        }
    }
}
