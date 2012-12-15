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

namespace KinectRecorder
{
    /// <summary>
    /// Interaction logic for MultiRecorderControl.xaml
    /// </summary>
    public partial class MultiRecorderControl : UserControl, INotifyPropertyChanged
    {
        public MultiRecorderControl()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            recorder1.StartRecording(sender, e);
            recorder2.StartRecording(sender, e);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            recorder1.StopRecording(sender, e);
            recorder2.StopRecording(sender, e);
        }

        public bool ReadyAndNotRecording
        {
            get
            {
                return recorder1.CurrentSession.Ready && recorder2.CurrentSession.Ready && !recorder1.CurrentSession.Recording && !recorder2.CurrentSession.Recording;
            }
        }

        public bool ReadyAndRecording
        {
            get
            {
                return recorder1.CurrentSession.Ready && recorder2.CurrentSession.Ready && recorder1.CurrentSession.Recording && recorder2.CurrentSession.Recording;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
