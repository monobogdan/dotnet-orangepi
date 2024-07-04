using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Phone
{

    internal sealed class NativeCalls
    {
        public struct SPIDevIOCTransfer
        {
            public ulong txBufPointer;
            public ulong rxBufPointer;

            public uint length;
            public uint speedHz;

            public ushort delayUsecs;
            public byte bitsPerWord;
            public byte csChange;

            public uint pad;
        }

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int ioctl(IntPtr handle, uint request, IntPtr data);
    }

    public sealed class SPIBus
    {
        const string DeviceName = "/dev/spidev0.0";
        const int IOCTLMessage = 1075866368; // SPI_IOC_MESSAGE (1)
        const int DesiredFrequency = 20000000;

        public string DebugName;
        
        private FileStream device;

        public SPIBus(string debugName)
        {
            DebugName = debugName;
            
            device = File.OpenWrite(DeviceName);
        }

        public unsafe bool Transfer(byte[] data)
        {
            NativeCalls.SPIDevIOCTransfer transferDesc = new NativeCalls.SPIDevIOCTransfer();

            fixed (void* ptr = &data[0])
            {
                // Since single transaction buffer size is limited to 64-bytes on AllWinner chipsets, we should split single buffer to multiple transactions
                int numTrans = data.Length / 128;
                bool isSucceded = true;
                
                for(int i = 0; i < numTrans; i++)
                {
                    transferDesc.txBufPointer = ((ulong)ptr) + ((ulong)(i * 128));
                    transferDesc.bitsPerWord = 8;
                    transferDesc.length = 128;
                    transferDesc.speedHz = DesiredFrequency;
                    if (i == numTrans - 1)
                        transferDesc.length = (uint)(data.Length - (i * 128));

                    isSucceded = NativeCalls.ioctl(device.SafeFileHandle.DangerousGetHandle(), IOCTLMessage, new IntPtr(&transferDesc)) >= 0;

                    if (!isSucceded)
                        break;
                }

                return isSucceded;
            }
        }

        public unsafe bool Transfer(byte data)
        {
            NativeCalls.SPIDevIOCTransfer transferDesc = new NativeCalls.SPIDevIOCTransfer();
            
            transferDesc.txBufPointer = (ulong)&data;
            transferDesc.bitsPerWord = 8;
            transferDesc.length = 1;
            transferDesc.speedHz = DesiredFrequency;

            return NativeCalls.ioctl(device.SafeFileHandle.DangerousGetHandle(), IOCTLMessage, new IntPtr(&transferDesc)) >= 0;
        }

        public unsafe void Receive(byte[] data, int length)
        {
            NativeCalls.SPIDevIOCTransfer transferDesc = new NativeCalls.SPIDevIOCTransfer();

            fixed(byte* ptr = &data[0])
            {
                transferDesc.rxBufPointer = (ulong)ptr;
                transferDesc.bitsPerWord = 8;
                transferDesc.length = (uint)length;
                transferDesc.speedHz = DesiredFrequency;

                if (NativeCalls.ioctl(device.SafeFileHandle.DangerousGetHandle(), IOCTLMessage, new IntPtr(&transferDesc)) < 0)
                    throw new ArgumentException("Receive failed");
            }
        }

        public unsafe byte Receive()
        {
            byte ret = 0;
            byte zero = 0;
            NativeCalls.SPIDevIOCTransfer transferDesc = new NativeCalls.SPIDevIOCTransfer();
            
            transferDesc.rxBufPointer = (ulong)&ret;
            transferDesc.bitsPerWord = 8;
            transferDesc.length = 1;
            transferDesc.speedHz = DesiredFrequency;

            if (NativeCalls.ioctl(device.SafeFileHandle.DangerousGetHandle(), IOCTLMessage, new IntPtr(&transferDesc)) < 0)
                throw new ArgumentException("Receive failed");

            return ret;
        }
    }

    public sealed class GPIO
    {
        /// <summary>
        /// True - output, False - input
        /// </summary>
        public bool Mode;
        
        private bool state;
        string basePath;

        public static void Export(int id)
        {
            try
            {
                File.WriteAllText("/sys/class/gpio/export", id.ToString());
                Log.WriteLine("Exported GPIO {0}", id);
            }
            catch (IOException e)
            {
                Log.WriteLine("Failed to export GPIO, assuming they are already exported");
            }
        }

        public GPIO(int id, bool isOutput)
        {
            Mode = isOutput;

            basePath = string.Format("/sys/class/gpio/gpio{0}/", id);

            if (!Directory.Exists(basePath))
                throw new ArgumentException("GPIO not available");

            File.WriteAllText(basePath + "direction", Mode ? "out" : "in");
        }

        public bool ReadValue()
        {
            return File.ReadAllText(basePath + "value") == "1";
        }

        public void SetValue(bool val)
        {
            state = val;

            File.WriteAllText(basePath + "value", val ? "1" : "0");
        }

    }
}
