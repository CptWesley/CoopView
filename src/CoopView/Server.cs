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
        private const int FrameSize = 65507;
        private const int HeaderSize = 12;
        private const int DataSize = FrameSize - HeaderSize;

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
        /// Reads the int from byte array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <returns>Integer in a byte array at the given index.</returns>
        private static int ReadIntFromByteArray(byte[] array, int index)
            => (array[index] << 24) + (array[index + 1] << 16) + (array[index + 2] << 8) + array[index + 3];

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

                byte[] packet = udp.Receive(ref ep);
                while (true)
                {
                    int firstSequenceNumber = ReadIntFromByteArray(packet, 0);
                    int size = ReadIntFromByteArray(packet, 8);
                    byte[][] packets = new byte[size][];
                    packets[ReadIntFromByteArray(packet, 4)] = packet;
                    int remaining = size - 1;
                    if (remaining == 0)
                    {
                        packet = udp.Receive(ref ep);
                    }

                    bool overriden = false;
                    while (remaining > 0)
                    {
                        packet = udp.Receive(ref ep);
                        int sequenceNumber = ReadIntFromByteArray(packet, 0);
                        if (sequenceNumber > firstSequenceNumber || (sequenceNumber < (int.MinValue / 2) && firstSequenceNumber > (int.MaxValue / 2)))
                        {
                            overriden = true;
                            break;
                        }
                        else if (sequenceNumber == firstSequenceNumber)
                        {
                            int index = ReadIntFromByteArray(packet, 4);
                            if (packets[index] == null)
                            {
                                packets[index] = packet;
                                remaining--;
                            }
                        }
                    }

                    if (!overriden)
                    {
                        byte[] data = new byte[(DataSize * (packets.Length - 1)) + packets[packets.Length - 1].Length - 12];
                        int dataIndex = 0;
                        for (int i = 0; i < packets.Length; i++)
                        {
                            int copySize = packets[i].Length - HeaderSize;
                            Array.Copy(packets[i], HeaderSize, data, dataIndex, copySize);
                            dataIndex += copySize;
                        }

                        ShowBitmap(data);
                    }
                }
            });
        }

        private void ShowBitmap(byte[] data)
        {
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
            }
        }
    }
}
