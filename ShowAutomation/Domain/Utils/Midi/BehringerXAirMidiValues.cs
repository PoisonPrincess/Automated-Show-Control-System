using System.Linq;

namespace ShowAutomation.Domain.Utils.Midi
{
    public class BehringerXAirMidiValues: MidiValues
    {/// <summary>
    /// Behringer Specific values , pre programed 
    /// </summary>
        public static object[] CommandCode { get { return new string[] { "Control Change", "Program Change" }; } }

        public struct ControlChange
        {
            public static string[] Channel { get { return new string[] { "Fader", "Mute", "Pan" }; } }

            public struct DataByte1
            {
                public static string[] Fader { get { return GetFaderValues(); } }
                public static string[] Mute { get { return GetMuteValues(); } }
                public static string[] Pan { get { return GetPanValues(); } }

                private static string[] GetFaderValues()
                {
                    var faderValues = new string[40];

                    for (int i = 0; i < faderValues.Length; ++i)
                    {
                        if (i >= 0 && i <= 15)
                        {
                            faderValues[i] = "Channel " + (i + 1).ToString();
                        }

                        if (i == 16)
                        {
                            faderValues[i] = "Aux In 17/18";
                        }

                        if (i >= 17 && i <= 20)
                        {
                            faderValues[i] = "FX" + (i - 16).ToString() + " Return";
                        }

                        if (i >= 21 && i <= 26)
                        {
                            faderValues[i] = "Aux" + (i - 20).ToString() + " Send";
                        }

                        if (i >= 27 && i <= 30)
                        {
                            faderValues[i] = "FX" + (i - 26).ToString() + " Send";
                        }

                        if (i == 31)
                        {
                            faderValues[i] = "Main LR";
                        }

                        if (i >= 32 && i <= 35)
                        {
                            faderValues[i] = "DCA " + (i - 31).ToString();
                        }

                        if (i >= 36 && i <= 39)
                        {
                            faderValues[i] = "Unused";
                        }
                    }

                    return faderValues;
                }

                private static string[] GetMuteValues()
                {
                    var muteValues = GetFaderValues();

                    for (int i = 36; i <= 39; ++i)
                    {
                        muteValues[i] = "Mute Group " + (i - 35).ToString();
                    }

                    return muteValues;
                }

                private static string[] GetPanValues()
                {
                    var panValues = GetFaderValues();

                    for (int i = 27; i <= 30; ++i)
                    {
                        panValues[i] = "Unused";
                    }

                    for (int i = 32; i <= 35; ++i)
                    {
                        panValues[i] = "Unused";
                    }

                    return panValues;
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
            public static string[] Channel { get { return new string[] { "Snapshots" }; } }
            public static int[] DataByte1 { get { return Enumerable.Range(0, 64).ToArray(); } }
        }
    }
}
