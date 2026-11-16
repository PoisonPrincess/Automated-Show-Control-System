using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils;
using System;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ShowAutomation.Domain.Models.Commands
{
    [Serializable()]
    public class VideoCommand : AbstractCommand, ISerializable
    {
        /// <summary>
        /// Constructor that takes 3 parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        /// <param name="id"></param>
        public VideoCommand(String name, Video media, Guid id) : base(name, media, id)
        {
            type = Types.VIDEO;
            this.color = Colors.SeaGreen;
        }
        /// <summary>
        /// Constructer that takes 2 parameters 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="media"></param>
        public VideoCommand(String name, Video media) : base(name, media)
        {
            type = Types.VIDEO;
            this.color = Colors.SeaGreen;
        }

        public VideoCommand(SerializationInfo info, StreamingContext context) : base(info, context) {
            this.color = Colors.SeaGreen;
        }
        /// <summary>
        /// Creates Json instance 
        /// </summary>
        /// <returns></returns>
        public override object ToJsonInstance()
        {
            return new JsonVideoCommand(this);
        }
        /// <summary>
        /// Gets Video Duration 
        /// </summary>
        /// <returns></returns>
        public override ulong getDuration ()
        {
            return this.media.getDuration();
        }
    }
    /// <summary>
    /// Save video information to Json 
    /// </summary>
    public class JsonVideoCommand : JsonCommand
    {
        public String videoPath;
        public JsonVideoCommand(VideoCommand command) : base(command)
        {
            Video media = (Video)command.media;
            this.videoPath = media.videoPath;
        }
    }
}
