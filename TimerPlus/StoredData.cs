﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TimerPlus
{
    [Serializable]
    public class SessionType : INotifyPropertyChanged
    {
        private string id;
        private string name;
        private bool countUp = false;
        private TimeSpan time;
        private bool filterVisible = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get => id; set { id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id))); } }

        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }

        public bool CountUp
        {
            get => countUp; set
            {
                countUp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountUp)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatsTime)));
            }
        }

        [XmlElement("Time")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public long TimeTick { get { return Time.Ticks; } set { Time = new TimeSpan(value); } }

        [XmlIgnore]
        public TimeSpan Time
        {
            get => time; set
            {
                time = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatsTime)));
            }
        }

        [XmlIgnore]
        public bool FilterVisible { get => filterVisible; set { filterVisible = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterVisible))); } }

        [XmlIgnore]
        public TimeSpan StatsTime
        {
            get
            {
                if (CountUp)
                {
                    if (SavedState.Data.SessionRecords.Where(x => x.TypeId == id).Count() > 0)
                    {
                        return TimeSpan.FromSeconds(SavedState.Data.SessionRecords.Where(x => x.TypeId == id).Average(x => x.TimeElapsed.TotalSeconds));
                    }
                    else
                    {
                        return TimeSpan.Zero;
                    }
                }
                else
                {
                    return Time;
                }
            }
        }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return SavedState.Data.SessionRecords.Count(x => x.TypeId == id);
            }
        }

        [XmlIgnore]
        public TimeSpan AverageTimeDiff
        {
            get
            {
                if (!CountUp && SavedState.Data.SessionRecords.Where(x => x.TypeId == id).Count() > 0)
                {
                    return TimeSpan.FromSeconds(SavedState.Data.SessionRecords.Where(x => x.TypeId == id).Average(x => Time.TotalSeconds - x.TimeElapsed.TotalSeconds));
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public void NotifyCountChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatsTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageTimeDiff)));
        }

        public SessionType() { }

        public SessionType(string _id, string _name, TimeSpan _time, bool _countUp)
        {
            Id = _id;
            Name = _name;
            Time = _time;
            CountUp = _countUp;
        }
    }

    [Serializable]
    public class Session
    {
        public string TypeId { get; set; }

        [XmlElement("TimeElapsed")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public long TimeElapsedTick { get { return TimeElapsed.Ticks; } set { TimeElapsed = new TimeSpan(value); } }
        [XmlIgnore]
        public TimeSpan TimeElapsed { get; set; } = TimeSpan.FromTicks(0);

        [XmlIgnore]
        public bool Paused { get; set; } = true;

        [XmlIgnore]
        public SessionType Type
        {
            get
            {
                return SavedState.Data.SessionTypes.FirstOrDefault(x => x.Id == TypeId);
            }
        }

        [XmlIgnore]
        public string TypeName
        {
            get
            {
                return Type.GetTypeName();
            }
        }

        [XmlIgnore]
        public bool TypeCountUp
        {
            get
            {
                SessionType sType = Type;
                if (sType != null) return sType.CountUp;
                else return true;
            }
        }

        [XmlIgnore]
        public TimeSpan TimeDiff
        {
            get
            {
                SessionType sType = Type;
                if (sType != null && !sType.CountUp)
                {
                    return sType.Time - TimeElapsed;
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        [XmlElement(DataType = "dateTime")]
        public DateTime PauseTime { get; set; } = DateTime.MinValue;

        [XmlElement(DataType = "dateTime")]
        public DateTime StartTime { get; set; } = DateTime.MinValue;

        [XmlElement(DataType = "dateTime")]
        public DateTime EndTime { get; set; } = DateTime.MinValue;
    }

    public class DaySummary : INotifyPropertyChanged
    {
        private DateTime date;

        public DateTime Date
        {
            get => date; set
            {
                date = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridRow)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridColumn)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInThePast)));
            }
        }

        public ObservableCollection<Session> Records { get; } = new ObservableCollection<Session>();

        public string SessionTypeListString
        {
            get
            {
                if (Records.Count == 0)
                    return "";
                else
                    return Records.GroupBy(x => x.TypeId)
                        .Select(g => SavedState.Data.SessionTypes.FirstOrDefault(x => x.Id == g.Key).GetTypeName() + (g.Count() > 1 ? " ×" + g.Count().ToString() : ""))
                        .Aggregate((x1, x2) => x1 + "\r\n" + x2);
            }
        }

        public TimeSpan TotalDuration
        {
            get
            {
                if (Records.Count == 0)
                    return TimeSpan.Zero;
                else
                    return Records.Select(x =>
                    {
                        var sType = SavedState.Data.SessionTypes.FirstOrDefault(y => y.Id == x.TypeId);
                        if (sType != null)
                            return sType.CountUp ? x.TimeElapsed : sType.Time;
                        else return x.TimeElapsed;
                    }).Aggregate((x1, x2) => x1 + x2);
            }
        }

        public TimeSpan TotalNetDuration
        {
            get
            {
                if (Records.Count == 0)
                    return TimeSpan.Zero;
                else
                    return Records.Select(x => x.TimeElapsed).Aggregate((x1, x2) => x1 + x2);
            }
        }

        public int GridRow
        {
            get
            {
                return Helper.GetWeekOfMonth(date) - 1;
            }
        }

        public int GridColumn
        {
            get
            {
                return (int)date.DayOfWeek;
            }
        }

        public bool IsVisible
        {
            get
            {
                return Date.FirstDayOfMonth() == SavedState.Data.CurrentMonth;
            }
        }

        public bool IsInThePast
        {
            get
            {
                return Date <= DateTime.Now;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DaySummary()
        {
            Records.CollectionChanged += Records_CollectionChanged;
        }

        public void NotifyVisibilityChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
        }

        private void Records_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyStatsChanged();
        }

        public void NotifyStatsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionTypeListString)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDuration)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalNetDuration)));
        }
    }

    [Serializable]
    public class StoredData : INotifyPropertyChanged
    {
        private DateTime currentMonth = DateTime.Today.FirstDayOfMonth();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SessionType> SessionTypes { get; set; } = new ObservableCollection<SessionType>();

        public ObservableCollection<Session> SessionRecords { get; set; } = new ObservableCollection<Session>();

        public Session CurrentSession { get; set; } = null;

        [XmlIgnore]
        public ObservableCollection<DaySummary> DaySummaries { get; } = new ObservableCollection<DaySummary>();

        [XmlIgnore]
        public DateTime CurrentMonth
        {
            get => currentMonth; set
            {
                currentMonth = value;
                UpdateCurrentMonth();
            }
        }

        [XmlIgnore]
        public bool HasNextMonth
        {
            get
            {
                List<DateTime> months = SessionRecords.Select(x => x.EndTime.FirstDayOfMonth()).Append(DateTime.Today.FirstDayOfMonth()).Distinct().ToList();
                return CurrentMonth != months.Max();
            }
        }

        [XmlIgnore]
        public bool HasPrevMonth
        {
            get
            {
                List<DateTime> months = SessionRecords.Select(x => x.EndTime.FirstDayOfMonth()).Append(DateTime.Today.FirstDayOfMonth()).Distinct().ToList();
                return CurrentMonth != months.Min();
            }
        }

        public StoredData()
        {
            SessionRecords.CollectionChanged += SessionRecords_CollectionChanged;
        }

        private void UpdateCurrentMonth()
        {
            DaySummaries.Clear();
            foreach (DateTime day in Helper.EachDay(CurrentMonth.FirstDayOfMonth(), CurrentMonth.LastDayOfMonth()))
            {
                DaySummary summary = new DaySummary();
                summary.Date = day;
                foreach (Session s in SessionRecords.Where(x => x.EndTime.Date == day))
                {
                    summary.Records.Add(s);
                }
                DaySummaries.Add(summary);
            }

            foreach (DaySummary summary in DaySummaries)
            {
                summary.NotifyVisibilityChanged();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMonth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasPrevMonth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNextMonth)));
        }

        private void SessionRecords_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (SessionType type in SessionTypes)
            {
                type.NotifyCountChanged();
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Session s in e.NewItems)
                {
                    if (s.EndTime >= DateTime.Today.FirstDayOfMonth() && s.EndTime < DateTime.Now)
                    {
                        DaySummary summary = DaySummaries.FirstOrDefault(x => x.Date == s.EndTime.Date);
                        if (summary == null)
                        {
                            summary = new DaySummary();
                            summary.Date = s.EndTime.Date;
                            summary.Records.Add(s);
                            DaySummaries.Add(summary);
                        }
                        else
                        {
                            summary.Records.Add(s);
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Session s in e.OldItems)
                {
                    DaySummary summary = DaySummaries.FirstOrDefault(x => x.Date == s.EndTime.Date);
                    if (summary != null)
                    {
                        summary.Records.Remove(s);
                        if (summary.Records.Count == 0)
                        {
                            DaySummaries.Remove(summary);
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (Session s in e.OldItems)
                {
                    DaySummary summary = DaySummaries.FirstOrDefault(x => x.Date == s.EndTime.Date);
                    if (summary != null)
                    {
                        summary.Records.Remove(s);
                        if (summary.Records.Count == 0)
                        {
                            DaySummaries.Remove(summary);
                        }
                    }
                }
                foreach (Session s in e.NewItems)
                {
                    if (s.EndTime >= DateTime.Today.FirstDayOfMonth() && s.EndTime < DateTime.Now)
                    {
                        DaySummary summary = DaySummaries.FirstOrDefault(x => x.Date == s.EndTime.Date);
                        if (summary == null)
                        {
                            summary = new DaySummary();
                            summary.Date = s.EndTime.Date;
                            summary.Records.Add(s);
                            DaySummaries.Add(summary);
                        }
                        else
                        {
                            summary.Records.Add(s);
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                DaySummaries.Clear();
            }

            DaySummaries.RemoveAll(x => x.Date.FirstDayOfMonth() != DateTime.Today.FirstDayOfMonth());

            foreach (DateTime day in Helper.EachDay(DateTime.Today.FirstDayOfMonth(), DateTime.Today.LastDayOfMonth()))
            {
                if (!DaySummaries.Select(x => x.Date).Contains(day))
                {
                    DaySummaries.Add(new DaySummary() { Date = day });
                }
            }
        }
    }

    public static class SavedState
    {
        public static StoredData Data { get; set; }

        static SavedState()
        {
            Load();
        }

        public static void Save()
        {
            string myPath = "data.xml".FromAppPath();
            XmlSerializer s = new XmlSerializer(typeof(StoredData));
            StreamWriter streamWriter = new StreamWriter(myPath);
            s.Serialize(streamWriter, Data);
            streamWriter.Close();
        }
        public static void Load()
        {
            string myPath = "data.xml".FromAppPath();
            StoredData loadedData = new StoredData();
            if (File.Exists(myPath))
            {
                XmlSerializer s = new XmlSerializer(typeof(StoredData));
                StreamReader streamReader = new StreamReader(myPath);
                loadedData = (StoredData)s.Deserialize(streamReader);
                streamReader.Close();
            }
            Data = loadedData;
        }
    }
}
