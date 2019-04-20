using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Vanara.PInvoke;

namespace CoopView
{
    /// <summary>
    /// Contains client mode logic.
    /// </summary>
    public class Client
    {
        private const int FrameSize = 65507;
        private const int HeaderSize = 12;
        private const int DataSize = FrameSize - HeaderSize;

        private HWND hwnd;
        private string ip;
        private int port;
        private int scale;
        private int delay;
        private int sequenceNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="scale">Down scale factor.</param>
        /// <param name="fps">The FPS.</param>
        public Client(HWND hwnd, string ip, int port, int scale, int fps)
        {
            this.ip = ip;
            this.port = port;
            this.hwnd = hwnd;
            this.scale = scale;
            this.delay = 1000 / fps;
        }

        /// <summary>
        /// Creates a client instance.
        /// </summary>
        /// <param name="name">The name of the targeted window.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="scale">The down scale factor.</param>
        /// <param name="fps">The FPS.</param>
        /// <returns>The created client instance.</returns>
        public static Client Create(string name, string ip, string port, string scale, string fps)
        {
            HWND handle = default(HWND);
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                {
                    handle = p.MainWindowHandle;
                }
            }

            return new Client(
                handle,
                ip,
                TryParseDefaultTo(port, 43600),
                TryParseDefaultTo(scale, 6),
                TryParseDefaultTo(fps, 20));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            UdpClient udp = new UdpClient();
            udp.Connect(ip, port);
            sequenceNumber = int.MinValue;

            while (true)
            {
                byte[] bytes = GetFrame();
                byte[][] packets = CreatePackets(bytes);
                sequenceNumber++;

                foreach (byte[] packet in packets)
                {
                    udp.Send(packet, packet.Length);
                }

                Thread.Sleep(this.delay);
            }
        }

        /// <summary>
        /// Tries to parse a string to int, if it fails, return the fallback value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="fallback">The fallback value.</param>
        /// <returns>The string as an int or the default value.</returns>
        private static int TryParseDefaultTo(string value, int fallback)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            return fallback;
        }

        /// <summary>
        /// Stores the int in byte array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private static void StoreIntInByteArray(byte[] array, int index, int value)
        {
            array[index] = (byte)(value >> 24);
            array[index + 1] = (byte)(value >> 16);
            array[index + 2] = (byte)(value >> 8);
            array[index + 3] = (byte)value;
        }

        private byte[] GetFrame()
        {
            RECT windowRect = default(RECT);
            User32_Gdi.GetClientRect(hwnd, ref windowRect);

            Bitmap bmp = new Bitmap(Math.Max(1, windowRect.Width), Math.Max(1, windowRect.Height));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                HDC sourceHdc = User32_Gdi.GetDC(this.hwnd);
                HDC targetHdc = g.GetHdc();
                Gdi32.BitBlt(targetHdc, 0, 0, windowRect.Width, windowRect.Height, sourceHdc, 0, 0, Gdi32.RasterOperationMode.SRCCOPY);
                g.ReleaseHdc();
            }

            Bitmap sendableBmp = new Bitmap(bmp, Math.Max(1, bmp.Width / scale), Math.Max(1, bmp.Height / scale));
            bmp.Dispose();

            using (MemoryStream ms = new MemoryStream())
            {
                sendableBmp.Save(ms, ImageFormat.Jpeg);
                sendableBmp.Dispose();
                return ms.ToArray();
            }
        }

        private byte[][] CreatePackets(byte[] data)
        {
            int parts = (int)Math.Ceiling(data.Length / (double)DataSize);
            byte[][] result = new byte[parts][];

            for (int i = 0; i < result.Length; i++)
            {
                int size = i < result.Length - 1 ? FrameSize : HeaderSize + data.Length - (i * DataSize);
                result[i] = new byte[size];
                StoreIntInByteArray(result[i], 0, sequenceNumber);
                StoreIntInByteArray(result[i], 4, i);
                StoreIntInByteArray(result[i], 8, result.Length);
                Array.Copy(data, i * DataSize, result[i], HeaderSize, size - HeaderSize);
            }

            return result;
        }
    }
}
