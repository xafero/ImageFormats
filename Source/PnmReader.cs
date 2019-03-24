using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DmitryBrant.ImageFormats
{
    public static class PnmReader
    {
        public static Bitmap Load(string fileName)
        {
            Bitmap result = null;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                result = Load(fileStream);
            }
            return result;
        }

        public static Bitmap Load(Stream stream)
        {
            var num = -1;
            var num2 = -1;
            var num3 = -1;
            if ((ushort)stream.ReadByte() != 80)
            {
                throw new ApplicationException("Incorrect file format.");
            }
            var c = (char)stream.ReadByte();
            if (c < '1' || c > '6')
            {
                throw new ApplicationException("Unrecognized bitmap type.");
            }
            if (c == '1' || c == '4')
            {
                num3 = 1;
            }
            while (stream.Position < stream.Length)
            {
                var text = ReadLine(stream);
                if (text.Length != 0 && text[0] != '#')
                {
                    var array = text.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length != 0)
                    {
                        for (var i = 0; i < array.Length; i++)
                        {
                            if (num == -1)
                            {
                                num = Convert.ToInt32(array[i]);
                            }
                            else if (num2 == -1)
                            {
                                num2 = Convert.ToInt32(array[i]);
                            }
                            else if (num3 == -1)
                            {
                                num3 = Convert.ToInt32(array[i]);
                            }
                        }
                        if (num != -1 && num2 != -1 && num3 != -1)
                        {
                            break;
                        }
                    }
                }
            }
            if (num <= 0 || num2 <= 0 || num3 <= 0)
            {
                throw new ApplicationException("Invalid image dimensions.");
            }
            var num4 = num * num2;
            var num5 = num4 * 4;
            var array2 = new byte[num5];
            try
            {
                if (c == '1')
                {
                    var num6 = 0;
                    while (stream.Position < stream.Length)
                    {
                        var text = ReadLine(stream);
                        if (text.Length != 0 && text[0] != '#')
                        {
                            var array = text.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                            var num7 = 0;
                            while (num7 < array.Length && num6 < num5)
                            {
                                var b = (array[num7] == "0") ? byte.MaxValue : (byte)0;
                                array2[num6] = b;
                                array2[num6 + 1] = b;
                                array2[num6 + 2] = b;
                                num6 += 4;
                                num7++;
                            }
                            if (num6 >= num5)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (c == '2')
                {
                    var num8 = 0;
                    while (stream.Position < stream.Length)
                    {
                        var text = ReadLine(stream);
                        if (text.Length != 0 && text[0] != '#')
                        {
                            var array = text.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                            var num9 = 0;
                            while (num9 < array.Length && num8 < num5)
                            {
                                var num10 = Convert.ToInt32(array[num9]);
                                array2[num8] = (byte)(num10 * 255 / num3);
                                array2[num8 + 1] = array2[num8];
                                array2[num8 + 2] = array2[num8];
                                num8 += 4;
                                num9++;
                            }
                            if (num8 >= num5)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (c == '3')
                {
                    var num11 = 0;
                    var num12 = 2;
                    while (stream.Position < stream.Length)
                    {
                        var text = ReadLine(stream);
                        if (text.Length != 0 && text[0] != '#')
                        {
                            var array = text.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                            var num13 = 0;
                            while (num13 < array.Length && num11 < num5)
                            {
                                var num14 = Convert.ToInt32(array[num13]);
                                array2[num11 + num12] = (byte)(num14 * 255 / num3);
                                num12--;
                                if (num12 < 0)
                                {
                                    num11 += 4;
                                    num12 = 2;
                                }
                                num13++;
                            }
                            if (num11 >= num5)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (c == '4')
                {
                    var num15 = 0;
                    do
                    {
                        var b2 = (byte)stream.ReadByte();
                        for (var j = 7; j >= 0; j--)
                        {
                            var b3 = (((int)b2 & 1 << j) == 0) ? byte.MaxValue : (byte)0;
                            array2[num15++] = b3;
                            array2[num15++] = b3;
                            array2[num15++] = b3;
                            num15++;
                            if (num15 >= num5)
                            {
                                break;
                            }
                        }
                    }
                    while (num15 < num5);
                }
                else if (c == '5')
                {
                    var num16 = 0;
                    if (num3 < 256)
                    {
                        for (var k = 0; k < num4; k++)
                        {
                            var b4 = (byte)stream.ReadByte();
                            array2[num16++] = b4;
                            array2[num16++] = b4;
                            array2[num16++] = b4;
                            num16++;
                        }
                    }
                    else if (num3 < 65536)
                    {
                        for (var l = 0; l < num4; l++)
                        {
                            var b4 = (byte)stream.ReadByte();
                            stream.ReadByte();
                            array2[num16++] = b4;
                            array2[num16++] = b4;
                            array2[num16++] = b4;
                            num16++;
                        }
                    }
                }
                else if (c == '6')
                {
                    var array3 = new byte[16];
                    var num17 = 0;
                    if (num3 < 256)
                    {
                        for (var m = 0; m < num4; m++)
                        {
                            stream.Read(array3, 0, 3);
                            array2[num17++] = array3[2];
                            array2[num17++] = array3[1];
                            array2[num17++] = array3[0];
                            num17++;
                        }
                    }
                    else if (num3 < 65536)
                    {
                        for (var n = 0; n < num4; n++)
                        {
                            stream.Read(array3, 0, 6);
                            array2[num17++] = array3[4];
                            array2[num17++] = array3[2];
                            array2[num17++] = array3[0];
                            num17++;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num, num2, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            Marshal.Copy(array2, 0, bitmapData.Scan0, array2.Length);
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

        private static char[] whitespace = new char[]
        {
            ' ',
            '\t',
            '\r',
            '\n'
        };
    }
}