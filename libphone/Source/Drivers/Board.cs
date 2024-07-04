using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phone
{
    public sealed class Board
    {
        public static SPIBus SPI;
        public static LCD LCD;
        public static TouchScreen Touch;

        public static void InitializeHardware()
        {
            SPI = new SPIBus("test");

            LCD = new LCD();
            Touch = new TouchScreen();
        }
    }
}
