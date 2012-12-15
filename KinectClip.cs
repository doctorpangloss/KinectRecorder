using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;
using Microsoft.Kinect;
using MayaCacheIO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace KinectRecorder
{
    public enum FPS
    {
        FPS12 = 500,
        FPS30 = 200,
    }

    public class KinectClip : IEnumerable<KinectFrame>
    {
        public BinaryFormatter BinaryFormatter
        { get; set; }


        public KinectSensor Sensor
        {
            get
            {
                return KinectSensor.KinectSensors[SensorInstanceID];
            }
        }

        public string SensorInstanceID
        { get; set; }

        Stream _stream;

        bool _legacy = false;

        public Stream BaseStream
        {
            get { return _stream; }
            set { _stream = value; }
        }

        Cache _cache;

        public Cache Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public KinectClip(string clipPath, FileAccess access = FileAccess.ReadWrite)
        {
            // Initialize surrogate selector stuff
            BinaryFormatter = new BinaryFormatter();
            _stream = new FileStream(clipPath, FileMode.OpenOrCreate, access, FileShare.ReadWrite);
        }

        public void ToCache(string cacheFile, FPS fps = FPS.FPS12, bool onlyActors = false, bool legacy = false)
        {
            _cache = new Cache();
            _cache.TimePerFrame = (int)fps;
            _cache.BaseFileName = Path.GetFileNameWithoutExtension(cacheFile);
            _cache.Directory = Path.GetDirectoryName(cacheFile);
            _cache.CacheType = Cache.OneFilePerFrameCacheType;

            _legacy = legacy;

            int frameNumber = 1;

            ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);

            foreach (KinectFrame kf in this)
            {
                // TEMPORARY
                if (frameNumber > 360)
                    break;
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessKinectFrameAndWrite), new ProcessStruct(kf, frameNumber, onlyActors));
                ProcessKinectFrameAndWrite(new ProcessStruct(kf, frameNumber, onlyActors));
                frameNumber++;
            }
        }

        public class ProcessStruct
        {
            public KinectFrame kf;
            public int frameNumber;
            public bool onlyActors;

            public ProcessStruct(KinectFrame _vdp, int _frameNumber, bool _onlyActors)
            {
                kf = _vdp;
                frameNumber = _frameNumber;
                onlyActors = _onlyActors;
            }
        }

        private void ProcessKinectFrameAndWrite(Object state)
        {
            ProcessStruct input = (ProcessStruct)state;
            nCacheFile file = new nCacheFile();
            file.FrameNumber = input.frameNumber;

            int depthWidth = input.kf.DepthWidth;
            int depthHeight = input.kf.DepthHeight;

            byte[] pixels = input.kf.ColorPixelData;
            
            int colorWidth = input.kf.ColorWidth;
            int colorHeight = input.kf.ColorHeight;

            Console.WriteLine("Processing frame {0}...", input.frameNumber);

            // set up image channels for rescaling later
            Image<Rgb, float> skeleton = new Image<Rgb, float>(depthWidth, depthHeight, new Rgb(0,0,0));

            // set up actor mask
            Image<Gray, byte> actorMask = new Image<Gray, byte>(depthWidth, depthHeight, new Gray(0));

            // set up color
            //Image<Rgb, double> colors = new Image<Rgb, double>(colorWidth, colorHeight, new Rgb(0, 0, 0));

            //ColorImagePoint[] colorImagePoints = new ColorImagePoint[depthWidth * depthHeight];

            // color ratio
            //int colorRatio = colorWidth / depthWidth;

            // valid pixels
            short[] validPixels = input.kf.DepthPixelData.Where(n => n > 0 && (n & DepthImageFrame.PlayerIndexBitmask) > 0).ToArray();
            short average = 0;
            if (validPixels.Length > 0)
            {
                average = (short)(validPixels.Select(n => (double)n).Average());
            }



            // fill images
            for (int depth_y = 0; depth_y < depthHeight; depth_y++)
            {
                for (int depth_x = 0; depth_x < depthWidth; depth_x++)
                {
                    int depthIndex = depth_x + depth_y * depthWidth;
                    short depthAndIndex = input.kf.DepthPixelData[depthIndex];
                    byte player = (byte)(depthAndIndex & DepthImageFrame.PlayerIndexBitmask);

                    if (depthAndIndex <= 0)
                    {
                        depthAndIndex = average;
                    }

                    //// put in colors
                    //ColorImagePoint color = Sensor.MapDepthToColorImagePoint(input.kf.DepthFormat,depth_x,depth_y,depthAndIndex,input.kf.ColorFormat);

                    //if ( input.kf.ColorFormat == ColorImageFormat.RgbResolution1280x960Fps12)
                    //{
                    //    for (int p = 0; p < 2; p++)
                    //    {
                    //        for (int q = 0; q < 2; q++)
                    //        {
                    //            int colorIndex = ((color.Y + q) * colorWidth + (color.X + p)) * 4;
                    //            colors[depth_y*colorRatio + q, depth_x*colorRatio + p] = new Rgb(pixels[2 + colorIndex] * .004d, pixels[1 + colorIndex] * .004d, pixels[0 + colorIndex] * .004d);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    int colorIndex = ((color.Y) * colorWidth + (color.X)) * 4;
                    //    colors[depth_y*colorRatio, depth_x*colorRatio] = new Rgb(pixels[2 + colorIndex] * .004d, pixels[1 + colorIndex] * .004d, pixels[0 + colorIndex] * .004d);
                    //}
                    
                    actorMask[depth_y, depth_x] = new Gray(player);

                    SkeletonPoint pos = Sensor.MapDepthToSkeletonPoint(input.kf.DepthFormat, depth_x, depth_y, depthAndIndex);
                    skeleton[depth_y, depth_x] = new Rgb(pos.X, pos.Y, pos.Z);
                }
            }

            // smooth skeleton z
            Image<Gray, float> skeletonZ = skeleton[2];
            skeletonZ._SmoothGaussian(3,3,.045,.045);
            skeleton[2] = skeletonZ;

            // resize images
            actorMask = actorMask.Resize(colorWidth, colorHeight, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
            skeleton = skeleton.Resize(colorWidth, colorHeight, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            // conversion to particle format
            for (int y = 0; y < colorHeight; y++)
            {
                for (int x = 0; x < colorWidth; x++)
                {
                    if (((actorMask[y, x].Intensity > 0) && input.onlyActors) || !input.onlyActors)
                    {
                        Rgb skeletonColor = skeleton[y,x];

                        SkeletonPoint sp = new SkeletonPoint(){X = (float)skeletonColor.Red, Y = (float)skeletonColor.Green, Z=(float)skeletonColor.Blue};

                        // get color
                        //Rgb color = colors[y, x];

                        ColorImagePoint colorPoint = Sensor.MapSkeletonPointToColor(sp, input.kf.ColorFormat);

                        int colorIndex = ((colorPoint.Y + (y % 2)) * colorWidth + (colorPoint.X + (x % 2))) * 4;
                        Rgb color;
                        if (colorIndex < pixels.Length && colorIndex >= 0)
                        {
                            color = new Rgb(pixels[2 + colorIndex] * .004d, pixels[1 + colorIndex] * .004d, pixels[0 + colorIndex] * .004d);
                        }
                        else
                        {
                            color = new Rgb(0, 0, 0);
                        }

                        if (_legacy)
                        {
                            file.Position.Write(skeletonColor.Red, skeletonColor.Green, skeletonColor.Blue);
                            file.Rgb.Write(color.Red, color.Green, color.Blue);
                        }
                        else
                        {
                            file.Position.Write((float)skeletonColor.Red, (float)skeletonColor.Green, (float)skeletonColor.Blue);
                            file.Rgb.Write((float)color.Red, (float)color.Green, (float)color.Blue);
                        }
                    }
                }
            }

            //// convert to ncache format
            //for (int depth_y = 0; depth_y < depthHeight; depth_y++)
            //{
            //    for (int depth_x = 0; depth_x < depthWidth; depth_x++)
            //    {

            //        short depthAndIndex = input.kf.DepthPixelData[depth_x+depth_y*depthWidth];
            //        int player = depthAndIndex & DepthImageFrame.PlayerIndexBitmask;
            //        short depth = (short)(depthAndIndex >> DepthImageFrame.PlayerIndexBitmaskWidth);

            //        if (input.onlyActors && player == 0)
            //        {
            //            depth = 0;
            //        }

            //        if (depth != 0)
            //        {
            //            // get position
            //            SkeletonPoint pos = Sensor.MapDepthToSkeletonPoint(input.kf.DepthFormat, depth_x, depth_y, depthAndIndex);

            //            // get color
            //            ColorImagePoint color = Sensor.MapDepthToColorImagePoint(input.kf.DepthFormat, depth_x, depth_y, depthAndIndex, input.kf.ColorFormat);

            //            int colorIndex = (color.Y*colorWidth+color.X)*4;

            //            // save the particle rgbPP and position values to byte streams valid to be copied directly into a nCache file
            //            if (_legacy)
            //            {
            //                file.Position.Write((double)pos.X, (double)pos.Y, (double)pos.Z);
            //                file.Rgb.Write((double)pixels[2 + colorIndex]*0.004d, (double)pixels[1 + colorIndex]*0.004d, (double)pixels[0 + colorIndex]*0.004d);
            //            }
            //            else
            //            {
            //                file.Position.Write(pos.X, pos.Y, pos.Z);
            //                file.Rgb.Write(pixels[2 + colorIndex], pixels[1 + colorIndex], pixels[0 + colorIndex]);
            //            }
            //        }
            //    }
            //}

            // save to file
            _cache.WriteCacheFile(file.Channels, input.frameNumber, _legacy);
        }

        public void Close()
        {
            try
            {
                BaseStream.Flush();
                BaseStream.Close();
            }
            catch (Exception e)
            { }
        }

        public void Flush()
        {
            BaseStream.Flush();
        }

        public void Append(KinectFrame kf)
        {
            BaseStream.Seek(0, SeekOrigin.End);
            BinaryFormatter.Serialize(BaseStream, kf);
        }

        public IEnumerator<KinectFrame> GetEnumerator()
        {
            return new ClipEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ClipEnumerator(this);
        }

        public class ClipEnumerator : IEnumerator<KinectFrame>
        {
            KinectClip _clip;
            KinectFrame _current;

            public ClipEnumerator(KinectClip clip)
            {
                _clip = clip;
                _clip.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            public KinectFrame Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                _clip.Close();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_clip.BaseStream.Position < _clip.BaseStream.Length)
                {
                    try
                    {
                        _current = (KinectFrame)_clip.BinaryFormatter.Deserialize(_clip.BaseStream);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                _clip.BaseStream.Seek(0, SeekOrigin.Begin);
                _current = null;
            }
        }
    }
}
