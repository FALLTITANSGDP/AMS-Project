using System.Drawing;
using System.Drawing.Imaging;

namespace AMS.Services
{
    public static class BitmapExtensionBase
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}