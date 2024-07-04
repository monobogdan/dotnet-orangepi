using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Image2Bmp
{
    class Program
    {

        private static unsafe void WriteBitmap(Bitmap bmp, Stream strm)
        {
            BinaryWriter binWriter = new BinaryWriter(strm);

            // Write header
            binWriter.Write((short)0x1337);
            binWriter.Write((short)bmp.Width);
            binWriter.Write((short)bmp.Height);
            binWriter.Write((short)0); // 0 - RGB565, fixed at this moment

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            byte[] pixels = new byte[bmp.Width * bmp.Height * 2];
            byte* ptr = (byte*)data.Scan0.ToPointer();

            // Quick endianness swap
            for(int i = 0; i < bmp.Width * bmp.Height; i++)
            {
                pixels[i * 2] = ptr[i * 2 + 1];
                pixels[i * 2 + 1] = ptr[i * 2];
            }

            bmp.UnlockBits(data);

            binWriter.Write(pixels, 0, pixels.Length);
        }

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: Image2Bmp filename");

                return;
            }

            Bitmap bmp = (Bitmap)Image.FromFile(args[0]);
            FileStream strm = File.Create(Path.GetFileNameWithoutExtension(args[0]) + ".bitmap");

            Console.WriteLine("Converting bitmap {0}", args[0]);

            WriteBitmap(bmp, strm);
            strm.Close();
        }
    }
}
