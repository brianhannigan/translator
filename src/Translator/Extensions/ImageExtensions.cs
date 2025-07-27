using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Translator.Extensions
{
    public static class ImageExtensions
    {
        public static BitmapImage ToBitmapImage(this Image bitmap)
        {
            try
            {
                if (bitmap != null)
                {
                    var ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    return bi;
                }
            }
            catch
            {

            }

            return null;
        }

        public static BitmapImage ToBitmapImage(this byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                if (bi.CanFreeze)
                {
                    bi.Freeze();
                }
                return bi;
            }
            return null;
        }

        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    Bitmap bmp = new Bitmap(image);
                    bmp.Save(ms, format);
                }
                catch { }
                return ms.ToArray();
            }
        }

        public static Bitmap ToBitmap(this BitmapSource bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
