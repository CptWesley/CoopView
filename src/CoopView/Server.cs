using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoopView
{
    /// <summary>
    /// Contains logic for server mode.
    /// </summary>
    public class Server : Form
    {
        private int port;
        private PictureBox pb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public Server(int port)
        {
            this.port = port;
            this.pb = new PictureBox();
            this.Controls.Add(pb);
            this.Shown += OnShown;
        }

        /// <summary>
        /// Creates a server.
        /// </summary>
        /// <param name="portStr">The port string.</param>
        /// <returns>The created server.</returns>
        public static Server Create(string portStr)
        {
            int port;
            if (!int.TryParse(portStr, out port))
            {
                port = 43600;
            }

            return new Server(port);
        }

        /// <summary>
        /// Executed when the form is shown.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnShown(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, this.port);
                UdpClient udp = new UdpClient(ep);

                while (true)
                {
                    byte[] data = udp.Receive(ref ep);
                    using (var ms = new MemoryStream(data))
                    {
                        Bitmap bmp = new Bitmap(ms);
                        this.Invoke((MethodInvoker)(() =>
                        {
                            this.Width = bmp.Width;
                            this.Height = bmp.Height;
                            this.pb.Width = bmp.Width;
                            this.pb.Height = bmp.Height;
                            this.pb.Image = bmp;
                        }));
                        System.Console.WriteLine(bmp.Width + " : " + bmp.Height);
                    }
                }
            });
        }
    }
}
