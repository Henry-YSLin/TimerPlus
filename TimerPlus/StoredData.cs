using System;
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
        private TimeSpan time;
        private bool filterVisible = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get => id; set { id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id))); } }

        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }

        [XmlElement("Time")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public long TimeTick { get { return Time.Ticks; } set { Time = new TimeSpan(value); } }

        [XmlIgnore]
        public TimeSpan Time { get => time; set { time = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time))); } }

        [XmlIgnore]
        public bool FilterVisible { get => filterVisible; set { filterVisible = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterVisible))); } } 

        [XmlIgnore]
        public int Count
        {
            get
            {
                return SavedState.Data.SessionRecords.Count(x => x.TypeId == id);
            }
        }

        public void NotifyCountChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public SessionType() { }

        public SessionType(string _id, string _name, TimeSpan _time)
        {
            Id = _id;
            Name = _name;
            Time = _time;
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

        [XmlElement(DataType = "dateTime")]
        public DateTime PauseTime { get; set; } = DateTime.MinValue;

        [XmlElement(DataType = "dateTime")]
        public DateTime StartTime { get; set; } = DateTime.MinValue;

        [XmlElement(DataType = "dateTime")]
        public DateTime EndTime { get; set; } = DateTime.MinValue;
    }

    [Serializable]
    public class StoredData
    {
        public ObservableCollection<SessionType> SessionTypes { get; set; } = new ObservableCollection<SessionType>();

        public ObservableCollection<Session> SessionRecords { get; set; } = new ObservableCollection<Session>();

        public Session CurrentSession { get; set; } = null;

        public StoredData()
        {
            SessionRecords.CollectionChanged += SessionRecords_CollectionChanged;
        }

        private void SessionRecords_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (SessionType type in SessionTypes)
            {
                type.NotifyCountChanged();
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
