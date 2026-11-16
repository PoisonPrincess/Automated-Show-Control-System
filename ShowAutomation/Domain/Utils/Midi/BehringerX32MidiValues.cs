using System.Linq;

namespace ShowAutomation.Domain.Utils.Midi
{
    public class BehringerX32MidiValues: MidiValues
    {/// <summary>
    /// X32 Specific values pre programed 
    /// </summary>
        public static object[] CommandCode { get { return new string[] { "Control Change", "Program Change" }; } }

        public struct ControlChange
        {
            public static string[] Channel { get { return new string[] { "Fader", "Mute", "Pan" }; } }

            public class DataByte1
            {
                public static string[] Fader { get { return GetFaderValues(); } }
                public static string[] Mute { get { return GetMuteValues(); } }
                public static string[] Pan { get { return GetPanValues(); } }

                private static string[] GetFaderValues()
                {
                    var faderValues = new string[86];

                    for (int i = 0; i < faderValues.Length; ++i)
                    {
                        if (i >= 0 && i <= 31)
                        {
                            faderValues[i] = "Channel " + (i + 1).ToString();
                        }

                        if (i >= 32 && i <= 37)
                        {
                            faderValues[i] = "Aux In " + (i - 31).ToString();
                        }

                        if (i >= 48 && i <= 63)
                        {
                            faderValues[i] = "Mixbus " + (i - 47).ToString();
                        }

                        if (i >= 64 && i <= 69)
                        {
                            faderValues[i] = "Matrix " + (i - 63).ToString();
                        }

                        if (i >= 72 && i <= 79)
                        {
                            faderValues[i] = "DCA Group " + (i - 71).ToString();
                        }
                    }

                    faderValues[38] = "USB Player L";
                    faderValues[39] = "USB Player R";

                    for (int i = 40; i <= 47; i = i + 2)
                    {
                        faderValues[i] = "FX" + ((i - 38) / 2).ToString() + " L";
                        faderValues[i + 1] = "FX" + ((i - 38) / 2).ToString() + " R";
                    }

                    faderValues[70] = "Master L/R";
                    faderValues[71] = "M/C";

                    for (int i = 80; i <= 85; ++i)
                    {
                        faderValues[i] = "Unused";                    
                    }

                    return faderValues;
                }

                private static string[] GetMuteValues()
                {
                    var muteValues = GetFaderValues();

                    for (int i = 80; i <= 85; ++i)
                    {
                        muteValues[i] = "Mute Group " + (i - 79).ToString();
                    }

                    return muteValues;
                }

                private static string[] GetPanValues()
                {
                    return GetFaderValues();
                }
            }

            public struct DataByte2
            {
                public static int[] Fader { get { return Default.DataByte2; } }
                public static string[] Mute { get { return new string[] { "Off", "On" }; } }
                public static int[] Pan { get { return Default.DataByte2; } }
            }
        }

        public struct ProgramChange
        {
            public static string[] Channel { get { return new string[] { "Scenes", "Snippet", "Cue Group 1", "Cue Group 2", "Cue Group 3", "Cue Group 4" }; } }

            public struct DataByte1
            {
                public static int[] ScenesAndSnippets { get { return Enumerable.Range(0, 100).ToArray(); } }
                public static int[] CueGroups { get { return Default.DataByte1; } }
            }
        }
    }
}
