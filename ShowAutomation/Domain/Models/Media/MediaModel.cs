using System;
using System.Runtime.Serialization;

namespace ShowAutomation.Domain.Models.Media
{
    [Serializable()]
    abstract public class MediaModel : ISerializable
    {
        public Boolean isPlaying = false;

        public MediaModel()
        {

        }
        abstract public ulong getDuration();

        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
