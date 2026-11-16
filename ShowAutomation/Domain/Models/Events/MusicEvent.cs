using System;
using System.Collections.Generic;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using ShowAutomation.Domain.Models.Media;
using System.Windows.Controls;
using System.Runtime.Serialization;
using ShowAutomation.Domain.Models.Media.MediaPlayer;

namespace ShowAutomation.Domain.Models.Events
{

    [Serializable()]
    public class MusicEvent : Event, ISerializable
    {
        public static Boolean waveformType = true; // false = Polygon line, true = rectangle blocks
        public Polyline wave;

        public MusicEvent(MusicCommand command, ulong startTime, Scene scene) : base(command, startTime, scene)
        {
            (command.media as Song).waveformGenerator.UpdateData += UpdateData;
            this.type = Types.AUDIO;
            player = new AudioPlayer(this);

            var song = (command as MusicCommand).media as Media.Song;
        }

        public MusicEvent(MusicCommand command, ulong startTime, Scene scene, AudioDevice device) : this(command, startTime, scene)
        {
            this.OutputDevice = device;
        }

        public MusicEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            player = new AudioPlayer(this);
        }

        public override object ToJsonInstance()
        {
            return new JsonMusicEvent(this);
        }

        public void setCommand(MusicCommand command)
        {
            this.command = command;
            this.name = command.name;
        }

        public override Canvas drawEvent()
        {
            var song = this.command.media as Song;
            base.drawEvent();
            if (!File.Exists(song.songPath))
            {
                return TimelineEvent;
            }
            //draw Waveform here
            if (!waveformType)
            {
                wave = new Polyline();
                wave.Stroke = Brushes.Gray;
                wave.StrokeThickness = .5;
                Canvas.SetTop(wave, 25);
                TimelineEvent.Children.Insert(TimelineEvent.Children.Count - 1, wave);
            }

            UpdateData();

            return TimelineEvent;
        }

        protected override void SimplifiyRendering()
        {
            //this is done to reduce rendering lag when drag/droping the event
            if (!waveformType)      //for waveformType false (polygon)
            {
                UpdateData((command.media as Song).waveData[0].ToArray());
            }
        }

        private void UpdateData(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            (command.media as Song).waveformGenerator.UpdateData += UpdateData;
            double width = MainWindow.main.scaleToScreen(1000);
            List<float> data = (command.media as Song).waveData[2];
            if (data != null)
            {
                if (width <= 30 || data.Count < 2)
                {
                    data = (command.media as Song).waveData[1];
                }
                if (width <= 15 || data.Count < 2)
                {
                    data = (command.media as Song).waveData[0];
                }

                UpdateData(data.ToArray());
            }
        }

        private void UpdateData(float[] data)
        {            
                PointCollection points = new PointCollection();
                double distBetweenPoints = (MainWindow.main.scaleToScreen(duration) / data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    if (waveformType)
                    {
                        Rectangle waveRectangle = new Rectangle();                    //for waveformType true (recangles)
                        if ((distBetweenPoints + .5) > 0) waveRectangle.Width = distBetweenPoints + .5;
                        waveRectangle.Height = (Math.Abs(data[i]) * 45);
                        Canvas.SetTop(waveRectangle, (25 - (waveRectangle.Height / 2)));
                        Canvas.SetLeft(waveRectangle, (distBetweenPoints * i));
                        waveRectangle.Fill = Brushes.Gray;
                        if (TimelineEvent != null && TimelineEvent.Children.Count > 1)
                        {
                            TimelineEvent.Children.Insert(TimelineEvent.Children.Count - 1, waveRectangle);
                        }
                    }
                    else
                    {
                        points.Add(new Point(distBetweenPoints * i, data[i] * -24)); //for waveformType false (polygon)
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (!waveformType)
                    {
                        wave.Points = points;
                    }
                });
        }

        protected override void AddEventSpecificContextMenuItems(System.Windows.Controls.ContextMenu contextMenu)
        {
        }

        public override object Clone()
        {
            MusicEvent clone = new MusicEvent((MusicCommand)command, startTime, null, (AudioDevice)device);
            clone.group = group;
            return clone;
        }
    }

    public class JsonMusicEvent : JsonEvent
    {
        public dynamic outputDevice;
        public JsonMusicEvent(MusicEvent ev) : base(ev)
        {
            this.command = ev.command.ToJsonInstance();
            this.outputDevice = ev.OutputDevice.ToJsonInstance();
        }
    }
}
 