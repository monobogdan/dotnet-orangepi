using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Phone
{
    /// <summary>
    /// Represents 565 bitmap
    /// </summary>
    public sealed class Bitmap
    {
        public int Width;
        public int Height;

        public byte[] Data;

        public Bitmap(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new byte[width * height * 2];
        }

        public void SetPixelAt(int x, int y, Color color)
        {
            byte c1, c2;
            color.To565(out c1, out c2);

            int baseOffset = y * Width + x;

            Data[baseOffset] = c1;
            Data[baseOffset + 1] = c2;
        }

        public void Clear(Color color)
        {
            for(int i = 0; i < Width * Height; i++)
            {
                byte c1, c2;
                color.To565(out c1, out c2);

                Data[i * 2] = c1;
                Data[i * 2 + 1] = c2;
            }
        }

        public void Draw(Bitmap from, int x, int y)
        {
            // Scanline-copy without transparency. Needs optimization.

            for (int i = 0; i < from.Height; i++)
            {
                for (int j = 0; j < from.Width; j++)
                {
                    if (j >= Width)
                        break;

                    int baseOffset = (y + i) * Width + (x + j);
                    int baseOffsetSrc = i * from.Width + j;

                    Data[baseOffset * 2] = from.Data[baseOffsetSrc * 2];
                    Data[baseOffset * 2 + 1] = from.Data[baseOffsetSrc * 2 + 1];
                }

                if (i >= Height)
                    break;
            }
        }
    }

    public static class NativeUtils
    {
        public static unsafe T ReadStruct<T>(Stream strm) where T : struct
        {
            int sz = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            byte[] data = new byte[sz];
            strm.Read(data, 0, data.Length);

            T ret = new T();

            fixed (void* ptr = &data[0])
                return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(new IntPtr(ptr), typeof(T));
        }
    }

    public static class BmpLoader
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct BmpHeader
        {
            public short Header;

            public short Width, Height, Format;
        };

        public static unsafe Bitmap Load(string fileName)
        {
            // Read header
            Stream strm = File.OpenRead(fileName);
            BinaryReader binReader = new BinaryReader(strm);
            BmpHeader hdr = new BmpHeader()
            {
                Header = binReader.ReadInt16(),

                Width = binReader.ReadInt16(),
                Height = binReader.ReadInt16(),
                Format = binReader.ReadInt16()
            };

            Log.WriteLine("{0}x{1}", hdr.Width, hdr.Height);

            Bitmap bitmap = new Bitmap(hdr.Width, hdr.Height);
            binReader.Read(bitmap.Data, 0, bitmap.Data.Length);

            return bitmap;
        }
    }
}
