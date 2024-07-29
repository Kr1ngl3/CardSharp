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

        var temp = Dns.GetHostEntry(strings[0]).AddressList;

        IPEndPoint _ipEndPoint = new IPEndPoint(Dns.GetHostEntry(strings[0]).AddressList[0], int.Parse(strings[1])  /*IPAddress.Parse("127.0.0.1"), 25565*/);
        TcpClient tcpClient = new TcpClient();
        tcpClient.ConnectAsync(_ipEndPoint).Wait();
        using NetworkStream stream = tcpClient!.GetStream();
        Thread.Sleep(2000);
        byte[] buffer = { 1, 2, 3 };

        Greeting = "Connected and sending";

        await stream.WriteAsync(buffer, 0, 3);

    }

}