using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Commands;
using ShowAutomation.UI.Views;

namespace ShowAutomation.Domain.Actions
{
    class SplitVideoAction : Action
    {
        private VideoEvent videoEvent;
        private MusicEvent musicEvent;

        public SplitVideoAction(VideoEvent videoEvent)
        {
            this.videoEvent = videoEvent;
        }

        public override object GetFocus()
        {
            return videoEvent;
        }

        public override void Perform()
        {
            if (musicEvent == null)
            {
                MusicCommand audioCommand = new MusicCommand(videoEvent.name + " Audio", new Song((videoEvent.command.media as Video).videoPath));
                musicEvent = videoEvent.Audio = new MusicEvent(audioCommand, videoEvent.startTime, videoEvent.getScene(), (AudioDevice)MainWindow.main.GetDefaultDevice(Types.AUDIO));
                CommandList list = new CommandList();
                list.AddItem(audioCommand);
                CreateEventWindow window = new CreateEventWindow(videoEvent.getScene(), list, videoEvent.Audio, GetEvent);
                window.txtHours.IsEnabled = false;
                window.txtMinutes.IsEnabled = false;
                window.txtSeconds.IsEnabled = false;
                window.txtMilliseconds.IsEnabled = false;
                window.cmbCommands.IsEnabled = false;
                window.btnCreateCommand.IsEnabled = false;
                window.btnCreateEvent.Content = "Extract Audio";
                window.Title = "Extract Audio";
                window.setEditing(false);
                window.Owner = MainWindow.main;
                window.ShowDialog();
            }
            else {
                videoEvent.Audio = musicEvent;
            }
            (videoEvent.command.media as Video).isMute = true;
            videoEvent.Ungroup();
            MainWindow.main.DrawTimeline();
        }

        private Action GetEvent(Action a) {
            a.Perform();
            musicEvent = videoEvent.Audio = (MusicEvent)a.GetFocus();
            return a;
        }

        public override string Title()
        {
            return "Extract Audio from " + videoEvent.name;
        }

        public override void Undo()
        {
            videoEvent.getScene().removeEvent(videoEvent.Audio);
            videoEvent.Audio = null;
            (videoEvent.command.media as Video).isMute = false;
            videoEvent.Ungroup();
            MainWindow.main.DrawTimeline();
        }
    }
}
