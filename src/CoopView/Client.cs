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
        private HWND hwnd;
        private string ip;
        private int port;
        private int scale;
        private int delay;

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
            HWND handle;
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                {
                    handle = p.MainWindowHandle;
                }
            }

            return new Client(
                Process.GetProcessesByName(name)[0].MainWindowHandle,
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

            while (true)
            {
                byte[] bytes = GetFrame();
                udp.Send(bytes, bytes.Length);
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

        private byte[] GetFrame()
        {
            HDC hdc = User32_Gdi.GetDC(this.hwnd);
            RECT windowRect = default(RECT);
            User32_Gdi.GetClientRect(hwnd, ref windowRect);

            Bitmap bmp = new Bitmap(windowRect.Width, windowRect.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Gdi32.BitBlt(g.GetHdc(), 0, 0, windowRect.Width, windowRect.Height, hdc, 0, 0, Gdi32.RasterOperationMode.SRCCOPY);
            }

            bmp = new Bitmap(bmp, bmp.Width / scale, bmp.Height / scale);

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
