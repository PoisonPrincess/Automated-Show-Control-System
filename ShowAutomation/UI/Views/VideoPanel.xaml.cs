using ShowAutomation.Domain.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for VideoPannel.xaml
    /// </summary>
    public partial class VideoPanel : Window
    {
        private int screen;
        public bool KeyPreview;

        public VideoPanel()
        {
         
            InitializeComponent();
            this.KeyPreview = true;
            this.MediaPlayer.Volume = 0;

            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(HandleEsc);
            this.StateChanged += Form1_Resize;
        }

        public VideoPanel(int screen)
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(HandleEsc);
            this.StateChanged += Form1_Resize;
            this.Loaded += Window_Loaded;
            this.screen = screen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Mode.isEditMode && !Screen.AllScreens[screen].Equals(MainWindow.main.getMainScreen()))
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        internal void setMediaViewer(MediaElement videoViewer)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (MediaPlayer != null)
                {
                    MediaPlayer.Pause();
                    MainGrid.Children.Remove(MediaPlayer);
                }
                if (videoViewer != null)
                {
                    if (videoViewer.Parent != null)
                    {
                        (videoViewer.Parent as Grid).Children.Remove(videoViewer);
                    }
                    MainGrid.Children.Add(videoViewer);
                }
                MediaPlayer = videoViewer;
                if (videoViewer != null)
                {
                    Console.WriteLine("Media " + videoViewer.Source.AbsoluteUri);
                }
            });
        }

        public void stop(Object sender, ExecutedRoutedEventArgs e)
        {

            this.MediaPlayer.Stop();

        }

        public void play()
        {
            this.MediaPlayer.Play();   
        }

        public void setTime(ulong time) {
            play();
            this.MediaPlayer.Position = TimeSpan.FromMilliseconds(time);
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                this.MediaPlayer.Pause();
            });
        }

        public void mute(bool mute)
        {
            if (mute)
            {
                this.MediaPlayer.Volume = 0;
            }else
            {
                this.MediaPlayer.Volume = 1;
            }

        }

        public void close(Object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                MainGrid.Children.Remove(MediaPlayer);
                this.Close();
            });
        }

        public void close()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                if (this.MediaPlayer != null)
                {
                    this.MediaPlayer.Close();
                }
                this.Close();
            });
        }

        private void Opened(object sender, EventArgs e){}       //not used

        private void HandleEsc(object sender, System.Windows.Input.KeyEventArgs e)
        {
             // Minimize with the option to close, or make full screen again
            if (e.Key == Key.Escape)
                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // handle when user wants go back to full screen
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
        }

    }
}
