using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ETMPClient.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage? LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static byte[]? TagLibPictureToBytes(TagLib.IPicture picture)
        {
            if (picture == null) return null;
            return picture.Data.Data;
        }
    }
}

