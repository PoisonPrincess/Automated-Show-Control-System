using NAudio.Midi;
using ShowAutomation.Domain.Utils.Midi;

namespace ShowAutomation.UI.Views
{
    internal static class CreateCommandWindow_BehringerX32
    {
        internal static void DeviceProfileComboBox_SelectionChanged_BehringerX32(this CreateCommandWindow window)
        {
            // Save command code index.
            int commandCodeIndex = window.CommandCodeComboBox.SelectedIndex;

            // Set command code values.
            window.CommandCodeComboBox.ItemsSource = BehringerX32MidiValues.CommandCode;
            window.CommandCodeComboBox.SelectedIndex = commandCodeIndex < BehringerX32MidiValues.CommandCode.Length ? commandCodeIndex : 0;

            window.AddButton.IsEnabled = IsValidCommand_BehringerX32(window);

            CommandCodeComboBox_SelectionChanged_BehringerX32(window);
            ChannelComboBox_SelectionChanged_BehringerX32(window);
            DataByte1ComboBox_SelectionChanged_BehringerX32(window);
            DataByte2ComboBox_SelectionChanged_BehringerX32(window);
        }

        internal static void CommandCodeComboBox_SelectionChanged_BehringerX32(this CreateCommandWindow window)
        {
            // Save channel index.
            int channelIndex = window.ChannelComboBox.SelectedIndex;

            // Set channel values and data byte labels.
            switch (window.CommandCodeComboBox.SelectedIndex)
            {
                // Control Change
                case 0:
                    window.ChannelComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.Channel;
                    window.ChannelComboBox.SelectedIndex = channelIndex < BehringerX32MidiValues.ControlChange.Channel.Length ? channelIndex : 0;
                    window.DataByte1GroupBox.Header = "Controller";
                    window.DataByte2GroupBox.Header = "Value";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;

                // Program Change
                case 1:
                    window.ChannelComboBox.ItemsSource = BehringerX32MidiValues.ProgramChange.Channel;
                    window.ChannelComboBox.SelectedIndex = channelIndex < BehringerX32MidiValues.ProgramChange.Channel.Length ? channelIndex : 0;
                    window.DataByte1GroupBox.Header = "Scene";
                    window.DataByte2GroupBox.Header = "Data Byte 2";
                    window.DataByte2GroupBox.IsEnabled = false;
                    window.DataByte2ComboBox.SelectedIndex = -1;
                    break;

                default:
                    window.DataByte1GroupBox.Header = "Data Byte 1";
                    window.DataByte2GroupBox.Header = "Data Byte 2";
                    break;
            }

            window.AddButton.IsEnabled = IsValidCommand_BehringerX32(window);
        }

