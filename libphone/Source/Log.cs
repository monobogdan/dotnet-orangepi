using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Phone
{
    public static class Log
    {

        public static void WriteLine(string format, params object[] args)
        {
            StackFrame frame = new StackFrame(1);

            Console.WriteLine("[{0}::{1}]: {2}", frame.GetMethod().DeclaringType.Name, frame.GetMethod().Name, string.Format(format, args));
        }
    }
}
