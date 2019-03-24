using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DmitryBrant.ImageFormats
{
    public static class XpmReader
    {
        public static Bitmap Load(string fileName)
        {
            Bitmap result = null;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = Load(fileStream);
            }
            return result;
        }

        public static Bitmap Load(Stream stream)
        {
            var num = -1;
            var num2 = -1;
            var dictionary = new Dictionary<string, uint>();
            var text = ReadUntil(stream, '"');
            text = ReadUntil(stream, '"');
            var array = text.Split(whitespacequote, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length < 4)
            {
                throw new ApplicationException("Invalid file format.");
            }
            num = Convert.ToInt32(array[0]);
            num2 = Convert.ToInt32(array[1]);
            var num3 = Convert.ToInt32(array[2]);
            var num4 = Convert.ToInt32(array[3]);
            if (num <= 0 || num2 <= 0 || num3 <= 0 || num4 <= 0)
            {
                throw new ApplicationException("Invalid image dimensions.");
            }
            for (var i = 0; i < num3; i++)
            {
                text = ReadUntil(stream, '"');
                text = ReadUntil(stream, '"');
                var key = text.Substring(0, num4);
                var array2 = text.Split(whitespacequote, StringSplitOptions.RemoveEmptyEntries);
                var text2 = array2[array2.Length - 1];
                uint num5;
                if (text2.ToLower().Contains("none"))
                {
                    num5 = 0u;
                }
                else if (text2.StartsWith("#"))
                {
                    text2 = text2.Replace("#", "");
                    var num6 = Convert.ToUInt64(text2, 16);
                    if (text2.Length > 6)
                    {
                        num5 = 0xFF000000;
                        num5 |= (UInt32)((num6 & 0xFF0000000000) >> 24);
                        num5 |= (UInt32)((num6 & 0xFF000000) >> 16);
                        num5 |= (UInt32)((num6 & 0xFF00) >> 8);
                    }
                    else
                    {
                        num5 = (UInt32)num6 | 0xFF000000;
                    }
                }
                else
                {
                    num5 = (uint)Color.FromName(text2).ToArgb();
                }
                dictionary.Add(key, num5);
            }
            var num7 = num * num2;
            var num8 = 0;
            var num9 = num7 * 4;
            var array3 = new byte[num9];
            try
            {
                while (stream.Position < stream.Length)
                {
                    text = ReadUntil(stream, '"');
                    text = ReadUntil(stream, '"');
                    var j = 0;
                    while (j < text.Length - 1)
                    {
                        var num5 = dictionary[text.Substring(j, num4)];
                        j += num4;
                        array3[num8++] = (byte)(num5 & 255u);
                        array3[num8++] = (byte)((num5 & 65280u) >> 8);
                        array3[num8++] = (byte)((num5 & 16711680u) >> 16);
                        array3[num8++] = (byte)((num5 & 4278190080u) >> 24);
                        if (num8 >= num9)
                        {
                            break;
                        }
                    }
                    if (num8 >= num9)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num, num2, PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(array3, 0, bitmapData.Scan0, array3.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static string ReadLine(Stream stream)
        {
            var result = "";
            var array = new byte[1024];
            var num = (int)stream.Position;
            stream.Read(array, 0, 1024);
            int i;
            for (i = 0; i < 1024; i++)
            {
                if (array[i] == 13 || array[i] == 10)
                {
                    i++;
                    break;
                }
            }
            if (i > 1)
            {
                result = Encoding.ASCII.GetString(array, 0, i - 1);
            }
            stream.Position = (long)(num + i);
            return result;
        }

        private static string ReadUntil(Stream stream, char stopChar)
        {
            var text = "";
            var num = 0;
            while (stream.Position < stream.Length)
            {
                var c = (char)stream.ReadByte();
                text += c.ToString();
                if (c == stopChar)
                {
                    break;
                }
                num++;
                if (num > 4096)
                {
                    break;
                }
            }
            return text;
        }

        private static char[] whitespace = new char[]
        {
            ' ',
            '\t',
            '\r',
            '\n'
        };

        private static char[] whitespacequote = new char[]
        {
            ' ',
            '\t',
            '\r',
            '\n',
            '"'
        };
    }
}