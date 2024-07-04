using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Phone
{
    class Program
    {

        static void Main(string[] args)
        {
            Bitmap backBuffer = new Bitmap(240, 320);
            Bitmap bg = BmpLoader.Load("test.bitmap");
            Bitmap apple = BmpLoader.Load("apple.bitmap");

            Board.InitializeHardware();

            int x = 0;

            while(true)
            {
                backBuffer.Draw(bg, 0, 0);
                //backBuffer.Draw(apple, x, 0);

                x += 3;

                Board.Touch.Sample();

                Board.LCD.CopyFrom(backBuffer);
            }

            
        }
    }
}
