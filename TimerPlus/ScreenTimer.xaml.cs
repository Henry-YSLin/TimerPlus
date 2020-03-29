using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TimerPlus
{
    /// <summary>
    /// Interaction logic for ScreenTimer.xaml
    /// </summary>
    public partial class ScreenTimer : UserControl
    {
        public delegate void SessionEndHandler(ScreenTimer sender, Session session);
        public event SessionEndHandler SessionEnd;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        public ScreenTimer()
        {
            InitializeComponent();
            Helper.HideBoundingBox(screen);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateControls();
        }

        public bool Paused;

        public void UpdateControls()
        {
            Session s = SavedState.Data.CurrentSession;
            if (s == null) return;
            SessionType sType = SavedState.Data.SessionTypes.First(x => x.Id == s.TypeId);
            TimeSpan timeRemaining;
            if (s.Paused)
            {
                iconPlayPause.Kind = PackIconKind.PlayOutline;
                iconPlayPause.Margin = new Thickness(5, 0, 0, 0);
                timeRemaining = sType.Time - s.TimeElapsed;
            }
            else
            {
                iconPlayPause.Kind = PackIconKind.Pause;
                iconPlayPause.Margin = new Thickness(0);
                timeRemaining = sType.Time - s.TimeElapsed - stopwatch.Elapsed;
            }
            if (timeRemaining.TotalSeconds < -5)
            {
                lblOvertime.Visibility = Visibility.Visible;
            }
            else
            {
                lblOvertime.Visibility = Visibility.Collapsed;
            }
            if (timeRemaining.TotalSeconds > 0)
            {
                lblTimerHour.Text = ((int)Math.Floor(timeRemaining.TotalHours)).ToString("00");
                lblTimerMinute.Text = timeRemaining.Minutes.ToString("00");
                lblTimerSecond.Text = timeRemaining.Seconds.ToString("00");
                ButtonProgressAssist.SetMaximum(btnPlayPause, sType.Time.TotalSeconds);
                ButtonProgressAssist.SetValue(btnPlayPause, timeRemaining.TotalSeconds);
            }
            else if (timeRemaining.TotalSeconds < -5)
            {
                timeRemaining = -timeRemaining;
                lblTimerHour.Text = ((int)Math.Floor(timeRemaining.TotalHours)).ToString("00");
                lblTimerMinute.Text = timeRemaining.Minutes.ToString("00");
                lblTimerSecond.Text = timeRemaining.Seconds.ToString("00");
                ButtonProgressAssist.SetMaximum(btnPlayPause, sType.Time.TotalSeconds);
                ButtonProgressAssist.SetValue(btnPlayPause, 0);
            }
            else
            {
                lblTimerHour.Text = "00";
                lblTimerMinute.Text = "00";
                lblTimerSecond.Text = "00";
                ButtonProgressAssist.SetMaximum(btnPlayPause, 5);
                ButtonProgressAssist.SetValue(btnPlayPause, -timeRemaining.TotalSeconds);
            }
            if (lblTimerHour.Text == "00")
            {
                lblTimerHour.Opacity = 0.5;
                if (lblTimerMinute.Text == "00")
                {
                    lblTimerMinute.Opacity = 0.5;
                    if (lblTimerSecond.Text == "00")
                    {
                        lblTimerSecond.Opacity = 0.5;
                    }
                    else
                    {
                        lblTimerSecond.Opacity = 1;
                    }
                }
                else
                {
                    lblTimerMinute.Opacity = 1;
                    lblTimerSecond.Opacity = 1;
                }
            }
            else
            {
                lblTimerHour.Opacity = 1;
                lblTimerMinute.Opacity = 1;
                lblTimerSecond.Opacity = 1;
            }
            lblColon1.Opacity = lblTimerHour.Opacity;
            lblColon2.Opacity = lblTimerMinute.Opacity;
            lblSessionName.Text = sType.Name;
        }

        public void PlayPause()
        {
            Session s = SavedState.Data.CurrentSession;
            DateTime now = DateTime.Now;
            if (s.Paused)
            {
                if (s.StartTime == DateTime.MinValue)
                {
                    s.StartTime = now;
                }
                s.Paused = false;
                dispatcherTimer.Start();
                stopwatch.Reset();
                s.PauseTime = now;
                stopwatch.Start();
            }
            else
            {
                stopwatch.Stop();
                dispatcherTimer.Stop();
                s.Paused = true;
                s.TimeElapsed += stopwatch.Elapsed;
                s.PauseTime = now;
                UpdateControls();
                SavedState.Save();
            }
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        private void btnSaveSession_Click(object sender, RoutedEventArgs e)
        {
            Session s = SavedState.Data.CurrentSession;
            DateTime now = DateTime.Now;
            if (!s.Paused)
            {
                PlayPause();
            }
            s.EndTime = s.PauseTime;
            SavedState.Data.SessionRecords.Add(s);
            SavedState.Data.CurrentSession = null;
            SavedState.Save();
            SessionEnd?.Invoke(this, s);
        }

        private void DialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter == null) return;
            if ((bool)eventArgs.Parameter)
            {
                Session s = SavedState.Data.CurrentSession;
                SavedState.Data.CurrentSession = null;
                SavedState.Save();
                SessionEnd?.Invoke(this, s);
            }
        }

        private void screen_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                PlayPause();
        }
    }
}
