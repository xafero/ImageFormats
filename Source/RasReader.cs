using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DmitryBrant.ImageFormats
{
    public static class RasReader
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
            if (BigEndian(binaryReader.ReadUInt32()) != 1504078485u)
            {
                throw new ApplicationException("This is not a valid RAS file.");
            }
            var num = (int)BigEndian(binaryReader.ReadUInt32());
            var num2 = (int)BigEndian(binaryReader.ReadUInt32());
            var num3 = (int)BigEndian(binaryReader.ReadUInt32());
            BigEndian(binaryReader.ReadUInt32());
            var num4 = BigEndian(binaryReader.ReadUInt32());
            var num5 = BigEndian(binaryReader.ReadUInt32());
            var num6 = BigEndian(binaryReader.ReadUInt32());
            var rleReader = new RleReader(stream, num4 == 2u);
            if (num < 1 || num2 < 1 || num > 32767 || num2 > 32767 || num6 > 32767u)
            {
                throw new ApplicationException("This RAS file appears to have invalid dimensions.");
            }
            if (num3 != 32 && num3 != 24 && num3 != 8 && num3 != 4 && num3 != 1)
            {
                throw new ApplicationException("Only 1, 4, 8, 24, and 32 bit images are supported.");
            }
            var array = new byte[num * 4 * num2];
            byte[] array2 = null;
            if (num5 > 0u)
            {
                array2 = new byte[num6];
                stream.Read(array2, 0, (int)num6);
            }
            try
            {
                if (num3 == 1)
                {
                    var num7 = 0;
                    var i = 0;
                    var num8 = 0;
                    while (i < num2)
                    {
                        var num9 = rleReader.ReadByte();
                        if (num9 == -1)
                        {
                            break;
                        }
                        for (var j = 7; j >= 0; j--)
                        {
                            byte b;
                            if ((num9 & 1 << j) != 0)
                            {
                                b = 0;
                            }
                            else
                            {
                                b = byte.MaxValue;
                            }
                            array[num8++] = b;
                            array[num8++] = b;
                            array[num8++] = b;
                            num8++;
                            num7++;
                            if (num7 == num)
                            {
                                num7 = 0;
                                i++;
                                break;
                            }
                        }
                    }
                }
                else if (num3 == 4)
                {
                    var num10 = 0;
                    var array3 = new byte[num + 1];
                    for (var k = 0; k < num2; k++)
                    {
                        for (var l = 0; l < num; l++)
                        {
                            var num11 = rleReader.ReadByte();
                            array3[l++] = (byte)(num11 >> 4 & 15);
                            array3[l] = (byte)(num11 & 15);
                        }
                        if (num % 2 == 1)
                        {
                            rleReader.ReadByte();
                        }
                        if (num5 > 0u && num6 == 48u)
                        {
                            for (var m = 0; m < num; m++)
                            {
                                array[num10++] = array2[(int)(array3[m] + 32)];
                                array[num10++] = array2[(int)(array3[m] + 16)];
                                array[num10++] = array2[(int)array3[m]];
                                num10++;
                            }
                        }
                        else
                        {
                            for (var n = 0; n < num; n++)
                            {
                                array[num10++] = array3[n];
                                array[num10++] = array3[n];
                                array[num10++] = array3[n];
                                num10++;
                            }
                        }
                    }
                }
                else if (num3 == 8)
                {
                    var num12 = 0;
                    var array4 = new byte[num];
                    for (var num13 = 0; num13 < num2; num13++)
                    {
                        for (var num14 = 0; num14 < num; num14++)
                        {
                            array4[num14] = (byte)rleReader.ReadByte();
                        }
                        if (num % 2 == 1)
                        {
                            rleReader.ReadByte();
                        }
                        if (num5 > 0u && num6 == 768u)
                        {
                            for (var num15 = 0; num15 < num; num15++)
                            {
                                array[num12++] = array2[(int)array4[num15] + 512];
                                array[num12++] = array2[(int)array4[num15] + 256];
                                array[num12++] = array2[(int)array4[num15]];
                                num12++;
                            }
                        }
                        else
                        {
                            for (var num16 = 0; num16 < num; num16++)
                            {
                                array[num12++] = array4[num16];
                                array[num12++] = array4[num16];
                                array[num12++] = array4[num16];
                                num12++;
                            }
                        }
                    }
                }
                else if (num3 == 24)
                {
                    var num17 = 0;
                    var array5 = new byte[num * 3];
                    for (var num18 = 0; num18 < num2; num18++)
                    {
                        for (var num19 = 0; num19 < num * 3; num19++)
                        {
                            array5[num19] = (byte)rleReader.ReadByte();
                        }
                        if (num * 3 % 2 == 1)
                        {
                            stream.ReadByte();
                        }
                        for (var num20 = 0; num20 < num; num20++)
                        {
                            array[num17++] = array5[num20 * 3];
                            array[num17++] = array5[num20 * 3 + 1];
                            array[num17++] = array5[num20 * 3 + 2];
                            num17++;
                        }
                    }
                }
                else if (num3 == 32)
                {
                    var num21 = 0;
                    var array6 = new byte[num * 4];
                    for (var num22 = 0; num22 < num2; num22++)
                    {
                        for (var num23 = 0; num23 < num * 4; num23++)
                        {
                            array6[num23] = (byte)rleReader.ReadByte();
                        }
                        for (var num24 = 0; num24 < num; num24++)
                        {
                            array[num21++] = array6[num24 * 4];
                            array[num21++] = array6[num24 * 4 + 1];
                            array[num21++] = array6[num24 * 4 + 2];
                            array[num21++] = array6[num24 * 4 + 3];
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num, num2, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
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

        private class RleReader
        {
            public RleReader(Stream stream, bool isRle)
            {
                this.stream = stream;
                this.isRle = isRle;
            }

            public int ReadByte()
            {
                if (!isRle)
                {
                    return stream.ReadByte();
                }
                if (runLength > 0)
                {
                    runIndex++;
                    if (runIndex == runLength - 1)
                    {
                        runLength = 0;
                    }
                }
                else
                {
                    currentByte = stream.ReadByte();
                    if (currentByte == 128)
                    {
                        currentByte = stream.ReadByte();
                        if (currentByte == 0)
                        {
                            currentByte = 128;
                        }
                        else
                        {
                            runLength = currentByte + 1;
                            runIndex = 0;
                            currentByte = stream.ReadByte();
                        }
                    }
                }
                return currentByte;
            }

            private int currentByte;

            private int runLength;

            private int runIndex;

            private Stream stream;

            private bool isRle;
        }
    }
}