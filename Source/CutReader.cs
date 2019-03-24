using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DmitryBrant.ImageFormats
{
    public static class CutReader
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
            var num = (int)LittleEndian(binaryReader.ReadUInt16());
            var num2 = (int)LittleEndian(binaryReader.ReadUInt16());
            LittleEndian(binaryReader.ReadUInt16());
            if (num < 1 || num2 < 1 || num > 32767 || num2 > 32767)
            {
                throw new ApplicationException("This CUT file appears to have invalid dimensions.");
            }
            var array = new byte[num * 4 * num2];
            var array2 = new byte[256];
            for (var i = 0; i < array2.Length; i++)
            {
                array2[i] = (byte)i;
            }
            try
            {
                var num3 = 0;
                var num4 = 0;
                while (num4 < num2 && stream.Position < stream.Length)
                {
                    binaryReader.ReadUInt16();
                    while (stream.Position < stream.Length)
                    {
                        var num5 = stream.ReadByte();
                        var num6 = num5 & 127;
                        if (num6 == 0)
                        {
                            num3 = 0;
                            num4++;
                            break;
                        }
                        if (num5 > 127)
                        {
                            var num7 = stream.ReadByte();
                            for (var j = 0; j < num6; j++)
                            {
                                array[4 * (num4 * num + num3)] = array2[num7];
                                array[4 * (num4 * num + num3) + 1] = array2[num7];
                                array[4 * (num4 * num + num3) + 2] = array2[num7];
                                num3++;
                            }
                        }
                        else
                        {
                            for (var j = 0; j < num6; j++)
                            {
                                var num7 = stream.ReadByte();
                                array[4 * (num4 * num + num3)] = array2[num7];
                                array[4 * (num4 * num + num3) + 1] = array2[num7];
                                array[4 * (num4 * num + num3) + 2] = array2[num7];
                                num3++;
                            }
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