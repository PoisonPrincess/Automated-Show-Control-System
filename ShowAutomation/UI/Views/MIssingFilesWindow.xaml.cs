using Microsoft.Win32;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.Domain.Utils.Midi;
using ShowAutomation.UI.ViewModels.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for MissingFilesWindow.xaml
    /// </summary>
    public partial class MissingFilesWindow : Window, INotifyPropertyChanged
    {
        private AbstractCommand Command { get; set; }
        private Timer previewTimer = new Timer();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private String filePath;
        private String fileName;
        private String musicFileFilter = "Song files (*.wav, *.mp3,*.pcm, *.mp4, *.aiff, *.mpeg, *.wma, *.m4a) | *.wav; *.mp3; *.pcm; *.mp4; *.aiff; *.mpeg; *.wma; *.m4a";
        private String videoFileFilter = "Video files (*.avi, *.mp4, *.mpeg, *.wmv) | *.avi; *.mp4; *.mpeg; *.wmv";
        private MidiDeviceManager midiDeviceManager = MidiDeviceManager.Instance;
        private Func<AbstractCommand, AbstractCommand> doneFunction;
        private String _errorMessages = null;
        public Boolean _hasErrors = false;
        private string type;
        private MediaModel Media;
        private bool keyPreview;

        public event PropertyChangedEventHandler PropertyChanged;

        public MissingFilesWindow(String path, MediaModel media)
        {
            InitializeComponent();
            this.keyPreview = true;
            this.Media = media;

            txtCommandName.Text = Path.GetFileName(path);

            txtlabelPath.Text = path;
        }

        public void btnImportFileClick(object sender, RoutedEventArgs e)
        {
           
            //Open File explore and brings in video file if textbox2 == aud string 
            //creates the aud object and stores in command list
            OpenFileDialog open = new OpenFileDialog();

            if (Media is Song )
            {
                open.Filter = musicFileFilter;
            }
            else if (Media is Video)
            {
                open.Filter = videoFileFilter;
            }

            if (open.ShowDialog() == DialogResult)
            {
                return;
            }

            filePath = open.FileName;
            fileName = Path.GetFileName(filePath);
            txtFilePath.Text = filePath;
        }
 
        //formats the save button for all conditions
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MissingData())
            {
                if (Media is Song)
                {
                    (Media as Song).songPath = filePath;
                }
                else if (Media is Video)
                {
                    (Media as Video).videoPath = filePath;
                }
                Close();
            }
        }

        private void enterButton(object sender, KeyEventArgs e)
        {
            if (!MissingData() && e.Key == Key.Enter)
            {
                if (Media is Song)
                {
                    (Media as Song).songPath = filePath;
                }
                else if (Media is Video)
                {
                    (Media as Video).videoPath = filePath;
                }
                Close();
            }
        }

        //Error messages 
        protected void showErrors()
        {
            if (this._hasErrors)
            {
                txtBlockErrorMessageCmd.Text = this._errorMessages;
                txtBlockErrorMessageCmd.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtBlockErrorMessageCmd.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private Boolean MissingData()
        {
            if ((filePath == null || filePath == "" || !File.Exists(filePath)))
            { 
                this._hasErrors = true;
                this._errorMessages = "Must have a valid file for the command";
                this.showErrors();
                return true;
            }
            return false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CommandList n = MainWindow.main.commandList;
            if (MessageBox.Show("Are you sure?", "Canceling this procress will delete the command and any event associated with it", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }
    }
}
