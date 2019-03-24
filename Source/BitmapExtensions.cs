using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DmitryBrant.ImageFormats
{
    public static class BitmapExtensions
    {
        public static Bitmap Load(string fileName)
        {
            Bitmap bitmap;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap = Load(fileStream);
                if (bitmap != null)
                {
                    return bitmap;
                }
                var extension = Path.GetExtension(fileName);
                var text = (extension != null) ? extension.ToLowerInvariant() : null;
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }
                if (text.EndsWith("tga"))
                {
                    bitmap = TgaReader.Load(fileStream);
                }
                else if (text.EndsWith("cut"))
                {
                    bitmap = CutReader.Load(fileStream);
                }
                else if (text.EndsWith("sgi") || text.EndsWith("rgb") || text.EndsWith("bw"))
                {
                    bitmap = SgiReader.Load(fileStream);
                }
                else if (text.EndsWith("xpm"))
                {
                    bitmap = XpmReader.Load(fileStream);
                }
            }
            return bitmap;
        }

        public static Bitmap Load(Stream stream)
        {
            Bitmap result = null;
            var array = new byte[256];
            stream.Read(array, 0, array.Length);
            stream.Seek(0L, SeekOrigin.Begin);
            if (array[0] == 10 && array[1] >= 3 && array[1] <= 5 && array[2] == 1 && array[4] == 0 && array[5] == 0)
            {
                result = PcxReader.Load(stream);
            }
            else if (array[0] == 80 && array[1] >= 49 && array[1] <= 54 && (array[2] == 10 || array[2] == 13))
            {
                result = PnmReader.Load(stream);
            }
            else if (array[0] == 89 && array[1] == 166 && array[2] == 106 && array[3] == 149)
            {
                result = RasReader.Load(stream);
            }
            else if (array[128] == 68 && array[129] == 73 && array[130] == 67 && array[131] == 77)
            {
                result = DicomReader.Load(stream);
            }
            return result;
        }

        public static byte[] ToBytes(this Bitmap bitmap, ImageFormat imageFormat)
        {
            byte[] result;
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, imageFormat);
                result = memoryStream.ToArray();
            }
            return result;
        }
    }
}