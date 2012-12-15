using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace KinectRecorder
{

    [Serializable()]
    public class VideoDepthPair
    {
        private ImageFrame _videoImageFrame;

        public ImageFrame VideoImageFrame
        {
            get { return _videoImageFrame; }
            set { _videoImageFrame = value; }
        }
        private ImageFrame _depthImageFrame;

        public ImageFrame DepthImageFrame
        {
            get { return _depthImageFrame; }
            set { _depthImageFrame = value; }
        }

        public VideoDepthPair(ImageFrame video, ImageFrame depth)
        {
            _videoImageFrame = video;
            _depthImageFrame = depth;
        }
    }

    [Serializable()]
    public class VideoDepthSkeletonTuple : VideoDepthPair
    {
        private SkeletonFrame _skeletonFrame;

        public SkeletonFrame SkeletonFrame
        {
            get { return _skeletonFrame; }
            set { _skeletonFrame = value; }
        }

        public VideoDepthSkeletonTuple(ImageFrame video, ImageFrame depth, SkeletonFrame skeletonFrame)
            : base(video, depth)
        {
            this._skeletonFrame = skeletonFrame;
        }
    }

    public sealed class SerializableSkeletonFrame : ISerializationSurrogate
    {
        public SerializableSkeletonFrame()
        { }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            SkeletonFrame sf = (SkeletonFrame)(obj);

            info.AddValue("FrameNumber", sf.FrameNumber);
            info.AddValue("Quality", sf.Quality, typeof(SkeletonFrameQuality));
            info.AddValue("TimeStamp", sf.TimeStamp);

            info.AddValue("FloorClipPlane", sf.FloorClipPlane, typeof(Vector));
            info.AddValue("NormalToGravity", sf.NormalToGravity, typeof(Vector));

            info.AddValue("Skeletons", sf.Skeletons, typeof(SkeletonData[]));

        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SerializableVector : ISerializationSurrogate
    {
        public SerializableVector()
        { }



        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector v = (Vector)(obj);

            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
            info.AddValue("W", v.W);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Vector v = (Vector)(obj);
            v.X = info.GetSingle("X");
            v.Y = info.GetSingle("Y");
            v.Z = info.GetSingle("Z");
            v.W = info.GetSingle("W");
            return v;
        }
    }

    public sealed class SerializableSkeletonData : ISerializationSurrogate
    {
        public SerializableSkeletonData()
        { }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            SkeletonData sd = (SkeletonData)(obj);

            info.AddValue("Position", sd.Position);
            info.AddValue("Quality",sd.Quality,typeof(SkeletonQuality));
            info.AddValue("TrackingID",sd.TrackingID);
            info.AddValue("TrackingState",sd.TrackingState,typeof(SkeletonTrackingState));
            info.AddValue("UserIndex",sd.UserIndex);

            info.AddValue("Joints", sd.Joints, typeof(JointsCollection));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            SkeletonData sd = (SkeletonData)(obj);

            sd.Position = (Vector)info.GetValue("Position", typeof(Vector));
            sd.Quality = (SkeletonQuality)info.GetValue("Quality", typeof(SkeletonQuality));
            sd.TrackingID = info.GetInt32("TrackingID");
            sd.TrackingState = (SkeletonTrackingState)info.GetValue("TrackingState", typeof(SkeletonTrackingState));
            sd.UserIndex = info.GetInt32("UserIndex");

            sd.Joints = (JointsCollection)info.GetValue("Joints", typeof(JointsCollection));

            return sd;
        }
    }

    public sealed class SerializableJointsCollection : ISerializationSurrogate
    {
        public SerializableJointsCollection()
        { }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            JointsCollection jc = (JointsCollection)obj;
            info.AddValue("Count", jc.Count);

            int i = 0;
            foreach (Joint j in jc)
            {
                info.AddValue(i.ToString(), j, typeof(Joint));
                i++;
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            JointsCollection jc = (JointsCollection)obj;

            int count = info.GetInt32("Count");

            for (int i = 0; i < count; i++)
            {
                Joint j = (Joint)info.GetValue(i.ToString(), typeof(Joint));
                jc[j.ID] = j;
            }

            return jc;
        }
    }

    public sealed class SerializableJoint : ISerializationSurrogate
    {
        public SerializableJoint()
        { }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Joint j = (Joint)obj;

            info.AddValue("JointID", j.ID, typeof(JointID));
            info.AddValue("Position", j.Position, typeof(Vector));
            info.AddValue("TrackingState",j.TrackingState,typeof(JointTrackingState);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Joint j = (Joint)obj;

            j.ID = (JointID)info.GetValue("JointID", typeof(JointID));
            j.Position = (Vector)info.GetValue("Position", typeof(Vector));
            j.TrackingState = (JointTrackingState)info.GetValue("TrackingState", typeof(JointTrackingState));

            return j;
        }
    }

    public sealed class SerializableImageFrame : ISerializationSurrogate
    {
        public SerializableImageFrame()
        { }

        void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            ImageFrame _imageFrame = (ImageFrame)obj;

            info.AddValue("FrameNumber", _imageFrame.FrameNumber);
            info.AddValue("Image", _imageFrame.Image);
            info.AddValue("Resolution", _imageFrame.Resolution);
            info.AddValue("Timestamp", _imageFrame.Timestamp);
            info.AddValue("Type", _imageFrame.Type);
            info.AddValue("ViewArea", _imageFrame.ViewArea);
        }

        Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            ImageFrame _imageFrame = (ImageFrame)obj;
            _imageFrame.FrameNumber = info.GetInt32("FrameNumber");
            _imageFrame.Image = (PlanarImage)info.GetValue("Image", typeof(PlanarImage));
            _imageFrame.Resolution = (ImageResolution)info.GetValue("Resolution", typeof(ImageResolution));
            _imageFrame.Timestamp = info.GetInt64("Timestamp");
            _imageFrame.Type = (ImageType)info.GetValue("Type", typeof(ImageType));
            _imageFrame.ViewArea = (ImageViewArea)info.GetValue("ViewArea", typeof(ImageViewArea));

            return _imageFrame;
        }
    }

    public sealed class SerializablePlanarImage : ISerializationSurrogate
    {
        bool _compressVideo = true;

        public bool CompressVideo
        {
            get { return _compressVideo; }
            set { _compressVideo = value; }
        }

        public SerializablePlanarImage(bool compress = true)
        {
            _compressVideo = compress;
        }

        void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            PlanarImage pi = (PlanarImage)obj;

            info.AddValue("__CompressedVideo", _compressVideo);

            info.AddValue("BytesPerPixel", pi.BytesPerPixel);
            info.AddValue("Height", pi.Height);
            info.AddValue("Width", pi.Width);

            // If we're dealing with a video image
            if (pi.BytesPerPixel == 4 && CompressVideo)
            {
                // Compress the image
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 65;
                BitmapSource image = BitmapSource.Create(pi.Width, pi.Height, 72, 72, PixelFormats.Bgr32, null, pi.Bits, pi.Width * 4);
                MemoryStream imageBytes = new MemoryStream(200 * 1024);
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(imageBytes);
                info.AddValue("Bits", imageBytes.ToArray(), typeof(byte[]));
            }
            else
            {
                info.AddValue("Bits", pi.Bits, typeof(byte[]));
            }
        }

        Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            PlanarImage pi = (PlanarImage)obj;

            bool Decompress = info.GetBoolean("__CompressedVideo");

            pi.BytesPerPixel = info.GetInt32("BytesPerPixel");
            pi.Height = info.GetInt32("Height");
            pi.Width = info.GetInt32("Width");

            // If we're dealing with a video image
            if (pi.BytesPerPixel == 4 && Decompress)
            {
                // Decompress the image
                MemoryStream imageStreamSource = new MemoryStream((byte[])info.GetValue("Bits", typeof(byte[])));
                JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                BitmapSource bitmapSource = decoder.Frames[0];
                pi.Bits = new byte[pi.Width*pi.Height*pi.BytesPerPixel];
                bitmapSource.CopyPixels(pi.Bits, pi.Width * 4, 0);
                imageStreamSource.Close();
            }
            else
            {
                pi.Bits = (byte[])info.GetValue("Bits", typeof(byte[]));
            }

            return pi;
        }
    }

    public sealed class SerializableImageViewArea : ISerializationSurrogate
    {

        public SerializableImageViewArea()
        { }

        void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            ImageViewArea iva = (ImageViewArea)obj;

            info.AddValue("CenterX", iva.CenterX);
            info.AddValue("CenterY", iva.CenterY);
            info.AddValue("Zoom", iva.Zoom);
        }

        Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            ImageViewArea iva = (ImageViewArea)obj;
            iva.CenterX = info.GetInt32("CenterX");
            iva.CenterY = info.GetInt32("CenterY");
            iva.Zoom = (ImageDigitalZoom)info.GetValue("Zoom", typeof(ImageDigitalZoom));

            return iva;
        }
    }

    public class Serialization
    {
        public static ISurrogateSelector SurrogateSelector
        {
            get
            {
                SurrogateSelector ss = new SurrogateSelector();
                StreamingContext sc = new StreamingContext(StreamingContextStates.All);
                
                ss.AddSurrogate(typeof(ImageFrame), sc, new SerializableImageFrame());
                ss.AddSurrogate(typeof(PlanarImage), sc, new SerializablePlanarImage());
                ss.AddSurrogate(typeof(ImageViewArea), sc, new SerializableImageViewArea());
                ss.AddSurrogate(typeof(Vector), sc, new SerializableVector());
                ss.AddSurrogate(typeof(Joint), sc, new SerializableJoint());
                ss.AddSurrogate(typeof(JointsCollection), sc, new SerializableJointsCollection());
                ss.AddSurrogate(typeof(SkeletonData), sc, new SerializableSkeletonData());
                ss.AddSurrogate(typeof(SkeletonFrame), sc, new SerializableSkeletonFrame());

                return ss;
            }
        }
    }
}
