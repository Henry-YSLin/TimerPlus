using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ScreenStatistics.xaml
    /// </summary>
    public partial class ScreenStatistics : UserControl, INotifyPropertyChanged
    {
        public ScreenStatistics()
        {
            InitializeComponent();
            Helper.HideBoundingBox(screen);
            listSessionRecords.ItemsSource = SavedState.Data.SessionRecords;
            listSessionType.ItemsSource = SavedState.Data.SessionTypes;
            listDaySummaries.ItemsSource = SavedState.Data.DaySummaries;
            lblCurrentMonth.DataContext = SavedState.Data;
            btnPrevMonth.DataContext = SavedState.Data;
            btnNextMonth.DataContext = SavedState.Data;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            if (SavedState.Data.HasPrevMonth)
            {
                SavedState.Data.CurrentMonth = SavedState.Data.CurrentMonth.AddMonths(-1);
            } 
        }

        private void btnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            if (SavedState.Data.HasNextMonth)
            {
                SavedState.Data.CurrentMonth = SavedState.Data.CurrentMonth.AddMonths(1);
            }
        }
    }
}
