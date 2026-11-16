using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Scenes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShowAutomation.UI.ViewModels
{
    class TimelineInfo
    {
        //data
        public readonly static double showRatio = 1.4;
        private static double ShowViewRatio = 1.05;
        private readonly static double zoomStandard = 1.0 / 1000; // pixels per millisecond. zoomStandard = 100% zoom
        private double TimelineScale = zoomStandard;
        private String cursorTime = "ct: ––:––:––.–––";

        //UI eliments
        private TextBlock cursorTimeText;
        private TextBlock currentTimeText;
        private TextBlock endTimeText;
        private ComboBox TimelineScaleText;

        //get timeline scale
        public double getScale() {
            return TimelineScale;
        }

        //Set up timeline info UI
        public void SetUpInfo(Scene scene, MouseEventArgs e) {
            Grid info = MainWindow.main.TimelineInfo;
            info.Children.Clear();
            info.ColumnDefinitions.Clear();

            TimelineScaleText = new ComboBox();
            cursorTimeText = new TextBlock();
            currentTimeText = new TextBlock();
            endTimeText = new TextBlock();

            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(ColorManager.TEXT_COLOR);
            cursorTimeText.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            currentTimeText.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            endTimeText.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

            endTimeText.ToolTip = "Remaining Scene Time: Time to End of Scene";
            cursorTimeText.ToolTip = "Cursor Time: Position of cursor over Timeline";
            currentTimeText.ToolTip = "Scene Time: Current playback position";

            if (!Mode.isEditMode)
            {
                cursorTimeText.Foreground = Brushes.LightGray;
                currentTimeText.Foreground = Brushes.LightGray;
            }

            TimelineScaleText.IsEditable = true;
            TimelineScaleText.IsTextSearchEnabled = true;
            TimelineScaleText.Items.Add("Fit to Screen");
            TimelineScaleText.Items.Add("100%");
            TimelineScaleText.Items.Add("200%");
            TimelineScaleText.Items.Add("400%");
            TimelineScaleText.Items.Add("800%");
            TimelineScaleText.Items.Add("1200%");
            TimelineScaleText.Items.Add("2400%");
            TimelineScaleText.Items.Add("6000%");
            TimelineScaleText.Items.Add("12000%");
            TimelineScaleText.Items.Add("24000%");
            TimelineScaleText.DropDownClosed += updateZoom;
            TimelineScaleText.KeyDown += updateZoom;
            
            ColumnDefinition col = new ColumnDefinition();
            col.Width = new GridLength(100);
            ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(100);
            ColumnDefinition col3 = new ColumnDefinition();
            col3.Width = new GridLength(100);
            ColumnDefinition col4 = new ColumnDefinition();
            col4.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition col5 = new ColumnDefinition();
            col5.Width = new GridLength(100);
            info.ColumnDefinitions.Add(col);
            info.ColumnDefinitions.Add(col2);
            info.ColumnDefinitions.Add(col3);
            info.ColumnDefinitions.Add(col4);
            info.ColumnDefinitions.Add(col5);

            Grid.SetColumn(currentTimeText, 0);
            info.Children.Add(currentTimeText);
            Grid.SetColumn(endTimeText, 1);
            info.Children.Add(endTimeText);
            Grid.SetColumn(cursorTimeText, 2);
            info.Children.Add(cursorTimeText);
            Grid.SetColumn(TimelineScaleText, 4);
            info.Children.Add(TimelineScaleText);
            
            updateInfo(scene, e);
        }

        //set rendered timeline info data
        internal void updateInfo(Scene scene, MouseEventArgs e)
        {
            TimelineScaleText.Text = (MainWindow.main.scaleToScreen(1000) / (1000 * zoomStandard)) * 100 + "%";
            updateTimeInfo(scene, e);
        }

        public void updateTimeInfo(Scene scene, MouseEventArgs e) {
            if (e != null)
            {
                double Xpos = e.GetPosition(MainWindow.main.LaneGrid).X;
                if (Xpos < 0) {
                    Xpos = 0;
                }
                cursorTime = "ct: " + TimelineTimeConverter.formatTime(MainWindow.main.scaleToTime(Xpos));
            }
            cursorTimeText.Text = cursorTime;
            currentTimeText.Text = "st: " + TimelineTimeConverter.formatTime(scene.elapsedMilliseconds);
            endTimeText.Text = "rst: " + TimelineTimeConverter.formatTime((scene.duration - scene.elapsedMilliseconds < scene.duration) ? scene.duration - scene.elapsedMilliseconds : 0);
            currentTimeText.Text = "st: " + TimelineTimeConverter.formatTime(scene.elapsedMilliseconds);
        }
        public void updateTimeInfoWithWheel(double zoomValue)
        {
            String value = zoomValue.ToString()+"%";
            double newZoom;
            if (MainWindow.main.sceneList.Scenes.Count > 0)
            {
                Scene scene = MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying];
                if (value.Equals("Fit to Screen"))
                {
                    if (MainWindow.main.ShowView)
                    {
                        zoomToShow(MainWindow.main.sceneList, MainWindow.main.Timeline);
                    }
                    else
                    {
                        zoomToShow(scene, MainWindow.main.Timeline);
                    }
                    MainWindow.main.DrawTimeline();
                    MainWindow.main.scrollTimelineToTime(scene.elapsedMilliseconds, scene);
                }
                else if (double.TryParse(value.TrimEnd('%'), out newZoom))
                {
                    newZoom /= 100;
                    newZoom *= zoomStandard;
                    Console.WriteLine("New Zoom = " + value + " -> " + newZoom + "  old " + zoomStandard);
                    TimelineScale = newZoom;
                    MainWindow.main.DrawTimeline();
                    MainWindow.main.scrollTimelineToTime(scene.elapsedMilliseconds, scene);
                }
            }

        }
        //get zoom amount from menu and change zoom
        private void updateZoom(object sender, EventArgs e)
        {
            if (e is KeyEventArgs) {
                if ((e as KeyEventArgs).Key == Key.Enter)
                {
                }
                else {
                    return;
                }
            }
            if (sender is ComboBox) {
                ComboBox box = sender as ComboBox;
                String value = box.Text as String;
                double newZoom;
                if (MainWindow.main.sceneList.Scenes.Count > 0)
                {
                    Scene scene = MainWindow.main.sceneList.Scenes[MainWindow.main.sceneList.currentScenePlaying];
                    if (value.Equals("Fit to Screen"))
                    {
                        if (MainWindow.main.ShowView)
                        {
                            zoomToShow(MainWindow.main.sceneList, MainWindow.main.Timeline);
                        }
                        else
                        {
                            zoomToShow(scene, MainWindow.main.Timeline);
                        }
                        MainWindow.main.DrawTimeline();
                        MainWindow.main.scrollTimelineToTime(scene.elapsedMilliseconds, scene);
                    }
                    else if (double.TryParse(value.TrimEnd('%'), out newZoom))
                    {
                        newZoom /= 100;
                        newZoom *= zoomStandard;
                        Console.WriteLine("New Zoom = " + value + " -> " + newZoom + "  old " + zoomStandard);
                        TimelineScale = newZoom;
                        MainWindow.main.DrawTimeline();
                        MainWindow.main.scrollTimelineToTime(scene.elapsedMilliseconds, scene);
                    }
                }
            }
        }


        public void zoomToShow(Scene scene, ScrollViewer Timeline) {
            if (scene.duration > 0)
            {
                double showDuration = scene.duration * showRatio;
                double percent = ((showDuration * (Timeline.ViewportWidth / showDuration)) / (showDuration * zoomStandard)) * 100;
                percent = Math.Floor(percent * 100) / 100;
                percent /= 100;
                percent *= zoomStandard;
                TimelineScale = percent;
                updateInfo(scene, null);
            }
        }

        public void zoomToShow(SceneList show, ScrollViewer Timeline)
        {
            if (show.GetDuration() > 0)
            {
                double showDuration = show.GetDuration() * ShowViewRatio;
                double percent = ((showDuration * (Timeline.ViewportWidth / showDuration)) / (showDuration * zoomStandard)) * 100;
                percent = Math.Floor(percent * 100) / 100;
                percent /= 100;
                percent *= zoomStandard;
                TimelineScale = percent;
                updateInfo(show.Scenes[show.currentScenePlaying], null);
            }
        }

        public double getZoomToShowVal(SceneList show, ScrollViewer Timeline)
        {
            if (show.GetDuration() > 0)
            {
                double showDuration = show.GetDuration() * ShowViewRatio;
                double percent = ((showDuration * (Timeline.ViewportWidth / showDuration)) / (showDuration * zoomStandard)) * 100;
                percent = Math.Floor(percent * 100) / 100;
                percent /= 100;
                percent *= (1000 * zoomStandard);
                return percent * 2;
            }
            else
            {
                return 0;
            }
        }

        //clear cursor location
        internal void clearCursorLocation()
        {
            cursorTime = "ct: ––:––:––.–––";
        }

    }
}
