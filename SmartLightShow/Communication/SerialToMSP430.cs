using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Collections;

namespace SmartLightShow.Communication
{
    class SerialToMSP430
    {
        public static SerialToMSP430 staticInstance = new SerialToMSP430();

        SerialPort serialPort;
        Thread serialThread; 
        bool threadRunning;
        string[] ports = null;
        public int selectedPort = 0;
        public string COMport = "COM4";
        public Queue<byte[]> byteQueue;
        public Queue<long> timeQueue;
        public Hashtable map;
        public long startTime;

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
            timeQueue = new Queue<long>();
            map = new Hashtable();
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

        public void startTimer()
        {
            startTime = DateTime.UtcNow.Ticks;
        }

        public void sendBytes(byte[] b, long timestamp)
        {
            if (map.ContainsKey(timestamp))
            {
                byte[] old = (byte[])map[timestamp];
                for (int i = 0; i < old.Length; i++)
                {
                    old[i] |= b[i];
                }
            }
            else
            {
                map.Add(timestamp, b);
                timeQueue.Enqueue(timestamp);
                byteQueue.Enqueue(b);
            }
        }

        void serialThreadLoop()
        {
            while (threadRunning)
            {
                if (byteQueue.Count > 0)
                {
                    long ms = DateTime.UtcNow.Ticks - startTime;
                    long timestamp = timeQueue.Peek();
                    if (ms >= timestamp * TimeSpan.TicksPerMillisecond)
                    {
                        byte[] data = byteQueue.Dequeue();
                        serialPort.Write(data, 0, data.Length);
                        timeQueue.Dequeue();
                    }
                }
            }
        }
    }
}
