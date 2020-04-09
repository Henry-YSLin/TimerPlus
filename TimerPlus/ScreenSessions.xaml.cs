using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace TimerPlus
{
    /// <summary>
    /// Interaction logic for ScreenSessions.xaml
    /// </summary>
    public partial class ScreenSessions : UserControl, INotifyPropertyChanged
    {
        public delegate void SessionStartHandler(ScreenSessions sender, string sessionTypeId);
        public event SessionStartHandler SessionStart;

        public SessionType EditSession = null;

        public ScreenSessions()
        {
            InitializeComponent();
            Helper.HideBoundingBox(screen);
            listSessionType.ItemsSource = SavedState.Data.SessionTypes;
        }

        private void expanderNewSession_Expanded(object sender, RoutedEventArgs e)
        {
            ShadowAssist.SetShadowDepth(expanderBase, ShadowDepth.Depth2);
            NewSessionNameValidationRule.CheckDuplicate = EditSession == null;
            if (EditSession == null)
            {
                NewSessionName = "";
                timeNewSession.SelectedTime = null;
            }
        }

        private void expanderNewSession_Collapsed(object sender, RoutedEventArgs e)
        {
            ShadowAssist.SetShadowDepth(expanderBase, ShadowDepth.Depth0);
            if (EditSession != null)
            {
                EditSession = null;
                NewSessionName = "";
                timeNewSession.SelectedTime = null;
                expanderNewSession.Header = "New Session Type";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private string newSessionName;
        public string NewSessionName
        {
            get
            {
                return newSessionName;
            }
            set
            {
                newSessionName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewSessionName)));
            }
        }

        private void btnNewSession_Click(object sender, RoutedEventArgs e)
        {
            NewSessionNameValidationRule.CheckDuplicate = EditSession == null;
            if (!NewSessionNameValidationRule.StaticValidate(newSessionName, CultureInfo.InvariantCulture).IsValid)
            {
                txtNewSessionName.Focus();
                txtNewSessionName.SelectAll();
                Keyboard.Focus(txtNewSessionName);
                return;
            }
            if (timeNewSession.SelectedTime == null && !checkCountUp.IsChecked.GetValueOrDefault())
            {
                timeNewSession.Focus();
                Keyboard.Focus(timeNewSession);
                return;
            }
            if (EditSession == null)
            {
                string id;
                do
                {
                    id = Helper.RandomString(10);
                } while (SavedState.Data.SessionTypes.Select(x => x.Id).Contains(id));
                if (checkCountUp.IsChecked.GetValueOrDefault())
                {
                    SavedState.Data.SessionTypes.Add(new SessionType(id, txtNewSessionName.Text, TimeSpan.Zero, true));
                }
                else
                {
                    SavedState.Data.SessionTypes.Add(new SessionType(id, txtNewSessionName.Text, timeNewSession.SelectedTime.Value - DateTime.Today, false));
                }
                SavedState.Save();
                expanderNewSession.IsExpanded = false;
                snackbar.MessageQueue.Enqueue("New session type created");
            }
            else
            {
                EditSession.Name = txtNewSessionName.Text;
                EditSession.CountUp = checkCountUp.IsChecked.GetValueOrDefault();
                if (EditSession.CountUp)
                {
                    EditSession.Time = TimeSpan.Zero;
                }
                else
                {
                    EditSession.Time = timeNewSession.SelectedTime.Value - DateTime.Today;
                }
                SavedState.Save();
                foreach (DaySummary daySummary in SavedState.Data.DaySummaries)
                {
                    if (daySummary.Records.Select(x => x.TypeId).Contains(EditSession.Id))
                    {
                        daySummary.NotifyStatsChanged();
                    }
                }
                expanderNewSession.IsExpanded = false;
                expanderNewSession.Header = "New Session Type";
                snackbar.MessageQueue.Enqueue("Session type saved");
                EditSession = null;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (SessionType sType in SavedState.Data.SessionTypes)
            {
                sType.FilterVisible = Fastenshtein.AutoCompleteLevenshtein.Distance(txtSearch.Text.ToLower(), sType.Name.ToLower()) < 3;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            SessionType type = listSessionType.SelectedItem as SessionType;
            if (type == null) return;
            SavedState.Data.CurrentSession = new Session() { TypeId = type.Id };
            SavedState.Save();
            SessionStart?.Invoke(this, type.Id);
        }

        private void listSessionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SessionType type = listSessionType.SelectedItem as SessionType;
            btnStart.IsEnabled = type != null;
            btnEdit.IsEnabled = type != null;
            btnDelete.IsEnabled = type != null;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            SessionType type = listSessionType.SelectedItem as SessionType;
            if (type == null) return;
            EditSession = type;
            expanderNewSession.Header = "Edit Session Type";
            expanderNewSession.IsExpanded = true;
            txtNewSessionName.Text = type.Name;
            timeNewSession.SelectedTime = DateTime.Today + type.Time;
            checkCountUp.IsChecked = type.CountUp;
            NewSessionNameValidationRule.CheckDuplicate = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            SessionType type = listSessionType.SelectedItem as SessionType;
            if (type == null) return;
            if (SavedState.Data.SessionRecords.Select(x => x.TypeId).Contains(type.Id))
            {
                snackbar.MessageQueue.Enqueue("Cannot delete a session type that is in use");
                return;
            }
            SavedState.Data.SessionTypes.Remove(type);
            SavedState.Save();
            snackbar.MessageQueue.Enqueue("Session type deleted");
        }

        private void checkCountUp_Checked(object sender, RoutedEventArgs e)
        {
            timeNewSession.IsEnabled = false;
            timeNewSession.SelectedTime = null;
        }

        private void checkCountUp_Unchecked(object sender, RoutedEventArgs e)
        {
            timeNewSession.IsEnabled = true;
        }
    }

    public class NewSessionNameValidationRule : ValidationRule
    {
        public static bool CheckDuplicate = true;

        public static ValidationResult StaticValidate(object value, CultureInfo cultureInfo)
        {
            string name = Convert.ToString(value);
            if (name.IsEmpty())
            {
                return new ValidationResult(false, "Name cannot be empty");
            }
            else
            {
                if (SavedState.Data.SessionTypes.Select(x => x.Name).Contains(name) && CheckDuplicate)
                {
                    return new ValidationResult(false, "Duplicate name");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return StaticValidate(value, cultureInfo);
        }
    }
}
