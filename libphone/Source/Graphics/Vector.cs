using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phone
{
    public struct Vector
    {
        public int X;
        public int Y;
    }

    public struct Color
    {
        public static Color Blue = new Color(0, 0, 255);

        public byte R;
        public byte G;
        public byte B;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public unsafe void To565(out byte c1, out byte c2)
        {
            short ret = (short)(((uint)R << 11) | ((uint)G << 5) | (uint)B);
            byte* ptr = (byte*)&ret;

            c1 = ptr[0];
            c2 = ptr[1];
        }
    }
}
