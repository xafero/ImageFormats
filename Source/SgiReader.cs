using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DmitryBrant.ImageFormats
{
    public static class SgiReader
    {
        public static Bitmap Load(string fileName)
        {
            Bitmap result;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = Load(fileStream);
            }
            return result;
        }

        public static Bitmap Load(Stream stream)
        {
            var binaryReader = new BinaryReader(stream);
            if (BigEndian(binaryReader.ReadUInt16()) != 474)
            {
                throw new ApplicationException("Not a valid SGI file.");
            }
            var num = stream.ReadByte();
            var num2 = stream.ReadByte();
            var num3 = BigEndian(binaryReader.ReadUInt16());
            if (num > 1)
            {
                throw new ApplicationException("Unsupported compression type.");
            }
            if (num2 != 1)
            {
                throw new ApplicationException("Unsupported bytes per component.");
            }
            if (num3 != 1 && num3 != 2 && num3 != 3)
            {
                throw new ApplicationException("Unsupported dimension.");
            }
            var num4 = (int)BigEndian(binaryReader.ReadUInt16());
            var num5 = (int)BigEndian(binaryReader.ReadUInt16());
            var num6 = (int)BigEndian(binaryReader.ReadUInt16());
            BigEndian(binaryReader.ReadUInt32());
            BigEndian(binaryReader.ReadUInt32());
            if (num4 < 1 || num5 < 1 || num4 > 32767 || num5 > 32767)
            {
                throw new ApplicationException("This SGI file appears to have invalid dimensions.");
            }
            stream.Seek(4L, SeekOrigin.Current);
            Encoding.ASCII.GetString(binaryReader.ReadBytes(80)).Replace("\0", "").Trim();
            BigEndian(binaryReader.ReadUInt32());
            stream.Seek(404L, SeekOrigin.Current);
            uint[] array = null;
            if (num == 1)
            {
                var num7 = num5 * num6;
                array = new uint[num7];
                for (var i = 0; i < num7; i++)
                {
                    array[i] = BigEndian(binaryReader.ReadUInt32());
                }
                stream.Seek((long)((ulong)array[0]), SeekOrigin.Begin);
            }
            var array2 = new byte[num4 * 4 * num5];
            try
            {
                if (num == 1)
                {
                    if (num6 == 1)
                    {
                        for (var j = num5 - 1; j >= 0; j--)
                        {
                            var num8 = 0;
                            while (stream.Position < stream.Length)
                            {
                                var num9 = stream.ReadByte();
                                var num10 = num9 & 127;
                                if (num10 == 0)
                                {
                                    break;
                                }
                                if ((num9 & 128) != 0)
                                {
                                    for (var k = 0; k < num10; k++)
                                    {
                                        var num11 = stream.ReadByte();
                                        array2[4 * (j * num4 + num8)] = (byte)num11;
                                        array2[4 * (j * num4 + num8) + 1] = (byte)num11;
                                        array2[4 * (j * num4 + num8) + 2] = (byte)num11;
                                        num8++;
                                    }
                                }
                                else
                                {
                                    var num11 = stream.ReadByte();
                                    for (var k = 0; k < num10; k++)
                                    {
                                        array2[4 * (j * num4 + num8)] = (byte)num11;
                                        array2[4 * (j * num4 + num8) + 1] = (byte)num11;
                                        array2[4 * (j * num4 + num8) + 2] = (byte)num11;
                                        num8++;
                                    }
                                }
                            }
                        }
                    }
                    else if (num6 == 3 || num6 == 4)
                    {
                        var num12 = 0;
                        var array3 = new byte[num6, num4];
                        for (var l = num5 - 1; l >= 0; l--)
                        {
                            for (var m = 0; m < 3; m++)
                            {
                                var num13 = 0;
                                stream.Seek((long)((ulong)array[num12 + m * num5]), SeekOrigin.Begin);
                                while (stream.Position < stream.Length)
                                {
                                    var num14 = stream.ReadByte();
                                    var num15 = num14 & 127;
                                    if (num15 == 0)
                                    {
                                        break;
                                    }
                                    if ((num14 & 128) != 0)
                                    {
                                        for (var n = 0; n < num15; n++)
                                        {
                                            array3[m, num13++] = (byte)stream.ReadByte();
                                        }
                                    }
                                    else
                                    {
                                        var num16 = stream.ReadByte();
                                        for (var n = 0; n < num15; n++)
                                        {
                                            array3[m, num13++] = (byte)num16;
                                        }
                                    }
                                }
                            }
                            for (var num17 = 0; num17 < num4; num17++)
                            {
                                array2[4 * (l * num4 + num17)] = array3[2, num17];
                                array2[4 * (l * num4 + num17) + 1] = array3[1, num17];
                                array2[4 * (l * num4 + num17) + 2] = array3[0, num17];
                            }
                            num12++;
                        }
                    }
                }
                else if (num6 == 1)
                {
                    for (var num18 = num5 - 1; num18 >= 0; num18--)
                    {
                        for (var num19 = 0; num19 < num4; num19++)
                        {
                            var num20 = stream.ReadByte();
                            array2[4 * (num18 * num4 + num19)] = (byte)num20;
                            array2[4 * (num18 * num4 + num19) + 1] = (byte)num20;
                            array2[4 * (num18 * num4 + num19) + 2] = (byte)num20;
                        }
                    }
                }
                else if (num6 == 3)
                {
                    for (var num21 = num5 - 1; num21 >= 0; num21--)
                    {
                        for (var num22 = 0; num22 < num4; num22++)
                        {
                            var num23 = stream.ReadByte();
                            array2[4 * (num21 * num4 + num22)] = (byte)num23;
                        }
                    }
                    for (var num24 = num5 - 1; num24 >= 0; num24--)
                    {
                        for (var num25 = 0; num25 < num4; num25++)
                        {
                            var num23 = stream.ReadByte();
                            array2[4 * (num24 * num4 + num25) + 1] = (byte)num23;
                        }
                    }
                    for (var num26 = num5 - 1; num26 >= 0; num26--)
                    {
                        for (var num27 = 0; num27 < num4; num27++)
                        {
                            var num23 = stream.ReadByte();
                            array2[4 * (num26 * num4 + num27) + 2] = (byte)num23;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num4, num5, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            Marshal.Copy(array2, 0, bitmapData.Scan0, array2.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static ushort BigEndian(ushort val)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return val;
            }
            return conv_endian(val);
        }

        private static uint BigEndian(uint val)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return val;
            }
            return conv_endian(val);
        }

        private static UInt16 conv_endian(UInt16 val)
        {
            UInt16 temp;
            temp = (UInt16)(val << 8); temp &= 0xFF00; temp |= (UInt16)((val >> 8) & 0xFF);
            return temp;
        }
        private static UInt32 conv_endian(UInt32 val)
        {
            var temp = (val & 0x000000FF) << 24;
            temp |= (val & 0x0000FF00) << 8;
            temp |= (val & 0x00FF0000) >> 8;
            temp |= (val & 0xFF000000) >> 24;
            return (temp);
        }
    }
}