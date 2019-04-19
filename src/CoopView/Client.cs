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
        private string ip;
        private int port;
        private int width;
        private HDC hdc;
        private int height;
        private int delay;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="hdc">The HDC.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="fps">The FPS.</param>
        public Client(string ip, int port, HDC hdc, int width, int height, int fps)
        {
            this.ip = ip;
            this.port = port;
            this.hdc = hdc;
            this.width = width;
            this.height = height;
            this.delay = 1000 / fps;
        }

        /// <summary>
        /// Creates a client instance.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="name">The name of the targeted window.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="fps">The FPS.</param>
        /// <returns>The created client instance.</returns>
        public static Client Create(string ip, string port, string name, string width, string height, string fps)
        {
            HDC hdc = User32_Gdi.GetDC(Process.GetProcessesByName(name)[0].MainWindowHandle);

            return new Client(
                ip,
                TryParseDefaultTo(port, 43600),
                hdc,
                TryParseDefaultTo(width, 320),
                TryParseDefaultTo(height, 180),
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
            HWND hwnd = User32_Gdi.WindowFromDC(hdc);
            RECT windowRect = default(RECT);
            User32_Gdi.GetClientRect(hwnd, ref windowRect);

            Bitmap bmp = new Bitmap(windowRect.Width, windowRect.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Gdi32.BitBlt(hdc, 0, 0, windowRect.Width, windowRect.Height, g.GetHdc(), 0, 0, Gdi32.RasterOperationMode.SRCCOPY);
            }

            bmp = new Bitmap(bmp, this.width, this.height);

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
