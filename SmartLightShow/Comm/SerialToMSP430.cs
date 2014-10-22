using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartLightShow.Comm
{
    class SerialToMSP430
    {
        SerialPort serialPort;
        Thread serialThread; 
        bool threadRunning;
        string[] ports = null;
        public int selectedPort = 0;
        public string COMport = "COM4";
        public Queue<byte[]> byteQueue;

        public SerialToMSP430()
        {
            
        }

        public bool open()
        {
            ports = System.IO.Ports.SerialPort.GetPortNames();
            if (ports.Length == 0) return false;
            COMport = ports[selectedPort];
            serialPort = new SerialPort(COMport, 9600, Parity.None, 8, StopBits.One);
            serialThread = new Thread(new ThreadStart(serialThreadLoop));
            byteQueue = new Queue<byte[]>();
            serialPort.Open();
            threadRunning = true;
            serialThread.Start();
            return true;
        }

        public void close()
        {
            threadRunning = false;
            serialThread.Abort();
            serialPort.Close();
        }

        public void sendBytes(byte[] b)
        {
            byteQueue.Enqueue(b);
        }

        void serialThreadLoop()
        {
            while (threadRunning)
            {
                if (byteQueue.Count > 0)
                {
                    byte[] data = byteQueue.Dequeue();
                    serialPort.Write(data, 0, data.Length);
                }
            }
        }
    }
}
