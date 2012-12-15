using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using Ookii.Dialogs.Wpf;
using System.Text.RegularExpressions;

namespace KinectRecorder
{
    /// <summary>
    /// Interaction logic for ParticleProcessorControl.xaml
    /// </summary>
    public partial class ParticleProcessorControl : UserControl, INotifyPropertyChanged
    {
        KinectClip _currentClip;

        public KinectClip CurrentClip
        {
            get { return _currentClip; }
            set
            {
                _currentClip = value;
            }
        }

        string _clipName;

        public string ClipName
        {
            get { return _clipName; }
            set { _clipName = value; }
        }


        public ParticleProcessorControl()
        {
            InitializeComponent();
            DataContext = this;
            Legacy = false;
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofp = new OpenFileDialog();
            ofp.Filter = "All Files (*.*)|*.*";

            Nullable<bool> result = ofp.ShowDialog();
            if (result == true)
            {
                ClipName = ofp.FileName;
                CurrentClip = new KinectClip(ClipName);

                saveButton.IsEnabled = true;
                processButton.IsEnabled = true;
            }
        }


        string _fileName;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML files (*.xml)|*.xml;PDC files (*.pdc)|*.pdc;All files (*.*)|*.*";
            sfd.CheckFileExists = false;

            Nullable<bool> result = sfd.ShowDialog();
            if (result == true)
            {
                FileName = sfd.FileName;
            }
        }

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (KinectFrame kf in CurrentClip)
            //{
            //    BitmapSource bs = kf.ColorPixelData.ToBitmapSource(kf.ColorWidth,kf.ColorHeight);
            //    BitmapSource dp = kf.DepthPixelData.ToBitmapSource(kf.DepthWidth, kf.DepthHeight, 0, Color.FromRgb((byte)255, (byte)0, (byte)0));
            //    this.colorImage.Source = bs;
            //    bs.Save(string.Format("X:\\Recording\\test{0}.png",kf.ColorFrameNumber),ImageFormat.Png);
            //    this.depthImage.Source = dp;
            //    dp.Save(string.Format("X:\\Recording\\test_d{0}.png", kf.ColorFrameNumber), ImageFormat.Png);
            //}
            CurrentClip = new KinectClip(ClipName);
            Thread t = new Thread(new ThreadStart(BeginProcessing));
            t.Start();
        }

        void BeginProcessing()
        {
            CurrentClip.SensorInstanceID = KinectSensor.KinectSensors[0].DeviceConnectionId;
            try
            {
                CurrentClip.Sensor.Start();
            }
            catch { }
            CurrentClip.ToCache(FileName,onlyActors:OnlyActors,legacy:Legacy);
        }

        public bool OnlyActors
        { get; set; }

        public bool Legacy
        { get; set; }

        public string BatchOpenDirectory
        { get; set; }

        public string BatchSaveDirectory
        { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void openButton_Click_1(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            Nullable<bool> result = fbd.ShowDialog();
            if (result.Value)
            {
                BatchOpenDirectory = fbd.SelectedPath;
            }
        }

        private void saveButton_Click_1(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            Nullable<bool> result = fbd.ShowDialog();
            if (result.Value)
            {
                BatchSaveDirectory = fbd.SelectedPath;
            }
        }

        struct WorkParams
        {
            public IEnumerable<string> inFiles;
            public string saveDir;
            public bool onlyActors;
        }

        private void processButton_Click_1(object sender, RoutedEventArgs e)
        {
            //enumerate clips
            IEnumerable<string> clipPaths = Directory.EnumerateFiles(BatchOpenDirectory, "*.clip");


            progressBar.Maximum = clipPaths.Count();
            progressBar.Value = 0;


            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.WorkerReportsProgress = true;
            bw.RunWorkerAsync(new WorkParams() {inFiles = clipPaths, saveDir=BatchSaveDirectory, onlyActors = OnlyActors});
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = progressBar.Maximum * e.ProgressPercentage / 100;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            WorkParams wp = (WorkParams)e.Argument;
            IEnumerable<string> files = wp.inFiles;
            int c = files.Count();
            Regex r = new Regex(@"S(\d+)_(\w+)_T(\d+).*.clip");
            int i = 0;
            foreach (string path in files)
            {
                i++;
                string outPath = Path.Combine(wp.saveDir, r.Replace(Path.GetFileName(path), @"S$1_$2_T$3.xml"));

                Console.Write("Processing {0}", outPath);
                CurrentClip = new KinectClip(path);
                CurrentClip.SensorInstanceID = KinectSensor.KinectSensors[0].DeviceConnectionId;
                try
                {
                    CurrentClip.Sensor.Start();
                }
                catch { }
                CurrentClip.ToCache(outPath, onlyActors: wp.onlyActors, legacy: true);
                //try
                //{
                //    CurrentClip.ToCache(outPath, onlyActors: true, legacy: true);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
                bw.ReportProgress((int)((float)i / (float)c * 100));
            }
        }
    }
}
