using NAudio.Midi;
using ShowAutomation.Domain.Utils.Midi;

namespace ShowAutomation.UI.Views
{
    internal static class CreateCommandWindow_Midi
    {
        internal static void DeviceProfileComboBox_SelectionChanged_Midi(this CreateCommandWindow window)
        {
            // Save command code index.
            int commandCodeIndex = window.CommandCodeComboBox.SelectedIndex;

            // Set command code values.
            window.CommandCodeComboBox.ItemsSource = MidiValues.Default.CommandCode;
            window.CommandCodeComboBox.SelectedIndex = commandCodeIndex < MidiValues.Default.CommandCode.Length ? commandCodeIndex : 0;

            window.AddButton.IsEnabled = IsValidCommand_Midi(window);

            CommandCodeComboBox_SelectionChanged_Midi(window);
            ChannelComboBox_SelectionChanged_Midi(window);
            DataByte1ComboBox_SelectionChanged_Midi(window);
            DataByte2ComboBox_SelectionChanged_Midi(window);
        }

        internal static void CommandCodeComboBox_SelectionChanged_Midi(this CreateCommandWindow window)
        {
            // Save channel index.
            int channelIndex = window.ChannelComboBox.SelectedIndex;

            // Set channel values.
            window.ChannelComboBox.ItemsSource = MidiValues.Default.Channel;
            window.ChannelComboBox.SelectedIndex = channelIndex < MidiValues.Default.Channel.Length ? channelIndex : 0;

            // Set data byte labels.
            switch (window.CommandCodeComboBox.SelectedIndex)
            {
                // Note Off
                case 0:

                // Note On
                case 1:
                    window.DataByte1GroupBox.Header = "Note";
                    window.DataByte2GroupBox.Header = "Velocity";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;

                // Polyphonic Key Pressure
                case 2:
                    window.DataByte1GroupBox.Header = "Note";
                    window.DataByte2GroupBox.Header = "Pressure";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;

                // Control Change
                case 3:
                    window.DataByte1GroupBox.Header = "Controller";
                    window.DataByte2GroupBox.Header = "Value";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;

                // Program Change
                case 4:
                    window.DataByte1GroupBox.Header = "Program #";
                    window.DataByte2GroupBox.Header = "Data Byte 2";
                    window.DataByte2GroupBox.IsEnabled = false;
                    window.DataByte2ComboBox.SelectedIndex = -1;
                    break;

                // Channel Pressure
                case 5:
                    window.DataByte1GroupBox.Header = "Pressure";
                    window.DataByte2GroupBox.Header = "Data Byte 2";
                    window.DataByte2GroupBox.IsEnabled = false;
                    window.DataByte2ComboBox.SelectedIndex = -1;
                    break;

                // Pitch Bend Change
                case 6:
                    window.DataByte1GroupBox.Header = "Sensitivity LSB";
                    window.DataByte2GroupBox.Header = "Sensitivity MSB";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;

                // Sysex
                case 7:

                default:
                    window.DataByte1GroupBox.Header = "Data Byte 1";
                    window.DataByte2GroupBox.Header = "Data Byte 2";
                    window.DataByte2GroupBox.IsEnabled = true;
                    break;
            }

            window.AddButton.IsEnabled = IsValidCommand_Midi(window);
        }

        internal static void ChannelComboBox_SelectionChanged_Midi(this CreateCommandWindow window)
        {
            // Save data byte 1 index.
            int dataByte1Index = window.DataByte1ComboBox.SelectedIndex;

            // Save data byte 2 index.
            int dataByte2Index = window.DataByte2ComboBox.SelectedIndex;

            // Set data byte values.
            window.DataByte1ComboBox.ItemsSource = MidiValues.Default.DataByte1;
            window.DataByte1ComboBox.SelectedIndex = dataByte1Index < MidiValues.Default.DataByte1.Length ? dataByte1Index : 0;
            window.DataByte2ComboBox.ItemsSource = MidiValues.Default.DataByte2;
            window.DataByte2ComboBox.SelectedIndex = dataByte2Index < MidiValues.Default.DataByte2.Length ? dataByte2Index : 0;

            window.AddButton.IsEnabled = IsValidCommand_Midi(window);
        }

