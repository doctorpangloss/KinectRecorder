using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.IO;

namespace KinectRecorder
{
    [Serializable()]
    [XmlRoot("KinectSession")]
    public class KinectSession : INotifyPropertyChanged
    {
        public bool Recording
        { get; set; }

        public bool NotRecording
        {
            get
            {
                return !Recording;
            }
        }

        public bool Ready
        { get; set; }

        public string CurrentNote
        { get; set; }

        public string CurrentDirectory
        { get; set; }

        public int CurrentScene
        { get; set; }

        public string CurrentActor
        { get; set; }

        public int CurrentTake
        { get; set; }

        public string CurrentKinectConnectionID
        { get; set; }

        public ObservableCollection<String> KinectConnectionIDs
        { get; set; }

        public ObservableCollection<String> KinectUniqueIDs
        { get; set; }

        public ObservableCollection<String> Actors
        { get; set; }

        public ClipMeta CurrentClip
        {
            set
            {
                Clips.Insert(0, value);
            }
            get
            {
                if (Clips.Count == 0)
                {
                    return null;
                }
                else
                {
                    return Clips[0];
                }
            }
        }

        public ObservableCollection<ClipMeta> Clips
        { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public KinectSession()
        {
            KinectConnectionIDs = new ObservableCollection<string>();
            KinectUniqueIDs = new ObservableCollection<string>();
            Actors = new ObservableCollection<string>();
            Clips = new ObservableCollection<ClipMeta>();
        }
    }

    [Serializable()]
    public class ClipMeta : INotifyPropertyChanged
    {
        public string Directory
        { get; set; }

        public int Scene
        { get; set; }

        public string Actor
        { get; set; }

        public string Note
        { get; set; }

        public int Take
        { get; set; }

        public string KinectConnectionID
        { get; set; }

        public string KinectUniqueID
        { get; set; }

        public int StartFrame
        { get; set; }

        public int EndFrame
        { get; set; }

        [XmlAttribute("FileName")]
        public string DefaultFilePath
        {
            get
            {
                return Path.Combine(Directory, string.Format("S{0}_{1}_T{2}.clip", Scene, Note, Take));
            }
        }

        public ClipMeta()
        {
            Scene = 0;
            Actor = "";
            Note = "";
            Take = 0;
            KinectConnectionID = "";
            KinectUniqueID = "";
            StartFrame = 0;
            EndFrame = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
