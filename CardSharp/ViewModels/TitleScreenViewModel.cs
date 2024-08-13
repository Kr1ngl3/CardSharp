using ReactiveUI;
using Renci.SshNet;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using System.Diagnostics;
using System.Linq;

namespace CardSharp.ViewModels;
public class TitleScreenViewModel : ViewModelBase
{
    private Action _toHost;

    private string _greeting = "Hello there";
    public string Greeting { get => _greeting; set => this.RaiseAndSetIfChanged(ref _greeting, value, nameof(Greeting)); }

    private string _ip = "";
    public string Ip { get => _ip; set => this.RaiseAndSetIfChanged(ref _ip, value, nameof(Ip)); }

    // constructor for designer
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public TitleScreenViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    public TitleScreenViewModel(Action toHost)
    {
        _toHost = toHost;
    }

    public void HostGame()
    {
        _toHost();
    }
    public async Task JoinGame()
    {
        string text = Ip;
        string[]? strings = text.Split(':');

        IPEndPoint _ipEndPoint = new IPEndPoint(Dns.GetHostEntry(strings[0]).AddressList[0], int.Parse(strings[1])  /*IPAddress.Parse("127.0.0.1"), 25565*/);
        TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(_ipEndPoint);
        NetworkStream stream = tcpClient.GetStream();
        //network = stream;
        Greeting = "Connected and sending";

        while (tcpClient.Connected)
        {
            byte[] message = await RecieveMessage(stream);
            string temp = UnicodeEncoding.ASCII.GetString(message);
            await SendMessage(stream, UnicodeEncoding.ASCII.GetBytes(temp.Reverse().ToArray()));
        }
    }

    NetworkStream? network;

    public async Task SendData()
    {
        await SendMessage(network, UnicodeEncoding.ASCII.GetBytes("Gaming"));
    }

    /// <summary>
    /// sends message to client with its given stream
    /// </summary>
    /// <param name="stream"> stream of specific client </param>
    /// <param name="message"> message to send </param>
    private async Task SendMessage(NetworkStream stream, byte[] message)
    {
        await stream.WriteAsync(message, 0, message.Length);
    }

    /// <summary>
    ///  recieves message from client given by stream
    /// </summary>
    /// <param name="stream"> stream of specific client </param>
    /// <returns> the message </returns>
    private async Task<byte[]> RecieveMessage(NetworkStream stream)
    {
        byte[] buffer = new byte[1_024];

        int received = await stream.ReadAsync(buffer, 0, 1_024);

        byte[] temp = new byte[received];
        // copies message out of buffer
        for (int i = 0; i < received; i++)
            temp[i] = buffer[i];
        return temp;
    }

}