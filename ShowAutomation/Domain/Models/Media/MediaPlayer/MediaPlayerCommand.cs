using System;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer
{
    public class MediaPlayerCommand
    {
        public const String PLAY = "play";
        public const String LOAD_AT = "load";
        public const String STOP = "stop";
        public const String PAUSE = "pause";
        public const String FADE = "fade";

        public String type;
        public ulong time;
        public float percent;

        public MediaPlayerCommand(String type, ulong time, float percent)
        {
            this.type = type;
            this.time = time;
            this.percent = percent;
        }

        public MediaPlayerCommand(String type, ulong time) : this(type, time, 0.0f) { }

        public MediaPlayerCommand(String type) : this(type, 0) { }
    }
}