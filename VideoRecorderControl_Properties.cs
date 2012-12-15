using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;
using Microsoft.Kinect;

namespace KinectRecorder
{
    public partial class VideoRecorderControl : UserControl
    {
        public static readonly DependencyProperty KinectSessionProperty = DependencyProperty.Register("KinectSession", typeof(KinectSession), typeof(VideoRecorderControl), new UIPropertyMetadata(null));

        KinectSensor _sensor;

        string _clipName = "default.clip";

        public string ClipName
        {
            get { return _clipName; }
            set { _clipName = value; }
        }

        public KinectSensor Sensor
        {
            get { return _sensor; }
            set { _sensor = value; }
        }

        bool _recording = false;

        public bool Recording
        {
            get { return CurrentSession.Recording; }
            set { CurrentSession.Recording = value; }
        }


        public KinectSession CurrentSession
        {
            get
            {
                return (KinectSession)GetValue(KinectSessionProperty);
            }
            set
            {
                SetValue(KinectSessionProperty, value);
            }
        }


        public KinectClip CurrentFile
        { get; set; }

        ImageFrame _lastDepthImageFrame;

        public ImageFrame LastDepthImageFrame
        {
            get { return _lastDepthImageFrame; }
            set { _lastDepthImageFrame = value; }
        }

    }
}
