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
using Ookii.Dialogs.Wpf;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Kinect;
using System.Xml.Serialization;
using Coding4Fun.Kinect.Wpf;
using Coding4Fun.Kinect.Wpf.Controls;

namespace KinectRecorder
{
    /// <summary>
    /// Interaction logic for VideoRecorderControl.xaml
    /// </summary>
    public partial class VideoRecorderControl : UserControl
    {
        public VideoRecorderControl()
        {
            InitializeComponent();

            CreateNewSession();
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);

            DataContext = CurrentSession;



            if (kinectCombox.Items.Count > 0)
            {
                kinectCombox.SelectedIndex = 0;
            }
        }

        void CreateNewSession()
        {
            string[] actors = { "Bed", "Anna", "Ben" };
            CurrentSession = new KinectSession() {Ready=false, CurrentDirectory="C:\\Recording", CurrentActor = "Bed", CurrentNote="", Recording=false, CurrentScene = 1, CurrentTake = 1, Actors = new ObservableCollection<string>(actors) };
            foreach (KinectSensor k in KinectSensor.KinectSensors)
            {
                UpdateStatus(k);
            }
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            UpdateStatus(e.Sensor);
        }

        void UpdateStatus(KinectSensor k)
        {
            Debug.WriteLine(Enum.GetName(typeof(KinectStatus), k.Status));
            switch (k.Status)
            {
                case KinectStatus.Connected:
                case KinectStatus.DeviceNotGenuine:
                case KinectStatus.DeviceNotSupported:
                    if (!CurrentSession.KinectConnectionIDs.Contains(k.DeviceConnectionId))
                    {
                        CurrentSession.KinectConnectionIDs.Add(k.DeviceConnectionId);
                    }
                    break;
                case KinectStatus.InsufficientBandwidth:
                case KinectStatus.NotPowered:
                case KinectStatus.Error:
                case KinectStatus.Disconnected:
                case KinectStatus.Undefined:
                case KinectStatus.NotReady:
                    if (CurrentSession.KinectConnectionIDs.Contains(k.DeviceConnectionId))
                    {
                        CurrentSession.KinectConnectionIDs.Remove(k.DeviceConnectionId);
                    }
                    break;
            }
        }

        private void onButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)kinectCombox.SelectedItem != null)
            {
                Sensor = KinectSensor.KinectSensors[(string)kinectCombox.SelectedItem];

                try
                {
                    switch (qualityCombox.SelectedIndex)
                    {
                        case 0:
                            Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                            break;
                        case 1:
                            Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                            break;
                        case 2:
                            throw new NotImplementedException();
                    }

                    Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                }
                catch (Exception ee)
                {
                    System.Windows.MessageBox.Show("Error: " + ee.Message);
                    return;
                }

                Sensor.SkeletonStream.Enable();

                try
                {
                    Sensor.Start();
                }
                catch (Exception SkeletonEngineException)
                {
                    Sensor.SkeletonStream.Disable();
                    Debug.WriteLine("Skeleton engine already in use.");
                }
                finally
                {
                    Sensor.Start();
                }
                
                Sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(Sensor_AllFramesReady);
                CurrentSession.Ready = true;
            }
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame cif = e.OpenColorImageFrame();
            DepthImageFrame dif = e.OpenDepthImageFrame();
            SkeletonFrame sf = e.OpenSkeletonFrame();

            bool process = false;

            if (cif != null && dif != null)
            {
                colorImage.Source = cif.ToBitmapSource();
                depthImage.Source = dif.ToBitmapSource();

                process = true;
            }

            if (process)
            {
                if (Recording)
                {
                    CurrentFile.Append(KinectFrame.FromFrames(cif, dif, sf));
                    CurrentSession.CurrentClip.EndFrame += 1;
                }

                cif.Dispose();
                dif.Dispose();

                if (Sensor.SkeletonStream.IsEnabled)
                {
                    sf.Dispose();
                }
            }
        }

        private void SerializeVideoDepthPair(KinectFrame vdp)
        {
            CurrentFile.Append(vdp);
        }

        public void StartRecording(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentClip = new ClipMeta() { StartFrame=(CurrentSession.CurrentClip != null ? CurrentSession.CurrentClip.EndFrame+1 : 0), Note=CurrentSession.CurrentNote,  Actor = CurrentSession.CurrentActor, Directory=CurrentSession.CurrentDirectory, Take=CurrentSession.CurrentTake, Scene=CurrentSession.CurrentScene, KinectConnectionID = CurrentSession.CurrentKinectConnectionID };
            CurrentFile = new KinectClip(CurrentSession.CurrentClip.DefaultFilePath);
            CurrentSession.Recording = true;
        }

        private void SaveSession()
        {
            XmlSerializer x = new XmlSerializer(typeof(KinectSession));
            x.Serialize(new FileStream("session.xml", FileMode.Create), CurrentSession);
        }

        public void StopRecording(object sender, RoutedEventArgs e)
        {
            CurrentSession.Recording = false;
            if (CurrentFile != null)
            {
                CurrentFile.Close();
                CurrentSession.CurrentTake += 1;
            }
            //SaveSession();
        }

        private void offButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording(sender,e);
            Sensor.Stop();
            CurrentSession.Ready = false;
        }

        private void ChooseClipDirectory(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            Nullable<bool> r = fbd.ShowDialog();
            if (r.Value)
            {
                CurrentSession.CurrentDirectory = fbd.SelectedPath;
            }
        }

        private void OpenSession(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog ofp = new VistaOpenFileDialog();
            ofp.Filter = "Session Files (*.xml)|*.xml";

            Nullable<bool> r = ofp.ShowDialog();
            if (r.HasValue && r.Value)
            {
                XmlSerializer x = new XmlSerializer(typeof(KinectSession));
                CurrentSession = (KinectSession)x.Deserialize(new FileStream(ofp.FileName,FileMode.Open));
            }
        }

        private void NewSession(object sender, RoutedEventArgs e)
        {
            CreateNewSession();
        }

    }
}
