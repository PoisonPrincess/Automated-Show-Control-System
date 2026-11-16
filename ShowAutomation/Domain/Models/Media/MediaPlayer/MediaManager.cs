using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media.MediaPlayer;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowAutomation.Domain.Models.Media
{
    public class MediaManager
    {
        private List<MediaPlayer.MediaPlayer> mediaPlayers = new List<MediaPlayer.MediaPlayer>();
        private OutputDevice device;
        private bool videoMuted = false;
        private bool muted = false;
        private bool mutedBySolo = false;

        public MediaManager(OutputDevice device) {
            this.device = device;
        }

        internal void Stop()
        {
            while (mediaPlayers.Count > 0)
            {
                MediaPlayer.MediaPlayer media = mediaPlayers.First();
                Console.WriteLine("Prep Stop for " + media.ev.name);
                media.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.STOP));
                mediaPlayers.Remove(media);
            }
        }

        internal void Pause()
        {
            foreach (MediaPlayer.MediaPlayer media in mediaPlayers)
            {
                Console.WriteLine("Prep Pause for " + media.ev.name);
                media.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.PAUSE));
            }
        }

        public void PlayEvent(MediaPlayer.MediaPlayer media, ulong eventTime)
        {
            if (!mediaPlayers.Contains(media))
            {
                mediaPlayers.Add(media);
            }
            Console.WriteLine("Prep Play for " + media.ev.name);
            media.Mute(videoMuted);
            media.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.PLAY));
        }

        internal void LoadEvent(MediaPlayer.MediaPlayer media, ulong eventTime)
        {
            if (!muted && !mutedBySolo)
            {
                if (!mediaPlayers.Contains(media))
                {
                    mediaPlayers.Add(media);
                    media.MediaFinishedCallback = RemoveMediaPlayer;
                }
                if (!device.type.Equals(Types.GHOST))
                {
                    Console.WriteLine("Prep Load for " + media.ev.name);
                    media.queue.Add(new MediaPlayerCommand(MediaPlayerCommand.LOAD_AT, eventTime));
                }
            }
        }

        private MediaPlayer.MediaPlayer RemoveMediaPlayer(MediaPlayer.MediaPlayer media)
        {
            mediaPlayers.Remove(media);
            return media;
        }

        internal void MuteBySolo(bool mute)
        {
            if (mute)
            {
                Stop();
            }else if(mutedBySolo != mute)
            {
                mutedBySolo = mute;
                Scene currentScene = MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying];
                if (currentScene.isPlaying)
                {
                    foreach (Event ev in currentScene.getEventsByOutputDevice(device))
                    {
                        if (currentScene.elapsedMilliseconds >= ev.startTime && currentScene.elapsedMilliseconds < ev.getEndTime() && (!device.getDeviceMute() || device.getDeviceSolo()))
                        {
                            ulong eventTime = currentScene.elapsedMilliseconds - ev.startTime;
                            LoadEvent(ev.getPlayer(), eventTime);
                            PlayEvent(ev.getPlayer(), eventTime);
                        }
                    }
                }
            }
            mutedBySolo = mute;
        }

        internal void Mute(bool mute)
        {
            if (mute)
            {
                Stop();
            }else if(muted != mute)
            {
                muted = mute;
                Scene currentScene = MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying];
                if (currentScene.isPlaying)
                {
                    foreach (Event ev in currentScene.getEventsByOutputDevice(device))
                    {
                        if (currentScene.elapsedMilliseconds >= ev.startTime && currentScene.elapsedMilliseconds < ev.getEndTime() && (!device.getDeviceMuteBySolo() || device.getDeviceSolo()))
                        {
                            ulong eventTime = currentScene.elapsedMilliseconds - ev.startTime;
                            LoadEvent(ev.getPlayer(), eventTime);
                            PlayEvent(ev.getPlayer(), eventTime);
                        }
                    }
                }
            }
            muted = mute;
        }

        internal void MuteVideoAudio(bool mute)
        {
            videoMuted = mute;
            foreach(MediaPlayer.MediaPlayer player in mediaPlayers)
            {
                player.Mute(mute);
            }
        }
    }
}
