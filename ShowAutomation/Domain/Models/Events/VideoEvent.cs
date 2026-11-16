using ShowAutomation.Domain.Actions;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ShowAutomation.Domain.Models.Events
{
    [Serializable()]
    public class VideoEvent : Event, ISerializable
    {
        public MusicEvent Audio = null;
        private bool hasAudio;

        public VideoEvent(VideoCommand command, ulong startTime, Scene scene) : base(command, startTime, scene)
        {
            this.type = Types.VIDEO;
            player = new VideoPlayer(this);
            hasAudio = (command.media as Video).HasAudio();
            Console.WriteLine(name +  " has Audio " + hasAudio);
        }

        public VideoEvent(VideoCommand command, ulong startTime, Scene scene, VideoDevice device) : this(command, startTime, scene)
        {
            this.OutputDevice = device;
        }

        public VideoEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            player = new VideoPlayer(this);
            hasAudio = (command.media as Video).HasAudio();
            Console.WriteLine(name + " has Audio " + hasAudio);
        }

        public void setCommand(VideoCommand command)
        {
            this.command = command;
            this.name = command.name;
        }

        public override object ToJsonInstance()
        {
            return new JsonVideoEvent(this);
        }

        protected override void AddEventSpecificContextMenuItems(ContextMenu contextMenu)
        {
            
            contextMenu.Items.Add(new Separator());

            MenuItem splitAudio = new MenuItem();
            splitAudio.Header = "Extract Audio" + ((!hasAudio) ? " (No Audio)" : "");
            splitAudio.Click += SplitAudioOut;
            splitAudio.IsEnabled = videoPathExits() && !scene.Events.Contains(Audio) && hasAudio;
            contextMenu.Items.Add(splitAudio);
        }

        private void SplitAudioOut(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Extract Audio");
            MainWindow.main.DoAction(new SplitVideoAction(this));
        }

        public override void Ungroup()
        {
            group = null;
            if (Audio != null) {
                group = new EventGroup();
                group.Add(this);
                group.Add(Audio);
                group.Lock(true);
                Audio.group = group;
            }
        }

        protected Boolean videoPathExits()
        {
            return File.Exists((this.command.media as Video).videoPath);
        }

        public override object Clone()
        {
            VideoEvent clone = new VideoEvent((VideoCommand)command, startTime, null, (VideoDevice)device);
            clone.group = this.group;
            return clone;
        }
    }

    public class JsonVideoEvent : JsonEvent
    {
        public dynamic outputDevice;
        public JsonVideoEvent(VideoEvent ev) : base(ev)
        {
            this.command = ev.command.ToJsonInstance();
            this.outputDevice = ev.OutputDevice.ToJsonInstance();
        }
    }
}
