using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DmitryBrant.ImageFormats
{
    public static class TgaReader
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
            var binaryReader = new BinaryReader(stream);
            uint[] array = null;
            var b = (byte)stream.ReadByte();
            var b2 = (byte)stream.ReadByte();
            var b3 = (byte)stream.ReadByte();
            var num = LittleEndian(binaryReader.ReadUInt16());
            var num2 = LittleEndian(binaryReader.ReadUInt16());
            var b4 = (byte)stream.ReadByte();
            LittleEndian(binaryReader.ReadUInt16());
            LittleEndian(binaryReader.ReadUInt16());
            var num3 = LittleEndian(binaryReader.ReadUInt16());
            var num4 = LittleEndian(binaryReader.ReadUInt16());
            var b5 = (byte)stream.ReadByte();
            var b6 = (byte)stream.ReadByte();
            if (b2 > 1)
            {
                throw new ApplicationException("This is not a valid TGA file.");
            }
            if (b > 0)
            {
                var array2 = new byte[(int)b];
                stream.Read(array2, 0, (int)b);
                Encoding.ASCII.GetString(array2);
            }
            if (b3 > 11 || (b3 > 3 && b3 < 9))
            {
                throw new ApplicationException("This image type (" + b3 + ") is not supported.");
            }
            if (b5 != 8 && b5 != 15 && b5 != 16 && b5 != 24 && b5 != 32)
            {
                throw new ApplicationException("Number of bits per pixel (" + b5 + ") is not supported.");
            }
            if (b2 > 0 && b4 != 15 && b4 != 16 && b4 != 24 && b4 != 32)
            {
                throw new ApplicationException("Number of bits per color map (" + b5 + ") is not supported.");
            }
            var array3 = new byte[(int)(num3 * 4 * num4)];
            try
            {
                if (b2 > 0)
                {
                    var num5 = (int)(num + num2);
                    array = new uint[num5];
                    if (b4 == 24)
                    {
                        for (var i = (int)num; i < num5; i++)
                        {
                            array[i] = 4278190080u;
                            array[i] |= (uint)((uint)stream.ReadByte() << 16);
                            array[i] |= (uint)((uint)stream.ReadByte() << 8);
                            array[i] |= (uint)stream.ReadByte();
                        }
                    }
                    else if (b4 == 32)
                    {
                        for (var j = (int)num; j < num5; j++)
                        {
                            array[j] = 4278190080u;
                            array[j] |= (uint)((uint)stream.ReadByte() << 16);
                            array[j] |= (uint)((uint)stream.ReadByte() << 8);
                            array[j] |= (uint)stream.ReadByte();
                            array[j] |= (uint)((uint)stream.ReadByte() << 24);
                        }
                    }
                    else if (b4 == 15 || b4 == 16)
                    {
                        for (var k = (int)num; k < num5; k++)
                        {
                            var num6 = stream.ReadByte();
                            var num7 = stream.ReadByte();
                            array[k] = 4278190080u;
                            array[k] |= (uint)((uint)((uint)(num6 & 31) << 3) << 16);
                            array[k] |= (uint)((uint)((uint)(((num7 & 3) << 3) + ((num6 & 224) >> 5)) << 3) << 8);
                            array[k] |= (uint)((uint)((num7 & 127) >> 2) << 3);
                        }
                    }
                }
                if (b3 == 1 || b3 == 2 || b3 == 3)
                {
                    var array4 = new byte[(int)(num3 * (ushort)(b5 / 8))];
                    for (var l = (int)(num4 - 1); l >= 0; l--)
                    {
                        if (b5 <= 16)
                        {
                            if (b5 != 8)
                            {
                                if (b5 - 15 <= 1)
                                {
                                    for (var m = 0; m < (int)num3; m++)
                                    {
                                        var num8 = stream.ReadByte();
                                        var num9 = stream.ReadByte();
                                        array3[4 * (l * (int)num3 + m)] = (byte)((num8 & 31) << 3);
                                        array3[4 * (l * (int)num3 + m) + 1] = (byte)(((num9 & 3) << 3) + ((num8 & 224) >> 5) << 3);
                                        array3[4 * (l * (int)num3 + m) + 2] = (byte)((num9 & 127) >> 2 << 3);
                                        array3[4 * (l * (int)num3 + m) + 3] = byte.MaxValue;
                                    }
                                }
                            }
                            else
                            {
                                stream.Read(array4, 0, array4.Length);
                                if (b3 == 1)
                                {
                                    for (var n = 0; n < (int)num3; n++)
                                    {
                                        array3[4 * (l * (int)num3 + n)] = (byte)(array[(int)array4[n]] >> 16 & 255u);
                                        array3[4 * (l * (int)num3 + n) + 1] = (byte)(array[(int)array4[n]] >> 8 & 255u);
                                        array3[4 * (l * (int)num3 + n) + 2] = (byte)(array[(int)array4[n]] & 255u);
                                        array3[4 * (l * (int)num3 + n) + 3] = byte.MaxValue;
                                    }
                                }
                                else if (b3 == 3)
                                {
                                    for (var num10 = 0; num10 < (int)num3; num10++)
                                    {
                                        array3[4 * (l * (int)num3 + num10)] = array4[num10];
                                        array3[4 * (l * (int)num3 + num10) + 1] = array4[num10];
                                        array3[4 * (l * (int)num3 + num10) + 2] = array4[num10];
                                        array3[4 * (l * (int)num3 + num10) + 3] = byte.MaxValue;
                                    }
                                }
                            }
                        }
                        else if (b5 != 24)
                        {
                            if (b5 == 32)
                            {
                                stream.Read(array4, 0, array4.Length);
                                for (var num11 = 0; num11 < (int)num3; num11++)
                                {
                                    array3[4 * (l * (int)num3 + num11)] = array4[num11 * 4];
                                    array3[4 * (l * (int)num3 + num11) + 1] = array4[num11 * 4 + 1];
                                    array3[4 * (l * (int)num3 + num11) + 2] = array4[num11 * 4 + 2];
                                    array3[4 * (l * (int)num3 + num11) + 3] = byte.MaxValue;
                                }
                            }
                        }
                        else
                        {
                            stream.Read(array4, 0, array4.Length);
                            for (var num12 = 0; num12 < (int)num3; num12++)
                            {
                                array3[4 * (l * (int)num3 + num12)] = array4[num12 * 3];
                                array3[4 * (l * (int)num3 + num12) + 1] = array4[num12 * 3 + 1];
                                array3[4 * (l * (int)num3 + num12) + 2] = array4[num12 * 3 + 2];
                                array3[4 * (l * (int)num3 + num12) + 3] = byte.MaxValue;
                            }
                        }
                    }
                }
                else if (b3 == 9 || b3 == 10 || b3 == 11)
                {
                    var num13 = (int)(num4 - 1);
                    var num14 = 0;
                    var num15 = (int)(b5 / 8);
                    var array4 = new byte[(int)(num3 * 4)];
                    while (num13 >= 0 && stream.Position < stream.Length)
                    {
                        var num16 = stream.ReadByte();
                        if (num16 < 128)
                        {
                            num16++;
                            if (b5 <= 16)
                            {
                                if (b5 != 8)
                                {
                                    if (b5 - 15 <= 1)
                                    {
                                        for (var num17 = 0; num17 < num16; num17++)
                                        {
                                            var num18 = stream.ReadByte();
                                            var num19 = stream.ReadByte();
                                            array3[4 * (num13 * (int)num3 + num14)] = (byte)((num18 & 31) << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)(((num19 & 3) << 3) + ((num18 & 224) >> 5) << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)((num19 & 127) >> 2 << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    stream.Read(array4, 0, num16 * num15);
                                    if (b3 == 9)
                                    {
                                        for (var num20 = 0; num20 < num16; num20++)
                                        {
                                            array3[4 * (num13 * (int)num3 + num14)] = (byte)(array[(int)array4[num20]] >> 16 & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)(array[(int)array4[num20]] >> 8 & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)(array[(int)array4[num20]] & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                    else if (b3 == 11)
                                    {
                                        for (var num21 = 0; num21 < num16; num21++)
                                        {
                                            array3[4 * (num13 * (int)num3 + num14)] = array4[num21];
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = array4[num21];
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = array4[num21];
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (b5 != 24)
                            {
                                if (b5 == 32)
                                {
                                    stream.Read(array4, 0, num16 * num15);
                                    for (var num22 = 0; num22 < num16; num22++)
                                    {
                                        array3[4 * (num13 * (int)num3 + num14)] = array4[num22 * 4];
                                        array3[4 * (num13 * (int)num3 + num14) + 1] = array4[num22 * 4 + 1];
                                        array3[4 * (num13 * (int)num3 + num14) + 2] = array4[num22 * 4 + 2];
                                        array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                        num14++;
                                        if (num14 >= (int)num3)
                                        {
                                            num14 = 0;
                                            num13--;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                stream.Read(array4, 0, num16 * num15);
                                for (var num23 = 0; num23 < num16; num23++)
                                {
                                    array3[4 * (num13 * (int)num3 + num14)] = array4[num23 * 3];
                                    array3[4 * (num13 * (int)num3 + num14) + 1] = array4[num23 * 3 + 1];
                                    array3[4 * (num13 * (int)num3 + num14) + 2] = array4[num23 * 3 + 2];
                                    array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                    num14++;
                                    if (num14 >= (int)num3)
                                    {
                                        num14 = 0;
                                        num13--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            num16 = (num16 & 127) + 1;
                            if (b5 <= 16)
                            {
                                if (b5 != 8)
                                {
                                    if (b5 - 15 <= 1)
                                    {
                                        var num24 = stream.ReadByte();
                                        var num25 = stream.ReadByte();
                                        for (var num26 = 0; num26 < num16; num26++)
                                        {
                                            array3[4 * (num13 * (int)num3 + num14)] = (byte)((num24 & 31) << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)(((num25 & 3) << 3) + ((num24 & 224) >> 5) << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)((num25 & 127) >> 2 << 3);
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var num27 = stream.ReadByte();
                                    if (b3 == 9)
                                    {
                                        for (var num28 = 0; num28 < num16; num28++)
                                        {
                                            array3[4 * (num13 * (int)num3 + num14)] = (byte)(array[num27] >> 16 & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)(array[num27] >> 8 & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)(array[num27] & 255u);
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                    else if (b3 == 11)
                                    {
                                        for (var num29 = 0; num29 < num16; num29++)
                                        {
                                            array3[4 * (num13 * (int)num3 + num14)] = (byte)num27;
                                            array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)num27;
                                            array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)num27;
                                            array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                            num14++;
                                            if (num14 >= (int)num3)
                                            {
                                                num14 = 0;
                                                num13--;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (b5 != 24)
                            {
                                if (b5 == 32)
                                {
                                    var num30 = stream.ReadByte();
                                    var num31 = stream.ReadByte();
                                    var num32 = stream.ReadByte();
                                    stream.ReadByte();
                                    for (var num33 = 0; num33 < num16; num33++)
                                    {
                                        array3[4 * (num13 * (int)num3 + num14)] = (byte)num30;
                                        array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)num31;
                                        array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)num32;
                                        array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                        num14++;
                                        if (num14 >= (int)num3)
                                        {
                                            num14 = 0;
                                            num13--;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var num30 = stream.ReadByte();
                                var num31 = stream.ReadByte();
                                var num32 = stream.ReadByte();
                                for (var num34 = 0; num34 < num16; num34++)
                                {
                                    array3[4 * (num13 * (int)num3 + num14)] = (byte)num30;
                                    array3[4 * (num13 * (int)num3 + num14) + 1] = (byte)num31;
                                    array3[4 * (num13 * (int)num3 + num14) + 2] = (byte)num32;
                                    array3[4 * (num13 * (int)num3 + num14) + 3] = byte.MaxValue;
                                    num14++;
                                    if (num14 >= (int)num3)
                                    {
                                        num14 = 0;
                                        num13--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap((int)num3, (int)num4, PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(array3, 0, bitmapData.Scan0, (int)(num3 * 4 * num4));
            bitmap.UnlockBits(bitmapData);
            var num35 = b6 >> 4 & 3;
            if (num35 == 1)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            else if (num35 == 2)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            }
            else if (num35 == 3)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            return bitmap;
        }

        private static ushort LittleEndian(ushort val)
        {
            if (BitConverter.IsLittleEndian)
            {
                return val;
            }
            return conv_endian(val);
        }

        private static uint LittleEndian(uint val)
        {
            if (BitConverter.IsLittleEndian)
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