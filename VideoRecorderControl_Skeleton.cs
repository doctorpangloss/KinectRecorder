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
        //Dictionary<JointID, Brush> jointColors = new Dictionary<JointID, Brush>() { 
        //    {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
        //    {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
        //    {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
        //    {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
        //    {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
        //    {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
        //    {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
        //    {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
        //    {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
        //    {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
        //    {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
        //    {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
        //    {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
        //    {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
        //    {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
        //    {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
        //    {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
        //    {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
        //    {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
        //    {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        //};

        //private Point getDisplayPosition(Joint joint)
        //{
        //    float depthX, depthY;
        //    Sensor.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
        //    return new Point((int)(skeletonCanvas.Width * depthX), (int)(skeletonCanvas.Height * depthY));
        //}

        //Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
        //{
        //    PointCollection points = new PointCollection(ids.Length);
        //    for (int i = 0; i < ids.Length; ++i)
        //    {
        //        points.Add(getDisplayPosition(joints[ids[i]]));
        //    }

        //    Polyline polyline = new Polyline();
        //    polyline.Points = points;
        //    polyline.Stroke = brush;
        //    polyline.StrokeThickness = 5;
        //    return polyline;
        //}

        //void Nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        //{
        //    SkeletonFrame skeletonFrame = e.SkeletonFrame;
        //    int iSkeleton = 0;
        //    Brush[] brushes = new Brush[6];
        //    brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        //    brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        //    brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
        //    brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
        //    brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
        //    brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

        //    skeletonCanvas.Children.Clear();
        //    foreach (SkeletonData data in skeletonFrame.Skeletons)
        //    {
        //        if (SkeletonTrackingState.Tracked == data.TrackingState)
        //        {
        //            // Draw bones
        //            Brush brush = brushes[iSkeleton % brushes.Length];
        //            skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
        //            skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
        //            skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
        //            skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
        //            skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

        //            // Draw joints
        //            foreach (Joint joint in data.Joints)
        //            {
        //                Point jointPos = getDisplayPosition(joint);
        //                Line jointLine = new Line();
        //                jointLine.X1 = jointPos.X - 3;
        //                jointLine.X2 = jointLine.X1 + 6;
        //                jointLine.Y1 = jointLine.Y2 = jointPos.Y;
        //                jointLine.Stroke = jointColors[joint.ID];
        //                jointLine.StrokeThickness = 6;
        //                skeletonCanvas.Children.Add(jointLine);
        //            }
        //        }
        //        iSkeleton++;
        //    } // for each skeleton
        //}

    }
}
