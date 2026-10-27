using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, 4567);

            using var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverSocket.Bind(localEndPoint);

            Console.WriteLine("Server ready. Waiting...");

            var receiveBuffer = new byte[1024];

            while (true)
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                var receiveResult = await serverSocket.ReceiveFromAsync(receiveBuffer, SocketFlags.None, remoteEndPoint);
                string msg = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.ReceivedBytes).Trim().ToLower();

                Console.WriteLine($"{receiveResult.RemoteEndPoint} => {msg}");

                if (msg == "screenshot")
                {
                    int width = 640;
                    int height = 360;

                    using var bitmap = new Bitmap(width, height);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    }

                    using var ms = new MemoryStream();

                    bitmap.Save(ms, ImageFormat.Jpeg);
                    byte[] imgBytes = ms.ToArray();

                    if (imgBytes.Length < 65507)
                    {
                        await serverSocket.SendToAsync(imgBytes, SocketFlags.None, receiveResult.RemoteEndPoint);
                    }
                    else
                    {
                        Console.WriteLine("Image size is larger than UDP limit!");
                    }
                }
            }
        }
    }
}