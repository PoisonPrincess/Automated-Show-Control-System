using System;
using System.Globalization;

namespace ShowAutomation.Domain.Utils
{/// <summary>
/// Convets all the get durations from milisecond to readible time
/// </summary>
    class TimelineTimeConverter
    {
        public static ulong ConvertString (String rawTimeString)
        {
            ulong startTime = 0;
            float time = float.Parse(rawTimeString, CultureInfo.InvariantCulture.NumberFormat);
            time = time * 60;
            startTime = (ulong)(time * 1000);
            startTime = (ulong) Math.Ceiling((double)startTime / 250) * 250;
            return startTime;
        }

        public static String convertToTime (ulong startTime)
        {
            return ((double)startTime / 60000).ToString();
        }

        public static String formatTime (ulong time)
        {
            TimeSpan timespan = new TimeSpan(0, 0, 0, 0, (int)time);
            string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
            return timeText;
        }

        public static Boolean isValidTimeString (String rawTimeString)
        {
            try
            {
                float time = float.Parse(rawTimeString, CultureInfo.InvariantCulture.NumberFormat);
                return true;
            }
            catch (SystemException e)
            {
                return false;
            }
        }
    }
}
