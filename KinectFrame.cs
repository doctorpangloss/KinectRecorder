using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Kinect;
using Kaliko.ImageLibrary;
using Coding4Fun.Kinect.Wpf;

namespace KinectRecorder
{
    [Serializable()]
    public class KinectFrame
    {
        private Skeleton[] _skeletons;

        public Skeleton[] Skeletons
        {
            get { return _skeletons; }
        }

        private short[] _depthPixelData;

        public short[] DepthPixelData
        {
            get { return _depthPixelData; }
        }

        private byte[] _colorPixelData;

        [NonSerialized]
        private byte[] _cachedDecompressedPixelData;

        public byte[] ColorPixelData
        {
            get
            {
                if (Compressed)
                {
                    if (_cachedDecompressedPixelData == null)
                    {
                        MemoryStream imageStreamSource = new MemoryStream(_colorPixelData);
                        KalikoImage ki = new KalikoImage(imageStreamSource);
                        _cachedDecompressedPixelData = ki.ByteArray;
                        imageStreamSource.Close();

                        //JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.None, BitmapCacheOption.None);
                        //BitmapSource bitmapSource = decoder.Frames[0];
                        //bitmapSource.Save(String.Format("X:\\Recording\\bsm{0}.png",ColorFrameNumber),ImageFormat.Png);
                        //_cachedDecompressedPixelData = new byte[ColorWidth * ColorHeight * 4];
                        //bitmapSource.CopyPixels(_cachedDecompressedPixelData, bitmapSource.PixelWidth * 4, 0);
                        //imageStreamSource.Close();
                    }
                    return _cachedDecompressedPixelData;
                }
                else
                {
                    return _colorPixelData;
                }
            }
        }

        public int ColorWidth
        {
            get
            {
                switch (this.ColorFormat)
                {
                    case Microsoft.Kinect.ColorImageFormat.RawYuvResolution640x480Fps15:
                    case Microsoft.Kinect.ColorImageFormat.RgbResolution640x480Fps30:
                    case Microsoft.Kinect.ColorImageFormat.YuvResolution640x480Fps15:
                        return 640;
                    case Microsoft.Kinect.ColorImageFormat.RgbResolution1280x960Fps12:
                        return 1280;
                    case Microsoft.Kinect.ColorImageFormat.Undefined:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public int ColorHeight
        {
            get
            {
                switch (this.ColorFormat)
                {
                    case Microsoft.Kinect.ColorImageFormat.RawYuvResolution640x480Fps15:
                    case Microsoft.Kinect.ColorImageFormat.RgbResolution640x480Fps30:
                    case Microsoft.Kinect.ColorImageFormat.YuvResolution640x480Fps15:
                        return 480;
                    case Microsoft.Kinect.ColorImageFormat.RgbResolution1280x960Fps12:
                        return 960;
                    case Microsoft.Kinect.ColorImageFormat.Undefined:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public int DepthWidth
        {
            get
            {
                switch (this.DepthFormat)
                {
                    case DepthImageFormat.Resolution640x480Fps30:
                        return 640;
                    case DepthImageFormat.Resolution320x240Fps30:
                        return 1280;
                    case DepthImageFormat.Resolution80x60Fps30:
                        return 80;
                    case DepthImageFormat.Undefined:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public int DepthHeight
        {
            get
            {
                switch (this.DepthFormat)
                {
                    case DepthImageFormat.Resolution640x480Fps30:
                        return 480;
                    case DepthImageFormat.Resolution320x240Fps30:
                        return 240;
                    case DepthImageFormat.Resolution80x60Fps30:
                        return 60;
                    case DepthImageFormat.Undefined:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public ColorImageFormat ColorFormat
        { get; set; }

        public DepthImageFormat DepthFormat
        { get; set; }

        public int ColorFrameNumber
        { get; set; }

        public int DepthFrameNumber
        { get; set; }

        public long ColorTimestamp
        { get; set; }

        public long DepthTimestamp
        { get; set; }

        public bool Compressed
        { get; set; }

        protected KinectFrame()
        {
        }

        public static KinectFrame FromFrames(ColorImageFrame colorImageFrame, DepthImageFrame depthImageFrame, SkeletonFrame skeletonFrame, bool compress = true)
        {
            KinectFrame kf = new KinectFrame();
            kf.Compressed = compress;

            if (colorImageFrame != null)
            {
                if (kf.Compressed)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.QualityLevel = 85;
                    BitmapSource image = colorImageFrame.ToBitmapSource();
                    MemoryStream imageBytes = new MemoryStream(200 * 1024);
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(imageBytes);
                    kf._colorPixelData = imageBytes.ToArray();
                    imageBytes.Close();
                }
                else
                {
                    kf._colorPixelData = new byte[colorImageFrame.PixelDataLength];
                    colorImageFrame.CopyPixelDataTo(kf._colorPixelData);
                }
                kf.ColorFormat = colorImageFrame.Format;
                kf.ColorFrameNumber = colorImageFrame.FrameNumber;
                kf.ColorTimestamp = colorImageFrame.Timestamp;
            }

            if (depthImageFrame != null)
            {
                kf._depthPixelData = new short[depthImageFrame.PixelDataLength];
                depthImageFrame.CopyPixelDataTo(kf._depthPixelData);
                kf.DepthFormat = depthImageFrame.Format;
                kf.DepthFrameNumber = depthImageFrame.FrameNumber;
                kf.DepthTimestamp = depthImageFrame.Timestamp;
            }

            if (skeletonFrame != null)
            {
                kf._skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(kf._skeletons);
            }

            return kf;
        }
    }
}
