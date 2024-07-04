using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Phone
{
    // Since we have only single LCD controller for now (ST7739), there is no need to make LCD class abstract and move initialization to subclasses
    public sealed class LCD
    {
        public string ControllerName;
        public int Width;
        public int Height;

        private GPIO gpioChipSelect;
        private GPIO gpioDataCommand;
        private GPIO gpioReset;

        private void PrepareGPIO()
        {
            const int DC = 10, Reset = 20, CS = 9;

            GPIO.Export(DC);
            GPIO.Export(Reset);
            GPIO.Export(CS);

            gpioChipSelect = new GPIO(CS, true);
            gpioDataCommand = new GPIO(DC, true);
            gpioReset = new GPIO(Reset, true);

            gpioChipSelect.SetValue(true);
        }

        private void ReadID()
        {
            // This shitcode was here to test MISO lines on board
            gpioChipSelect.SetValue(false);

            gpioDataCommand.SetValue(false);
            Board.SPI.Transfer(0x04);

            gpioDataCommand.SetValue(true);
            byte[] id = new byte[4];
            Board.SPI.Receive(id, id.Length);

            Log.WriteLine("{0} {1} {2} {3}", id[0], id[1], id[2], id[3]);
            gpioChipSelect.SetValue(true);
        }

        private unsafe void Initialize()
        {
            // Reset LCM
            gpioReset.SetValue(false);
            Thread.Sleep(10);
            gpioReset.SetValue(true);

            SendCommand(0x01);   //SWRESET
            SendCommand(0x11);   //SLPOUT
            SendCommand(0x3A);   //COLMOD RGB444(12bit) 0x03, RGB565(16bit) 0x05,
            SendData(0x05);  //RGB666(18bit) 0x06
            SendCommand(0x36);   //MADCTL
            SendData(0x14);  //0x08 B-G-R, 0x14 R-G-B
            SendCommand(0x20);   //INVON
            SendCommand(0x13);   //NORON
            SendCommand(0x29);   //DISPON

            // Just copy width and height to prevent using of fixed operator
            int w = Width;
            int h = Height;
            byte* width = (byte*)&w;
            byte* height = (byte*)&h;

            SendCommand(0x2A);   // Set X address
            SendData(0);
            SendData(0);
            SendData((byte)(w >> 8));
            SendData((byte)w);

            SendCommand(0x2B);   // Set Y address
            SendData(0);
            SendData(0);
            SendData((byte)(h >> 8));
            SendData((byte)h);

            SendCommand(0x2C);// Start display
        }

        private void SendCommand(byte cmd)
        {
            // Acquire bus for transaction
            gpioChipSelect.SetValue(false);

            gpioDataCommand.SetValue(false);
            Board.SPI.Transfer(cmd);
            
            // Release bus
            gpioChipSelect.SetValue(true);
        }

        private void SendData(byte data)
        {
            gpioChipSelect.SetValue(false);

            gpioDataCommand.SetValue(true);
            Board.SPI.Transfer(data);

            gpioChipSelect.SetValue(true);
        }

        public LCD()
        {
            ControllerName = "ST7739";
            Width = 240;
            Height = 320;

            Log.WriteLine("Initilaizing LCD");
            Log.WriteLine("LCD configuration: {0} {1}x{2}", ControllerName, Width, Height);

            PrepareGPIO();
            Initialize();
        }

        /// <summary>
        /// CopyTo is assumed to draw back-buffer, that covers whole LCD panel size. So it lacks any positioning arguments and bitmap size should match lcd size
        /// </summary>
        /// <param name="bitmap"></param>
        public unsafe void CopyFrom(Bitmap bitmap)
        {
            gpioChipSelect.SetValue(false);

            gpioDataCommand.SetValue(true);
            if (!Board.SPI.Transfer(bitmap.Data))
                throw new ArgumentException("Failed to copy bitmap on to screen");

            gpioChipSelect.SetValue(true);
        }
    }
}
