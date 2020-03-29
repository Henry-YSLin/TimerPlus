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
            listSessionType.ItemsSource = SavedState.Data.SessionTypes;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
