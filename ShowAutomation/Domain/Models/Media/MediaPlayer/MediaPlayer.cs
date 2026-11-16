using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer
{
    public abstract class MediaPlayer
    {
        protected OutputDevice device;
        public Event ev;
        protected System.Timers.Timer timer = new System.Timers.Timer();
        public BlockingCollection<MediaPlayerCommand> queue = new BlockingCollection<MediaPlayerCommand>();
        public Func<MediaPlayer, MediaPlayer> MediaFinishedCallback = null;
        protected bool muted = false;
        protected System.Timers.Timer fadeTimer = new System.Timers.Timer();
        protected double fadeSpeed;

        public MediaPlayer(OutputDevice device) {
            this.device = device;
            Thread t = new Thread(new ThreadStart(Player));
            t.Start();
            fadeTimer.Interval = 150;
            fadeTimer.AutoReset = true;
        }

        private void Player()
        {
            while (true) // make something other then true at some point
            {
                MediaPlayerCommand command = queue.Take();
                parseCommand(command);
            }
        }

        private void parseCommand(MediaPlayerCommand command)
        {
            if (command.type.Equals(MediaPlayerCommand.LOAD_AT))
            {
                LoadAt(command.time);
            }
            else if (command.type.Equals(MediaPlayerCommand.PLAY))
            {
                Play();
            }
            else if (command.type.Equals(MediaPlayerCommand.PAUSE))
            {
                Pause();
            }
            else if (command.type.Equals(MediaPlayerCommand.STOP))
            {
                Stop();
            }
            else if (command.type.Equals(MediaPlayerCommand.FADE))
            {
                Fade(command.time, command.percent);
            }
        }

        internal void SetDevice(OutputDevice device)
        {
            this.device = device;
        }

        abstract protected void Play();
        abstract protected void LoadAt(ulong time);
        abstract protected void Stop();
        abstract protected void Pause();
        abstract protected void Fade(ulong time, float percent);

        protected void MediaEnded() {
            if (MediaFinishedCallback != null)
            {
                MediaFinishedCallback.Invoke(this);
            }
        }

        public virtual void Mute(bool mute)
        {
            muted = mute;
        }
    }
}