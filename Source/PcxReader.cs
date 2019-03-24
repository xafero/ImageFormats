using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DmitryBrant.ImageFormats
{
    public static class PcxReader
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
            var binaryReader = new BinaryReader(stream);
            var b = (byte)stream.ReadByte();
            if (b != 10)
            {
                throw new ApplicationException("This is not a valid PCX file.");
            }
            b = (byte)stream.ReadByte();
            if (b < 3 || b > 5)
            {
                throw new ApplicationException("Only Version 3, 4, and 5 PCX files are supported.");
            }
            b = (byte)stream.ReadByte();
            if (b != 1)
            {
                throw new ApplicationException("Invalid PCX compression type.");
            }
            var num3 = stream.ReadByte();
            if (num3 != 8 && num3 != 4 && num3 != 2 && num3 != 1)
            {
                throw new ApplicationException("Only 8, 4, 2, and 1-bit PCX samples are supported.");
            }
            var num4 = LittleEndian(binaryReader.ReadUInt16());
            var num5 = LittleEndian(binaryReader.ReadUInt16());
            var num6 = LittleEndian(binaryReader.ReadUInt16());
            var num7 = (int)LittleEndian(binaryReader.ReadUInt16());
            num = (int)(num6 - num4 + 1);
            num2 = num7 - (int)num5 + 1;
            if (num < 1 || num2 < 1 || num > 32767 || num2 > 32767)
            {
                throw new ApplicationException("This PCX file appears to have invalid dimensions.");
            }
            LittleEndian(binaryReader.ReadUInt16());
            LittleEndian(binaryReader.ReadUInt16());
            var array = new byte[48];
            stream.Read(array, 0, 48);
            stream.ReadByte();
            var num8 = stream.ReadByte();
            var num9 = (int)LittleEndian(binaryReader.ReadUInt16());
            if (num9 == 0)
            {
                num9 = (int)(num6 - num4 + 1);
            }
            if (num3 == 8 && num8 == 1)
            {
                array = new byte[768];
                stream.Seek(-768L, SeekOrigin.End);
                stream.Read(array, 0, 768);
            }
            if (num3 == 1 && array[0] == array[3] && array[1] == array[4] && array[2] == array[5])
            {
                array[0] = (array[1] = (array[2] = 0));
                array[3] = (array[4] = (array[5] = byte.MaxValue));
            }
            var array2 = new byte[(num + 1) * 4 * num2];
            stream.Seek(128L, SeekOrigin.Begin);
            var rleReader = new RleReader(stream);
            try
            {
                if (num3 == 1)
                {
                    var array3 = new byte[num9];
                    var array4 = new byte[num9 * 8];
                    for (var i = 0; i < num2; i++)
                    {
                        Array.Clear(array4, 0, array4.Length);
                        for (var j = 0; j < num8; j++)
                        {
                            var k = 0;
                            for (var l = 0; l < num9; l++)
                            {
                                array3[l] = (byte)rleReader.ReadByte();
                                for (var m = 7; m >= 0; m--)
                                {
                                    byte b2;
                                    if (((int)array3[l] & 1 << m) != 0)
                                    {
                                        b2 = 1;
                                    }
                                    else
                                    {
                                        b2 = 0;
                                    }
                                    var array5 = array4;
                                    var num10 = k;
                                    array5[num10] |= (byte)(b2 << j);
                                    k++;
                                }
                            }
                        }
                        for (var k = 0; k < num; k++)
                        {
                            var l = (int)array4[k];
                            array2[4 * (i * num + k)] = array[l * 3 + 2];
                            array2[4 * (i * num + k) + 1] = array[l * 3 + 1];
                            array2[4 * (i * num + k) + 2] = array[l * 3];
                        }
                    }
                }
                else if (num8 == 1)
                {
                    if (num3 == 8)
                    {
                        var array6 = new byte[num9];
                        for (var i = 0; i < num2; i++)
                        {
                            for (var l = 0; l < num9; l++)
                            {
                                array6[l] = (byte)rleReader.ReadByte();
                            }
                            for (var k = 0; k < num; k++)
                            {
                                var l = (int)array6[k];
                                array2[4 * (i * num + k)] = array[l * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[l * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[l * 3];
                            }
                        }
                    }
                    else if (num3 == 4)
                    {
                        var array7 = new byte[num9];
                        for (var i = 0; i < num2; i++)
                        {
                            for (var l = 0; l < num9; l++)
                            {
                                array7[l] = (byte)rleReader.ReadByte();
                            }
                            for (var k = 0; k < num; k++)
                            {
                                var l = (int)array7[k / 2];
                                array2[4 * (i * num + k)] = array[(l >> 4 & 15) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l >> 4 & 15) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l >> 4 & 15) * 3];
                                k++;
                                array2[4 * (i * num + k)] = array[(l & 15) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l & 15) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l & 15) * 3];
                            }
                        }
                    }
                    else if (num3 == 2)
                    {
                        var array8 = new byte[num9];
                        for (var i = 0; i < num2; i++)
                        {
                            for (var l = 0; l < num9; l++)
                            {
                                array8[l] = (byte)rleReader.ReadByte();
                            }
                            for (var k = 0; k < num; k++)
                            {
                                var l = (int)array8[k / 4];
                                array2[4 * (i * num + k)] = array[(l >> 6 & 3) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l >> 6 & 3) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l >> 6 & 3) * 3];
                                k++;
                                array2[4 * (i * num + k)] = array[(l >> 4 & 3) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l >> 4 & 3) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l >> 4 & 3) * 3];
                                k++;
                                array2[4 * (i * num + k)] = array[(l >> 2 & 3) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l >> 2 & 3) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l >> 2 & 3) * 3];
                                k++;
                                array2[4 * (i * num + k)] = array[(l & 3) * 3 + 2];
                                array2[4 * (i * num + k) + 1] = array[(l & 3) * 3 + 1];
                                array2[4 * (i * num + k) + 2] = array[(l & 3) * 3];
                            }
                        }
                    }
                }
                else if (num8 == 3)
                {
                    var array9 = new byte[num9];
                    var array10 = new byte[num9];
                    var array11 = new byte[num9];
                    var num11 = 0;
                    for (var i = 0; i < num2; i++)
                    {
                        for (var l = 0; l < num9; l++)
                        {
                            array9[l] = (byte)rleReader.ReadByte();
                        }
                        for (var l = 0; l < num9; l++)
                        {
                            array10[l] = (byte)rleReader.ReadByte();
                        }
                        for (var l = 0; l < num9; l++)
                        {
                            array11[l] = (byte)rleReader.ReadByte();
                        }
                        for (var n = 0; n < num; n++)
                        {
                            array2[num11++] = array11[n];
                            array2[num11++] = array10[n];
                            array2[num11++] = array9[n];
                            num11++;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num, num2, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            Marshal.Copy(array2, 0, bitmapData.Scan0, num * 4 * num2);
            bitmap.UnlockBits(bitmapData);
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

        private class RleReader
        {
            public RleReader(Stream stream)
            {
                this.stream = stream;
            }

            public int ReadByte()
            {
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
                    if (currentByte > 191)
                    {
                        runLength = currentByte - 192;
                        currentByte = stream.ReadByte();
                        if (runLength == 1)
                        {
                            runLength = 0;
                        }
                        runIndex = 0;
                    }
                }
                return currentByte;
            }

            private int currentByte;

            private int runLength;

            private int runIndex;

            private Stream stream;
        }
    }
}