        internal static void DataByte1ComboBox_SelectionChanged_Midi(this CreateCommandWindow window)
        {
            window.AddButton.IsEnabled = IsValidCommand_Midi(window);
        }

        internal static void DataByte2ComboBox_SelectionChanged_Midi(this CreateCommandWindow window)
        {
            window.AddButton.IsEnabled = IsValidCommand_Midi(window);
        }

        internal static bool IsValidCommand_Midi(this CreateCommandWindow window)
        {
            switch (window.CommandCodeComboBox.SelectedIndex)
            {
                // Program Change
                case 4:

                // Channel Pressure
                case 5:
                    return window.ChannelComboBox.SelectedIndex >= 0 && window.DataByte1ComboBox.SelectedIndex >= 0 ? true : false;

                default:
                    return window.ChannelComboBox.SelectedIndex >= 0 && window.DataByte1ComboBox.SelectedIndex >= 0 && window.DataByte2ComboBox.SelectedIndex >= 0 ? true : false;
            }
        }

        internal static MidiEvent GetMidiEvent_Midi(this CreateCommandWindow window)
        {
            MidiEvent midiEvent = null;
            long absoluteTime = 0;
            int channel;
            MidiCommandCode commandCode;
            int dataByte1;
            int dataByte2;

            if (window.IsValidCommand_Midi())
            {
                int.TryParse(window.ChannelComboBox.SelectedItem.ToString(), out channel);
                int.TryParse(window.DataByte1ComboBox.SelectedItem.ToString(), out dataByte1);
                //int.TryParse(MidiDataByte2ComboBox.SelectedItem.ToString(), out dataByte2);

                switch (window.CommandCodeComboBox.SelectedIndex)
                {
                    case 0:
                        commandCode = MidiCommandCode.NoteOff;
                        int.TryParse(window.DataByte2ComboBox.SelectedItem.ToString(), out dataByte2);
                        midiEvent = new NoteEvent(absoluteTime, channel, commandCode, dataByte1, dataByte2);
                        break;
                    case 1:
                        commandCode = MidiCommandCode.NoteOn;
                        int.TryParse(window.DataByte2ComboBox.SelectedItem.ToString(), out dataByte2);
                        midiEvent = new NoteEvent(absoluteTime, channel, commandCode, dataByte1, dataByte2);
                        break;
                    case 2:
                        commandCode = MidiCommandCode.KeyAfterTouch;
                        int.TryParse(window.DataByte2ComboBox.SelectedItem.ToString(), out dataByte2);
                        midiEvent = new NoteEvent(absoluteTime, channel, commandCode, dataByte1, dataByte2);
                        break;
                    case 3:
                        commandCode = MidiCommandCode.ControlChange;
                        int.TryParse(window.DataByte2ComboBox.SelectedItem.ToString(), out dataByte2);
                        midiEvent = new ControlChangeEvent(absoluteTime, channel, (MidiController)dataByte1, dataByte2);
                        break;
                    case 4:
                        commandCode = MidiCommandCode.PatchChange;
                        midiEvent = new PatchChangeEvent(absoluteTime, channel, dataByte1);
                        break;
                    case 5:
                        commandCode = MidiCommandCode.ChannelAfterTouch;
                        midiEvent = new ChannelAfterTouchEvent(absoluteTime, channel, dataByte1);
                        break;
                    case 6:
                        commandCode = MidiCommandCode.PitchWheelChange;
                        midiEvent = new PitchWheelChangeEvent(absoluteTime, channel, dataByte1);
                        break;
                    case 7:
                        commandCode = MidiCommandCode.Sysex;
                        midiEvent = new SysexEvent();
                        break;
                    default:
                        break;
                }
            }

            return midiEvent;
        }
    }
}
