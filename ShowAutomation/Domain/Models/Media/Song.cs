using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WPFSoundVisualizationLib;
using System.Windows.Threading;
using System.IO;
using System.Threading;
using ShowAutomation.UI.ViewModels.Media;
using NAudio.Wave.SampleProviders;
using System.Runtime.Serialization;
using ShowAutomation.UI.Views;

namespace ShowAutomation.Domain.Models.Media
{
    [Serializable()]
    public class Song : MediaModel, INotifyPropertyChanged
    {
        protected NAudio.Wave.MediaFoundationReader wave = null;
        protected WaveOut song = null;
        protected int deviceId = -1;
        private String _songPath;
        public String songPath {
            get
            {
                return _songPath;
            }
            set
            {
                _songPath = value;
                GenerateWave();
            }
        }
        public event Action PlaybackStopped;

        //not used:
        private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
        private bool disposed;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool isPlaying;
        private bool inChannelTimerUpdate;
        
        private double channelLength;
        private double channelPosition;
        private bool inChannelSet;
        private WaveStream activeStream;

        //For waveform generation
        public SongWaveformGenerator waveformGenerator;
        public List<float>[] waveData = new List<float>[3] { new List<float>(), new List<float>(), new List<float>() };

        protected String fileExtension { get; set; }

        public Song(String songPath) : base()
        {
            this.songPath = songPath;
        }

        private void GenerateWave()
        {
            if (MainWindow.main.redrawTicker())
            {
                waveformGenerator = new SongWaveformGenerator();
                waveformGenerator.UpdateData += setWaveData;
                waveformGenerator.songPath = songPath;
                waveformGenerator.UIDisaptcher = Dispatcher.CurrentDispatcher;
                Thread t = new Thread(new ThreadStart(waveformGenerator.getWaveData));
                t.Start();
            }
        }

        private void setWaveData(object sender, EventArgs e)
        {
            waveData = sender as List<float>[];
        }
        public Song(SerializationInfo info, StreamingContext context) : this((String)info.GetValue("songPath", typeof(String))) {

            if (!File.Exists(songPath))
            {
                MissingFilesWindow test = new MissingFilesWindow(songPath, this);
                test.Owner = MainWindow.main;
                if (test.ShowDialog() == true);
            }

        }

        private WaveOut createSong()
        {
            if (File.Exists((String)(songPath)))
            {
                this.song = new WaveOut();
                this.song.DeviceNumber = this.deviceId;
                this.song.Init(new AudioFileReader(songPath));
                return this.song;
            }
            return this.song;
        }

        private WaveOut createSong(ulong time)
        {
            if (!File.Exists(songPath))
            {
                return this.song;
            }
            if ((time <= 1) || ((time / (1000.0 * 60)) >= int.MaxValue))
            {
                return createSong();
            }
            this.song = new WaveOut();
            AudioFileReader file = new AudioFileReader(songPath);
            OffsetSampleProvider sample = new OffsetSampleProvider(file);

            sample.SkipOver = TimeSpan.FromMinutes(Math.Floor(time / (1000.0 * 60))).Add(TimeSpan.FromSeconds(Math.Floor((time / 1000.0) % 60))).Add(TimeSpan.FromMilliseconds((int)((time % (1000 * 60)) % 1000)));
            this.song.Init(sample);
            return this.song;
        }

        private WaveOut createSong(ulong time, int deviceId)
        {
            this.deviceId = deviceId;
            return this.createSong(time);
        }

        private WaveOut createSong(int deviceId)
        {
            this.deviceId = deviceId;
            return this.createSong();
        }
        
        public override ulong getDuration()
        {
            if (File.Exists((String)songPath))
            {
                AudioFileReader reader = new AudioFileReader(songPath); //Naudio exception   Message=NoDriver calling acmFormatSuggest
                TimeSpan duration = reader.TotalTime;
                return (ulong)duration.TotalMilliseconds;
            }
            return 1;
        }

        protected void Dispose()
        {
            if (this.song != null)
            {
                if (this.song.PlaybackState == PlaybackState.Playing)
                {
                    this.song.Stop();
                }
                //song.Dispose();
                this.song = null;
            }
            if (this.wave != null)
            {
                this.wave.Dispose(); wave = null;
            }
        }

        protected void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.Dispose();
            if (PlaybackStopped != null)
            {
                this.PlaybackStopped();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public double ChannelPosition
        {
            get { return channelPosition; }
            set
            {
                if (!inChannelSet)
                {
                    inChannelSet = true; // Avoid recursion
                    double oldValue = channelPosition;
                    double position = Math.Max(0, Math.Min(value, ChannelLength));
                    if (!inChannelTimerUpdate && ActiveStream != null)
                        ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
                    channelPosition = position;
                    if (oldValue != channelPosition)
                        NotifyPropertyChanged("ChannelPosition");
                    inChannelSet = false;
                }
            }
        }

        public double ChannelLength
        {
            get { return channelLength; }
            protected set
            {
                double oldValue = channelLength;
                channelLength = value;
                if (oldValue != channelLength)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                bool oldValue = isPlaying;
                isPlaying = value;
                if (oldValue != isPlaying)
                    NotifyPropertyChanged("IsPlaying");
            }
        }

      

        public WaveStream ActiveStream
        {
            get { return activeStream; }
            protected set
            {
                WaveStream oldValue = activeStream;
                activeStream = value;
                if (oldValue != activeStream)
                    NotifyPropertyChanged("ActiveStream");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("songPath", songPath);
        }
    }
}