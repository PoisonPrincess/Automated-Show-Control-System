using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.OutputDevices;

namespace ShowAutomation.Domain.Models.Media.MediaPlayer
{
    public class VideoPlayer : MediaPlayer
    {
        private MediaElement VideoViewer = new MediaElement();
        private MediaElement PreviewVideoViewer = new MediaElement();

        public VideoPlayer(VideoEvent video) : base(video.OutputDevice)
        {
            ev = video;
            VideoViewer.LoadedBehavior = MediaState.Manual;
            PreviewVideoViewer.LoadedBehavior = MediaState.Manual;
            timer.Interval = video.command.getDuration();
            timer.Elapsed += FinishedPlaying;
            fadeTimer.Elapsed += Fade;

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                VideoViewer.MediaOpened += setVideoToTime;
                PreviewVideoViewer.MediaOpened += setVideoToTime;
                VideoViewer.Source = new Uri(((Video)ev.command.media).videoPath);
                PreviewVideoViewer.Source = new Uri(((Video)ev.command.media).videoPath);
            });
        }

        private void FinishedPlaying(object sender, ElapsedEventArgs e)
        {
            ev.isPlaying = ev.command.media.isPlaying = false;
            if (((VideoDevice)ev.OutputDevice).isPlaying(VideoViewer, PreviewVideoViewer))
            {
                ((VideoDevice)ev.OutputDevice).videoClose();
                MainWindow.main.setPreviewViewer((VideoDevice)device, null);
            }
            MediaEnded();
        }

        protected override void Pause()
        {
            fadeTimer.Stop();
            timer.Stop();
            Console.WriteLine("VideoPlayer pause " + ev.name);
            ev.isPlaying = ev.command.media.isPlaying = false;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                VideoViewer.Pause();
                PreviewVideoViewer.Pause();
            });
        }

        protected override void Play()
        {
            Console.WriteLine("VideoPlayer play " + ev.name);
            ev.isPlaying = ev.command.media.isPlaying = true;
            timer.Start();
            ((VideoDevice)device).setMediaViewer(VideoViewer);
            ((VideoDevice)device).setPreviewMediaViewer(PreviewVideoViewer);
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (((Video)ev.command.media).isMute)
                {
                    VideoViewer.Volume = 0;
                }
                else
                {
                    VideoViewer.Volume = muted ? 0 : 1;
                }
                Console.WriteLine("Play " + ev.name + " from " + VideoViewer.Position.TotalMilliseconds);
                VideoViewer.Play();
                PreviewVideoViewer.Play();
            });
        }

        protected override void LoadAt(ulong time)
        {
            if (time <= ev.command.getDuration())
            {
                Console.WriteLine("VideoPlayer load " + ev.name + " at " + time);
                timer.Interval = ev.command.getDuration() - time;
                if (ev.getScene() == null || ev.getScene().elapsedMilliseconds > ev.startTime && ev.getScene().elapsedMilliseconds < ev.getEndTime())
                {
                    ((VideoDevice)device).setMediaViewer(VideoViewer);
                    ((VideoDevice)device).setPreviewMediaViewer(PreviewVideoViewer);
                }
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    VideoViewer.Volume = 0;
                    PreviewVideoViewer.Volume = 0;
                    VideoViewer.Opacity = PreviewVideoViewer.Opacity = 1.0f;
                    VideoViewer.Play();
                    PreviewVideoViewer.Play();
                    VideoViewer.Position = TimeSpan.FromMilliseconds(time);
                    PreviewVideoViewer.Position = TimeSpan.FromMilliseconds(time);
                    VideoViewer.Pause();
                    PreviewVideoViewer.Pause();

                    if (((Video)ev.command.media).isMute)
                    {
                        VideoViewer.Volume = 0;
                    }
                    else
                    {
                        VideoViewer.Volume = muted ? 0 : 1;
                    }
                });
            }
        }

        private void setVideoToTime(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                int LoadTo = ev.getScene() != null ? (int)ev.getScene().elapsedMilliseconds - (int)ev.startTime : 0;
                if (LoadTo < 0) LoadTo = 0;
                Console.WriteLine("Video " + ev.name + " loaded at " + LoadTo);
                (sender as MediaElement).Position = TimeSpan.FromMilliseconds(LoadTo);
                if (!ev.isPlaying)
                {
                    VideoViewer.Volume = 0;
                    PreviewVideoViewer.Volume = 0;
                    VideoViewer.Play();
                    PreviewVideoViewer.Play();
                    VideoViewer.Position = TimeSpan.FromMilliseconds(LoadTo);
                    PreviewVideoViewer.Position = TimeSpan.FromMilliseconds(LoadTo);
                    Thread.Sleep(25);
                    VideoViewer.Pause();
                    PreviewVideoViewer.Pause();
                }else
                {
                    VideoViewer.Play();
                    PreviewVideoViewer.Play();
                }
            });
        }

        protected override void Stop()
        {
            fadeTimer.Stop();
            timer.Stop();
            FinishedPlaying(null, null);
            Console.WriteLine("VideoPlayer stop " + ev.name);
            ev.isPlaying = ev.command.media.isPlaying = false;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                VideoViewer.Stop();
                PreviewVideoViewer.Stop();
            });
        }

        public MediaElement GetViewer() {
            return VideoViewer;
        }

        public override void Mute(bool mute)
        {
            muted = mute;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (mute)
                {
                    VideoViewer.Volume = 0;
                    PreviewVideoViewer.Volume = 0;
                }else
                {
                    VideoViewer.Volume = PreviewVideoViewer.Volume = (((Video)ev.command.media).isMute) ? 0 : 1;
                }
            });
        }


        protected override void Fade(ulong time, float percent)
        {
            fadeTimer.Start();
            fadeSpeed = 1.0 / (time / fadeTimer.Interval);
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (!((Video)ev.command.media).isMute && !muted)
                {
                    VideoViewer.Volume = 1.0f - percent;
                    PreviewVideoViewer.Volume = (PreviewVideoViewer.Volume > 0) ? 1.0f - percent : 0;
                }
                VideoViewer.Opacity = PreviewVideoViewer.Opacity = 1.0f - percent;
            });
        }

        private void Fade(object sender, ElapsedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (VideoViewer.Volume - fadeSpeed > 0)
                {
                    if (!((Video)ev.command.media).isMute && !muted)
                    {
                        VideoViewer.Volume = VideoViewer.Volume - fadeSpeed;
                        PreviewVideoViewer.Volume = (PreviewVideoViewer.Volume > 0) ? VideoViewer.Volume : 0;
                    }
                    VideoViewer.Opacity = PreviewVideoViewer.Opacity = VideoViewer.Opacity - fadeSpeed;
                }
                if (VideoViewer.Volume - fadeSpeed <= 0)
                {
                    fadeTimer.Stop();
                    ev.getScene().StopEvent(ev);
                    if (!((Video)ev.command.media).isMute && !muted)
                    {
                        VideoViewer.Volume = PreviewVideoViewer.Volume = 0;
                    }
                    VideoViewer.Opacity = PreviewVideoViewer.Opacity = 0;
                    Stop();
                }
            });
        }
    }
}
