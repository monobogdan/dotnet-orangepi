using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Phone
{
    // XPT2046 driver
    public sealed class TouchScreen
    {
        private GPIO gpioChipSelect;

        public int X;
        public int Y;
        public bool IsTouching;

        public TouchScreen()
        {
            const int GPIOCS = 8;

            GPIO.Export(GPIOCS);
            gpioChipSelect = new GPIO(GPIOCS, true);

            gpioChipSelect.SetValue(true);
        }

        private unsafe ushort RequestSample(byte cmd)
        {
            gpioChipSelect.SetValue(false);
            Board.SPI.Transfer(cmd);
            byte b1 = Board.SPI.Receive();
            gpioChipSelect.SetValue(true);

            return b1;
        }

        public void Sample()
        {
            const int SampleX = 0xD8, SampleY = 0x98;

            ushort sX = RequestSample(SampleX);
            ushort sY = RequestSample(SampleY);

            Log.WriteLine("{0} {1}", ((float)sX / 4096) * 240, sY);

            Thread.Sleep(30);
        }
    }
}
