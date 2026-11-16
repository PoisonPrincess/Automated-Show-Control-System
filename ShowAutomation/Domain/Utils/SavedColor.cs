using System;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ShowAutomation.Domain.Utils
{
    [Serializable()]
    class SavedColor : ISerializable
    {
        private Color color;
        public SavedColor(Color color)
        {
            this.color = color;
        }

        public SavedColor(SerializationInfo info, StreamingContext context)
        {
            byte a = (byte)info.GetValue("a", typeof(byte));
            byte r = (byte)info.GetValue("r", typeof(byte));
            byte b = (byte)info.GetValue("b", typeof(byte));
            byte g = (byte)info.GetValue("g", typeof(byte));

            color = Color.FromArgb(a, r, g, b);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("a", color.A);
            info.AddValue("r", color.R);
            info.AddValue("b", color.B);
            info.AddValue("g", color.G);
        }

        internal Color getColor()
        {
            return color;
        }
    }
}
