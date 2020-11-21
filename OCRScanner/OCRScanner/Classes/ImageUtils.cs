using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OCRScanner.Classes
{
    public static class ImageUtils
    {
        /// <summary>
        /// Create an image from a filepath
        /// </summary>
        /// <param name="filePath">Location of the image you want to load</param>
        /// <returns>A mat object that contains the image</returns>
        public static Mat ReadInImage(string filePath)
        {
            return CvInvoke.Imread(filePath, ImreadModes.AnyColor);
        }

        //Convert a bitmap to imagesource
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        /// Converts a bitmap to an image source
        /// </summary>
        /// <param name="bmp">The bitmap image to be converted</param>
        /// <returns>A converted image source</returns>
        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}