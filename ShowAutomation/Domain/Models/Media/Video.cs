using System;
using ShowAutomation.UI.Views;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.WindowsAPICodePack.Shell;
using System.Runtime.Serialization;
using NAudio.Wave;

namespace ShowAutomation.Domain.Models.Media
{
    [Serializable()]
    public class Video : MediaModel
    {
        public String videoPath;
        VideoPanel subWindow = new VideoPanel();
        protected int deviceId = -1;
        public Boolean isMute;
        VideoPanel thumbNail = new VideoPanel();
        MediaElement media = new MediaElement();
        public Duration NaturalDuration { get; }
        private string unitTestPlaybackState;

        public void setUnitTestPlaybackState(string state)
        {
            unitTestPlaybackState = state;
        }
        public string getUnitTestPlaybackState()
        {
            return unitTestPlaybackState;
        }
        public override ulong getDuration ()
        {
            double seconds = 1; //
            if (videoPath != null && (File.Exists((String)(videoPath))))
            {
                ShellFile so = ShellFile.FromFilePath(videoPath);
                
                double nanoseconds;
                double.TryParse(so.Properties.System.Media.Duration.Value.ToString(),
                out nanoseconds);

                if (nanoseconds > 0)
                {
                    seconds = (nanoseconds) / 10000;
                }
            }
                return (ulong)seconds;
            
        }

        internal bool HasAudio()
        {
            try
            {
                new AudioFileReader(videoPath);
                return true;
            }
            catch {
                return false;
            }
        }

        //get video path
        public Video(String videoPath) : base()
        {
            this.videoPath = videoPath;
        }

        public Video(SerializationInfo info, StreamingContext context) : this((String)info.GetValue("videoPath", typeof(String)))
        {
            if (!File.Exists(videoPath))
            {
                MissingFilesWindow test = new MissingFilesWindow(videoPath, this);
                test.Owner = MainWindow.main;
                if (test.ShowDialog() == true) ;
            }
        }

        private void ShowOnMonitor(int monitor)
        {            
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ///VideoDeciceShowOnMonitor(0, monitor);
            });
        }

        //mute window
        public void Mute()
        {
            this.subWindow.mute(isMute);
        }

        //close window
        public void close()
        {
            if (this.subWindow != null)
            {
                this.subWindow.close();
            }
            if (this.thumbNail != null)
            {
                this.thumbNail.close();
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("videoPath", videoPath);
        }
    }
}