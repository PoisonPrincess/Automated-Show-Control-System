using System.Linq;

namespace ShowAutomation.Domain.Utils.Midi
{
    public class MidiValues
    {
        public struct Default
        {
            public static object[] CommandCode { get { return new string[] { "Note Off", "Note On", "Polyphonic Key Pressure", "Control Change", "Program Change", "Channel Pressure", "Pitch Bend Change", "Sysex" }; } }
            public static int[] Channel { get { return Enumerable.Range(1, 16).ToArray(); } }
            public static int[] DataByte1 { get { return Enumerable.Range(0, 128).ToArray(); } }
            public static int[] DataByte2 { get { return Enumerable.Range(0, 128).ToArray(); } }
        }
    }
}
