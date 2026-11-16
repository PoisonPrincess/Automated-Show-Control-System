using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ShowAutomation.Domain.Models.Events;
using System;
using System.IO;
using System.Timers;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer
{
    public class AudioPlayer : MediaPlayer
    {
        private string songPath;
        protected WaveOut waveOut = null;

        public AudioPlayer(MusicEvent @event) : base(@event.OutputDevice)
        {
            this.songPath = (@event.command.media as Song).songPath;
            ev = @event;
            timer.Elapsed += FinishedPlaying;
            fadeTimer.Elapsed += Fade;
        }

        private void FinishedPlaying(object sender, ElapsedEventArgs e)
        {
            ev.isPlaying = ev.command.media.isPlaying = false;
            MediaEnded();
        }

        private WaveOut createSong(ulong time)
        {
            if (!File.Exists(songPath))
            {
                return this.waveOut;
            }
            if ((time <= 1) || ((time / (1000.0 * 60)) >= int.MaxValue))
            {
                return createSong();
            }
            this.waveOut = new WaveOut();
            AudioFileReader file = new AudioFileReader(songPath);
            OffsetSampleProvider sample = new OffsetSampleProvider(file);
            this.waveOut.DeviceNumber = device.id;
            sample.SkipOver = TimeSpan.FromMinutes(Math.Floor(time / (1000.0 * 60))).Add(TimeSpan.FromSeconds(Math.Floor((time / 1000.0) % 60))).Add(TimeSpan.FromMilliseconds((int)((time % (1000 * 60)) % 1000)));
            this.waveOut.Init(sample);
            return this.waveOut;
        }

        private WaveOut createSong()
        {
            if (File.Exists((String)(songPath)))
            {
                this.waveOut = new WaveOut();
                this.waveOut.DeviceNumber = device.id;
                waveOut.Volume = 1.0f;
                this.waveOut.Init(new AudioFileReader(songPath));
                return this.waveOut;
            }
            return this.waveOut;
        }

        protected override void LoadAt(ulong time)
        {
            Console.WriteLine("AudioPlayer load " + ev.name + " at " + time);
            if (!File.Exists((String)(songPath)))
            {
                return;
            }
            timer.Interval = ev.duration - time;

            createSong(time);
        }

        protected override void Play()
        {
            Console.WriteLine("AudioPlayer play " + ev.name);
            ev.isPlaying = ev.command.media.isPlaying = true;

            if (!File.Exists((String)(songPath)))
            {
                return;
            }

            if (waveOut == null)
            {
                createSong();
            }

            if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Pause();
            }
            else if (waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Resume();
            }
            else if (waveOut.PlaybackState == PlaybackState.Stopped)
            {
                waveOut.Play();
            }
            ev.isPlaying = ev.command.media.isPlaying = waveOut.PlaybackState == PlaybackState.Playing;
            timer.Start();
        }

        protected override void Stop()
        {
            Console.WriteLine("AudioPlayer stop " + ev.name);

            if (this.waveOut != null)
            {
                this.waveOut.Stop();
                this.Dispose();
                ev.isPlaying = ev.command.media.isPlaying = false;
            }
            fadeTimer.Stop();
            timer.Stop();
        }

        protected override void Pause()
        {
            Console.WriteLine("AudioPlayer pause " + ev.name);

            if (waveOut == null)
            {
                return;
            }
            waveOut.Pause();
            fadeTimer.Stop();
            timer.Stop();
            ev.isPlaying = ev.command.media.isPlaying = false;
        }

        protected void Dispose()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                //waveOut.Dispose();
                waveOut = null;
            }
        }

        protected override void Fade(ulong time, float percent)
        {
            fadeTimer.Start();
            fadeSpeed = 1.0 / (time / fadeTimer.Interval);
            if (waveOut != null)
            {
                waveOut.Volume = 1.0f - percent;
            }
        }

        private void Fade(object sender, ElapsedEventArgs e) {
            if (waveOut != null && waveOut.Volume - fadeSpeed > 0)
            {
                waveOut.Volume = (float)(waveOut.Volume - fadeSpeed);
            }
            if (waveOut != null && waveOut.Volume - fadeSpeed <= 0)
            {
                fadeTimer.Stop();
                waveOut.Volume = 0;
                Stop();
            }
        }
    }
}
