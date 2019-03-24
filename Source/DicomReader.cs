using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DmitryBrant.ImageFormats
{
    public static class DicomReader
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
            var reader = new BinaryReader(stream);
            var array = new byte[256];
            stream.Seek(128L, SeekOrigin.Current);
            stream.Read(array, 0, 16);
            if (!Encoding.ASCII.GetString(array, 0, 4).Equals("DICM"))
            {
                throw new ApplicationException("Not a valid DICOM file.");
            }
            var num = 0;
            var num2 = 0;
            var num3 = 0;
            var num4 = 0;
            var num5 = 0;
            var num6 = 0;
            var bigEndian = false;
            var flag = true;
            var flag2 = false;
            var flag3 = false;
            var num7 = (int)LittleEndian(BitConverter.ToUInt32(array, 12));
            if (num7 > 10000)
            {
                throw new ApplicationException("Meta group is a bit too long. May not be a valid DICOM file.");
            }
            array = new byte[num7];
            stream.Read(array, 0, num7);
            var @string = Encoding.ASCII.GetString(array);
            if (@string.Contains("1.2.840.10008.1.2\0"))
            {
                flag = false;
            }
            if (@string.Contains("1.2.840.10008.1.2.2\0"))
            {
                bigEndian = true;
            }
            if (@string.Contains("1.2.840.10008.1.2.5\0"))
            {
                flag3 = true;
            }
            if (@string.Contains("1.2.840.10008.1.2.4."))
            {
                flag2 = true;
            }
            if (flag3)
            {
                throw new ApplicationException("RLE-encoded DICOM images are not supported.");
            }
            if (flag2)
            {
                throw new ApplicationException("JPEG-encoded DICOM images are not supported.");
            }
            var flag4 = false;
            while (!flag4 && stream.Position < stream.Length)
            {
                var groupNumber = (int)getGroupNumber(reader, bigEndian);
                var @short = (int)getShort(reader, groupNumber, bigEndian);
                if (groupNumber == 40)
                {
                    if (@short == 2)
                    {
                        num3 = (int)getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else if (@short == 8)
                    {
                        num4 = (int)getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else if (@short == 16)
                    {
                        num2 = (int)getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else if (@short == 17)
                    {
                        num = (int)getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else if (@short == 256)
                    {
                        num5 = (int)getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else if (@short == 257)
                    {
                        getNumeric(reader, groupNumber, bigEndian, flag);
                    }
                    else
                    {
                        skipElement(reader, groupNumber, @short, bigEndian, flag);
                    }
                }
                else if (groupNumber == 32736)
                {
                    if (@short == 16)
                    {
                        if (flag)
                        {
                            stream.ReadByte();
                            stream.ReadByte();
                            getShort(reader, groupNumber, false);
                            num6 = (int)getInt(reader, groupNumber, bigEndian);
                        }
                        else
                        {
                            num6 = (int)getInt(reader, groupNumber, bigEndian);
                        }
                        flag4 = true;
                    }
                    else
                    {
                        skipElement(reader, groupNumber, @short, bigEndian, flag);
                    }
                }
                else
                {
                    skipElement(reader, groupNumber, @short, bigEndian, flag);
                }
            }
            byte[] array2 = null;
            if (num6 > 0)
            {
                array2 = new byte[num6];
                stream.Read(array2, 0, num6);
            }
            else if (num6 == -1)
            {
                var list = new List<byte[]>();
                while (stream.Position < stream.Length)
                {
                    var short2 = getShort(reader, 0, bigEndian);
                    if (short2 != 65534)
                    {
                        break;
                    }
                    short2 = getShort(reader, 0, bigEndian);
                    if (short2 != 57344 && short2 != 57357 && short2 != 57565)
                    {
                        break;
                    }
                    var @int = (int)getInt(reader, 0, bigEndian);
                    if (@int < 0 || @int > 100000000)
                    {
                        break;
                    }
                    if (@int > 0)
                    {
                        var array3 = new byte[@int];
                        stream.Read(array3, 0, @int);
                        list.Add(array3);
                    }
                }
                num6 = 0;
                foreach (var array4 in list)
                {
                    num6 += array4.Length;
                }
                array2 = new byte[num6];
                var num8 = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    Array.Copy(list[i], 0, array2, num8, list[i].Length);
                    num8 += list[i].Length;
                }
            }
            if (num6 == 0)
            {
                throw new ApplicationException("DICOM file does not appear to have any image data.");
            }
            var memoryStream = new MemoryStream(array2);
            reader = new BinaryReader(memoryStream);
            if (array2[0] == 255 && array2[1] == 216 && array2[2] == 255)
            {
                return (Bitmap)Image.FromStream(memoryStream);
            }
            if (num4 == 0)
            {
                num4 = 1;
            }
            if (num3 > 4)
            {
                throw new ApplicationException("Do not support greater than 4 samples per pixel.");
            }
            if (num5 != 8 && num5 != 16 && num5 != 32)
            {
                throw new ApplicationException("Invalid bits per sample.");
            }
            var array5 = new byte[num * 4 * num2];
            try
            {
                if (num3 == 1)
                {
                    if (num5 == 8)
                    {
                        for (var j = 0; j < num2; j++)
                        {
                            for (var k = 0; k < num; k++)
                            {
                                var b = (byte)memoryStream.ReadByte();
                                array5[4 * (j * num + k)] = b;
                                array5[4 * (j * num + k) + 1] = b;
                                array5[4 * (j * num + k) + 2] = b;
                                array5[4 * (j * num + k) + 3] = byte.MaxValue;
                            }
                        }
                    }
                    else if (num5 == 16)
                    {
                        var array6 = new ushort[num2 * num];
                        try
                        {
                            for (var l = 0; l < array6.Length; l++)
                            {
                                array6[l] = getShort(reader, 0, bigEndian);
                            }
                        }
                        catch
                        {
                        }
                        ushort num9 = 0;
                        for (var m = 0; m < array6.Length; m++)
                        {
                            if (array6[m] > num9)
                            {
                                num9 = array6[m];
                            }
                        }
                        var num10 = (num9 == 0) ? 1 : (65536 / (int)num9);
                        var num11 = 0;
                        for (var n = 0; n < num2; n++)
                        {
                            for (var num12 = 0; num12 < num; num12++)
                            {
                                var b2 = (byte)((int)array6[num11++] * num10 >> 8);
                                array5[4 * (n * num + num12)] = b2;
                                array5[4 * (n * num + num12) + 1] = b2;
                                array5[4 * (n * num + num12) + 2] = b2;
                                array5[4 * (n * num + num12) + 3] = byte.MaxValue;
                            }
                        }
                    }
                    else if (num5 == 32)
                    {
                        for (var num13 = 0; num13 < num2; num13++)
                        {
                            for (var num14 = 0; num14 < num; num14++)
                            {
                                var b3 = (byte)(getFloat(reader, 0, bigEndian) * 255f);
                                array5[4 * (num13 * num + num14)] = b3;
                                array5[4 * (num13 * num + num14) + 1] = b3;
                                array5[4 * (num13 * num + num14) + 2] = b3;
                                array5[4 * (num13 * num + num14) + 3] = byte.MaxValue;
                            }
                        }
                    }
                }
                else if (num3 == 3)
                {
                    if (num5 == 8)
                    {
                        for (var num15 = 0; num15 < num2; num15++)
                        {
                            for (var num16 = 0; num16 < num; num16++)
                            {
                                array5[4 * (num15 * num + num16) + 2] = (byte)memoryStream.ReadByte();
                                array5[4 * (num15 * num + num16) + 1] = (byte)memoryStream.ReadByte();
                                array5[4 * (num15 * num + num16)] = (byte)memoryStream.ReadByte();
                                array5[4 * (num15 * num + num16) + 3] = byte.MaxValue;
                            }
                        }
                    }
                    else if (num5 != 16)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
            var bitmap = new Bitmap(num, num2, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            Marshal.Copy(array5, 0, bitmapData.Scan0, num * 4 * num2);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static ushort getGroupNumber(BinaryReader reader, bool bigEndian)
        {
            var num = LittleEndian(reader.ReadUInt16());
            if (num != 2 && bigEndian)
            {
                num = BigEndian(num);
            }
            return num;
        }

        private static ushort getShort(BinaryReader reader, int groupNumber, bool bigEndian)
        {
            ushort result;
            if (groupNumber == 2)
            {
                result = LittleEndian(reader.ReadUInt16());
            }
            else if (bigEndian)
            {
                result = BigEndian(reader.ReadUInt16());
            }
            else
            {
                result = LittleEndian(reader.ReadUInt16());
            }
            return result;
        }

        private static uint getInt(BinaryReader reader, int groupNumber, bool bigEndian)
        {
            uint result;
            if (groupNumber == 2)
            {
                result = LittleEndian(reader.ReadUInt32());
            }
            else if (bigEndian)
            {
                result = BigEndian(reader.ReadUInt32());
            }
            else
            {
                result = LittleEndian(reader.ReadUInt32());
            }
            return result;
        }

        private static float getFloat(BinaryReader reader, int groupNumber, bool bigEndian)
        {
            uint value;
            if (groupNumber == 2)
            {
                value = LittleEndian(reader.ReadUInt32());
            }
            else if (bigEndian)
            {
                value = BigEndian(reader.ReadUInt32());
            }
            else
            {
                value = LittleEndian(reader.ReadUInt32());
            }
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        private static uint getNumeric(BinaryReader reader, int groupNumber, bool bigEndian, bool explicitVR)
        {
            var result = 0u;
            if (explicitVR)
            {
                var num = (int)reader.ReadByte();
                var num2 = (int)reader.ReadByte();
                var @short = (int)getShort(reader, groupNumber, bigEndian);
                if (num == 85 && num2 == 83)
                {
                    if (@short != 2)
                    {
                        throw new ApplicationException("Incorrect size for a US field.");
                    }
                    result = (uint)getShort(reader, groupNumber, bigEndian);
                }
                else if (num == 85 && num2 == 76)
                {
                    if (@short != 4)
                    {
                        throw new ApplicationException("Incorrect size for a UL field.");
                    }
                    result = getInt(reader, groupNumber, bigEndian);
                }
                else if (num == 83 && num2 == 83)
                {
                    if (@short != 2)
                    {
                        throw new ApplicationException("Incorrect size for a SS field.");
                    }
                    result = (uint)getShort(reader, groupNumber, bigEndian);
                }
                else if (num == 83 && num2 == 76)
                {
                    if (@short != 4)
                    {
                        throw new ApplicationException("Incorrect size for a SL field.");
                    }
                    result = getInt(reader, groupNumber, bigEndian);
                }
                else
                {
                    if (num == 73 && num2 == 83 && @short < 16)
                    {
                        var @string = Encoding.ASCII.GetString(reader.ReadBytes(@short));
                        try
                        {
                            return Convert.ToUInt32(@string.Trim());
                        }
                        catch
                        {
                            return result;
                        }
                    }
                    reader.BaseStream.Seek((long)@short, SeekOrigin.Current);
                }
            }
            else
            {
                var @int = (int)getInt(reader, groupNumber, bigEndian);
                if (@int == 2)
                {
                    result = (uint)getShort(reader, groupNumber, bigEndian);
                }
                else if (@int == 4)
                {
                    result = getInt(reader, groupNumber, bigEndian);
                }
                else
                {
                    reader.BaseStream.Seek((long)@int, SeekOrigin.Current);
                }
            }
            return result;
        }

        private static void skipElement(BinaryReader reader, int groupNumber, int elementNumber, bool bigEndian, bool explicitVR)
        {
            if (groupNumber == 65534)
            {
                var num = (int)getInt(reader, groupNumber, bigEndian);
                if (num > 0)
                {
                    reader.BaseStream.Seek((long)num, SeekOrigin.Current);
                    return;
                }
            }
            else if (explicitVR)
            {
                var num2 = (int)reader.ReadByte();
                var num3 = (int)reader.ReadByte();
                int num;
                if ((num2 != 79 || num3 != 66) && (num2 != 79 || num3 != 87) && (num2 != 79 || num3 != 70) && (num2 != 83 || num3 != 81) && (num2 != 85 || num3 != 84) && (num2 != 85 || num3 != 78))
                {
                    num = (int)getShort(reader, groupNumber, bigEndian);
                    reader.BaseStream.Seek((long)num, SeekOrigin.Current);
                    return;
                }
                getShort(reader, groupNumber, false);
                num = (int)getInt(reader, groupNumber, bigEndian);
                if (num2 == 83 && num3 == 81)
                {
                    getShort(reader, groupNumber, bigEndian);
                    getShort(reader, groupNumber, bigEndian);
                    num = (int)getInt(reader, groupNumber, bigEndian);
                    return;
                }
                if (elementNumber != 0)
                {
                    reader.BaseStream.Seek((long)num, SeekOrigin.Current);
                    return;
                }
            }
            else
            {
                var num = (int)getInt(reader, groupNumber, bigEndian);
                if (num == -1)
                {
                    num = 0;
                }
                reader.BaseStream.Seek((long)num, SeekOrigin.Current);
            }
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