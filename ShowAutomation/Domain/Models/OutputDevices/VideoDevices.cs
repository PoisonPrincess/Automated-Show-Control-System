using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ShowAutomation.Domain.Models.OutputDevices
{
    [Serializable()]
    public class VideoDevice : OutputDevice, ISerializable
    {
        public Screen device { get; set; }
        private VideoPanel videoPanel = null;
        private MediaElement PreviewViewer = null;
        private Video videoPlaying = null;

        public VideoDevice(int id, Screen device) : base(id, device.DeviceName)
        {
            this.device = device;
            this.isConnected = true;
            this.type = Types.VIDEO;
            this.ghostType = Types.GHOST;
        }

        public VideoDevice(string deviceName)
        {
            this.deviceName = deviceName;
            this.type = Types.VIDEO;
            this.ghostType = Types.GHOST;
        }

        public VideoDevice(int id, string deviceName1) : base(id, deviceName1)
        {
            this.deviceName = deviceName1;
            this.isConnected = true;
            this.type = Types.VIDEO;
            this.ghostType = Types.GHOST;
        }

        internal bool isPlaying(MediaElement videoViewer, MediaElement previewViewer)
        {
            if (videoPanel != null)
            {
                return videoViewer == videoPanel.MediaPlayer;
            }
            return previewViewer == PreviewViewer;
        }

        internal void setMediaViewer(MediaElement videoViewer)
        {
            if (videoPanel == null) {
                ShowOnMonitor(id);
            }
            if (videoPanel != null)
            {
                videoPanel.setMediaViewer(videoViewer);
            }
        }

        internal void setPreviewMediaViewer(MediaElement previewVideoViewer)
        {
            if (!IsDuplicate)
            {
                PreviewViewer = previewVideoViewer;
                MainWindow.main.setPreviewViewer(this, previewVideoViewer);
            }
            else
            {
                ((VideoDevice)original).setPreviewMediaViewer(previewVideoViewer);
            }
        }

        public MediaElement getPreviewViewer() {
            if (!IsDuplicate)
            {
                return PreviewViewer;
            }
            else
            {
                return ((VideoDevice)original).getPreviewViewer();
            }
        }

        public void PauseVideo()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (videoPanel != null && videoPanel.MediaPlayer != null)
                {
                    videoPanel.MediaPlayer.Pause();
                }
            });
        }

        public void videoClose()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (videoPanel != null)
                {
                    videoPanel.setMediaViewer(null); //Set video to black
                }
            });
        }


        private void ShowOnMonitor(int monitor)
        {
            videoPanel = null;
            if (monitor < Screen.AllScreens.Length)
            {
                Console.WriteLine("Default Screen: " + MainWindow.main.getMainScreen().DeviceName + " Display on Screen: " + Screen.AllScreens[monitor].DeviceName);
                if (!Screen.AllScreens[monitor].Equals(MainWindow.main.getMainScreen()))
                {
                    Console.WriteLine("Create monitor for " + deviceName);
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        videoPanel = new VideoPanel(monitor);
                        if (Mode.isEditMode)
                        {
                            videoPanel.WindowStyle = WindowStyle.ThreeDBorderWindow;
                        }
                        else
                        {
                            videoPanel.WindowStyle = WindowStyle.None;
                        }
                        if (type.Equals(Types.VIDEO))
                        {
                            Screen s1 = Screen.AllScreens[monitor];

                            System.Drawing.Rectangle r1 = s1.WorkingArea;

                            videoPanel.WindowStartupLocation = WindowStartupLocation.Manual;
                            videoPanel.Top = r1.Y;
                            videoPanel.Left = r1.X;
                            videoPanel.Owner = MainWindow.main;
                            videoPanel.Show();
                            videoPanel.Visibility = Visibility.Visible;
                            videoPanel.Topmost = true;
                            videoPanel.ResizeMode = ResizeMode.CanResize;
                        }
                    });
                    videoPanel.Closed += VideoPanelClosed;
                }
                else Console.WriteLine("Don't Create monitor for " + deviceName);
            }
        }

        public void muteVideo(bool mute)
        {
            if(!IsDuplicate)
            {
                mediaManager.MuteVideoAudio(mute);
            }
            else
            {
                ((VideoDevice)original).muteVideo(mute);
            }
        }

        private void VideoPanelClosed(object sender, EventArgs e)
        {
            videoPanel = null;
            if((videoPlaying != null) && (videoPlaying.isPlaying))
            {
                ulong time = MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].elapsedMilliseconds;
                if (MainWindow.main.sceneList.isPlaying)
                {
                    foreach (Event thisEvent in MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying].Events)
                    {
                        if ((thisEvent.startTime < time) && (thisEvent.getEndTime() > time) && (thisEvent is VideoEvent) && (thisEvent.command.media.Equals(videoPlaying)))
                        {
                            ReopenVideoPanel();
                            LoadCommandAt(thisEvent, (time - thisEvent.startTime));
                            PlayCommandAt(thisEvent, (time - thisEvent.startTime));
                            break;
                        }
                    }
                }
            }
        }

        public VideoDevice(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public int getDevice()
        {
            return (this.isActive()) ? this.id : -1;
        }

        public Boolean isActive()
        {
            return isConnected;
        }

        public override object ToJsonInstance()
        {
            return new JsonVideoDevice(this);
        }

        public static int getDeviceIndexByName(String deviceName)
        {
            List<OutputDevice> devices = MainWindow.main.outputs.getOutputs(Types.VIDEO, true);
            for (var i = 0; i < devices.Count; i++)
            {   
                if (devices[i].deviceName == deviceName)
                {
                    return i;
                }
            }
            return -1;
        }

        //this method scans for connected devices and creates new outputs from them
        //DON'T CALL UNLESS YOU WANT DUPLICATE OUTPUT DEVICES FLOATING AROUND
        public static List<VideoDevice> getVideoDevices(bool includeGhost, bool includeLane)
        {
            int id =0;
            Screen[] screens = Screen.AllScreens;
            List<VideoDevice> deviceList = new List<VideoDevice>();
  
            for(id = 0; id < screens.Length; id++){
                deviceList.Add(new VideoDevice(id, screens[id].DeviceName));
            }

            if (includeLane)
            {
                List<OutputDevice> laneDevices = LaneOutput.getLaneDevices().ToList();
                for (int i = 0; i < laneDevices.Count; i++)
                {
                    if (laneDevices[i].type.Equals(Types.VIDEO))
                    {
                        deviceList.Add(laneDevices[i] as VideoDevice);
                    }
                }
            }
            if (includeGhost)
            {
                List<OutputDevice> ghostDevices = GhostOutput.getGhostDevices().ToList();
                for (int i = 0; i < ghostDevices.Count; i++)
                {
                    if (ghostDevices[i].type.Equals(Types.GHOST) && ghostDevices[i].ghostType.Equals(Types.VIDEO))
                    {
                        deviceList.Add(ghostDevices[i] as VideoDevice);
                        ghostDevices[i].id = id;
                        id++;
                    }
                }
            }
            return deviceList;
        }

        internal override void OpenVideoPanel()
        {
            if(videoPanel == null)
            {
                ShowOnMonitor(id);
            }
        }

        public void ReopenVideoPanel() {
            if (videoPanel != null)
            {
                videoPanel.close();
                videoPanel = null;
            }
            OpenVideoPanel();
        }



        internal override void PlayCommandAt(Event command, ulong eventTime)
        {
            if (!IsDuplicate)
            {
                mediaManager.PlayEvent((command as VideoEvent).getPlayer(), eventTime);
            }
            else {
                original.PlayCommandAt(command, eventTime);
            }
        }

        internal override void LoadCommandAt(Event command, ulong eventTime)
        {
            if (!IsDuplicate)
            {
                mediaManager.LoadEvent((command as VideoEvent).getPlayer(), eventTime);
            }
            else
            {
                original.LoadCommandAt(command, eventTime);
            }
        }
    }

    public class JsonVideoDevice : JsonOutputDevice
    {
        public JsonVideoDevice(VideoDevice device) : base(device.id, device.deviceName)
        {
            this.type = Types.VIDEO;
        }
    }
}
