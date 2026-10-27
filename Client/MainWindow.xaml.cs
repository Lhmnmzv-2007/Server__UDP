using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts;
    private readonly IPEndPoint _remoteEndPoint =
        new IPEndPoint(IPAddress.Parse("192.168.0.20"), 4567);

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Start_Click(object sender, RoutedEventArgs e)
    {
        Start.IsEnabled = false;
        Stop.IsEnabled = true;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(_remoteEndPoint);

        byte[] request = Encoding.UTF8.GetBytes("screenshot");
        var buffer = new byte[65507];

        while (!token.IsCancellationRequested)
        {
            await socket.SendAsync(request, SocketFlags.None, token);
            int receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, token);
            using var ms = new MemoryStream(buffer, 0, receivedBytes);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();

            screenshotImage.Source = bitmap;

            await Task.Delay(250, token);
        }

        Start.IsEnabled = true;
        Stop.IsEnabled = false;
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        Stop.IsEnabled = false;
    }
}