        internal static void ChannelComboBox_SelectionChanged_BehringerX32(this CreateCommandWindow window)
        {
            // Save data byte 1 index.
            int dataByte1Index = window.DataByte1ComboBox.SelectedIndex;

            // Save data byte 2 index.
            int dataByte2Index = window.DataByte2ComboBox.SelectedIndex;

            // Set data byte values.
            switch (window.CommandCodeComboBox.SelectedIndex)
            {
                // Control Change
                case 0:
                    switch (window.ChannelComboBox.SelectedIndex)
                    {
                        // Fader
                        case 0:
                            window.DataByte1ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte1.Fader;
                            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < BehringerX32MidiValues.ControlChange.DataByte1.Fader.Length ? dataByte1Index : 0;
                            window.DataByte2ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte2.Fader;
                            window.DataByte2ComboBox.SelectedIndex = dataByte2Index < BehringerX32MidiValues.ControlChange.DataByte2.Fader.Length ? dataByte2Index : 0;

                            // Set reference value.
                            //window.MidiDataByte2ComboBox.Items[95] = "95 (0 dB)";
                            break;

                        // Mute
                        case 1:
                            window.DataByte1ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte1.Mute;
                            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < BehringerX32MidiValues.ControlChange.DataByte1.Mute.Length ? dataByte1Index : 0;
                            window.DataByte2ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte2.Mute;
                            window.DataByte2ComboBox.SelectedIndex = dataByte2Index < BehringerX32MidiValues.ControlChange.DataByte2.Mute.Length ? dataByte2Index : 0;
                            break;

                        // Pan
                        case 2:
                            window.DataByte1ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte1.Pan;
                            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < BehringerX32MidiValues.ControlChange.DataByte1.Pan.Length ? dataByte1Index : 0;
                            window.DataByte2ComboBox.ItemsSource = BehringerX32MidiValues.ControlChange.DataByte2.Pan;
                            window.DataByte2ComboBox.SelectedIndex = dataByte2Index < BehringerX32MidiValues.ControlChange.DataByte2.Pan.Length ? dataByte2Index : 0;

                            // Set reference value.
                            //window.MidiDataByte2ComboBox.Items[64] = "64 (Mid)";
                            break;

                        default:
                            break;
                    }
                    break;

                // Program Change
                case 1:
                    switch (window.ChannelComboBox.SelectedIndex)
                    {
                        // Scenes
                        case 0:

                        // Snippets
                        case 1:
                            window.DataByte1ComboBox.ItemsSource = BehringerX32MidiValues.ProgramChange.DataByte1.ScenesAndSnippets;
                            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < BehringerX32MidiValues.ProgramChange.DataByte1.ScenesAndSnippets.Length ? dataByte1Index : 0;
                            break;

                        // Cue Group 1
                        case 2:

                        // Cue Group 2
                        case 3:

                        // Cue Group 3
                        case 4:

                        // Cue Group 4
                        case 5:
                            window.DataByte1ComboBox.ItemsSource = BehringerX32MidiValues.ProgramChange.DataByte1.CueGroups;
                            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < BehringerX32MidiValues.ProgramChange.DataByte1.CueGroups.Length ? dataByte1Index : 0;
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            window.AddButton.IsEnabled = IsValidCommand_BehringerX32(window);
        }

        internal static void DataByte1ComboBox_SelectionChanged_BehringerX32(this CreateCommandWindow window)
        {
            window.AddButton.IsEnabled = IsValidCommand_BehringerX32(window);
        }

        internal static void DataByte2ComboBox_SelectionChanged_BehringerX32(this CreateCommandWindow window)
        {
            window.AddButton.IsEnabled = IsValidCommand_BehringerX32(window);
        }

        internal static bool IsValidCommand_BehringerX32(this CreateCommandWindow window)
        {
            switch (window.CommandCodeComboBox.SelectedIndex)
            {
                // Program Change
                case 1:
                    return window.ChannelComboBox.SelectedIndex >= 0 && window.DataByte1ComboBox.SelectedIndex >= 0 ? true : false;

                default:
                    return window.ChannelComboBox.SelectedIndex >= 0 && window.DataByte1ComboBox.SelectedIndex >= 0 && window.DataByte2ComboBox.SelectedIndex >= 0 ? true : false;
            }
        }

        internal static MidiEvent GetMidiEvent_BehringerX32(this CreateCommandWindow window)
        {
            MidiEvent midiEvent = null;
            long absoluteTime = 0;
            int channel;
            MidiCommandCode commandCode;
            int dataByte1;
            int dataByte2;

            if (window.IsValidCommand_BehringerX32())
            {
                channel = window.ChannelComboBox.SelectedIndex + 1;
                dataByte1 = window.DataByte1ComboBox.SelectedIndex;

                switch (window.CommandCodeComboBox.SelectedIndex)
                {
                    case 0:
                        commandCode = MidiCommandCode.ControlChange;

                        // Mute On
                        if (channel == 2 && window.DataByte2ComboBox.SelectedIndex == 1)
                        {
                            dataByte2 = 127;
                        }

                        else
                        {
                            dataByte2 = window.DataByte2ComboBox.SelectedIndex;
                        }

                        midiEvent = new ControlChangeEvent(absoluteTime, channel, (MidiController)dataByte1, dataByte2);
                        break;

                    case 1:
                        commandCode = MidiCommandCode.PatchChange;
                        midiEvent = new PatchChangeEvent(absoluteTime, channel, dataByte1);
                        break;

                    default:
                        break;
                }
            }
            return midiEvent;
        }
    }
}
