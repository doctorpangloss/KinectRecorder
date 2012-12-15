using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace KinectRecorder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)]
        public UInt32 ColorBGRA;
        // 8 bit components
        [FieldOffset(0)]
        public byte Blue;
        [FieldOffset(1)]
        public byte Green;
        [FieldOffset(2)]
        public byte Red;
        [FieldOffset(3)]
        public byte Alpha;
    }

    //public static class BitmapSourceHelper
    //{
    //    public static PixelColor[,] GetPixels(this BitmapSource source)
    //    {
    //        if (source.Format != PixelFormats.Bgra32)
    //            source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

    //        int width = source.PixelWidth;
    //        int height = source.PixelHeight;
    //        PixelColor[,] result = new PixelColor[width, height];

    //        source.CopyPixels(result, width * 4, 0);
    //        return result;
    //    }

    //    public unsafe static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
    //    {
    //        fixed (PixelColor* buffer = &pixels[0, 0])
    //            source.CopyPixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), (IntPtr)(buffer + offset), pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor), stride);
    //    }
    //}
}
