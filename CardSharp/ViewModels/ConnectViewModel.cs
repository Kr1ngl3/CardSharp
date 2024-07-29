using ReactiveUI;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;
public class ConnectViewModel : ViewModelBase
{
    private string _greeting = "Hello there";
    public string Greeting { get => _greeting; set => this.RaiseAndSetIfChanged(ref _greeting, value, nameof(Greeting)); }

    private string _ip = "";
    public string Ip { get => _ip; set => this.RaiseAndSetIfChanged(ref _ip, value, nameof(Ip)); }

    public async Task HostGame()
    {
        using var sshClient = new SshClient("eu.a.pinggy.io", 443, "tcp", "");

        sshClient.Connect();
        Stream shlStream = new MemoryStream();

        var shell = sshClient.CreateShellNoTerminal(new MemoryStream(), shlStream, new MemoryStream(), 65535);
        shell.Start();
        var port = new ForwardedPortRemote(0, "127.0.0.1", 25565);
        sshClient.AddForwardedPort(port);
        port.Exception += (_, e) => Console.WriteLine(e.Exception.Message);

        port.Start();
        Thread.Sleep(1000);
        byte[] buffer = new byte[shlStream.Length + 10];
        shlStream.Position = 0;
        shlStream.Read(buffer, 0, (int)shlStream.Length);

        string response = Encoding.UTF8.GetString(buffer, 0, (int)shlStream.Length);
        Greeting = response.Substring(150);
        shlStream.Close();



        // creates and starts server/listener
        TcpListener listener = new TcpListener(IPAddress.Any, 25565);
        listener.Start();


        // waits for client
        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
        using NetworkStream netStream = tcpClient.GetStream();

        byte[] messageBuf = new byte[1_024];

        Greeting = "Waiting for message, conected?";


        int received = await netStream.ReadAsync(messageBuf, 0, 1_024);
        byte[] message = new byte[received];
        // copies message out of buffer
        for (int i = 0; i < received; i++)
            message[i] = messageBuf[i];
        string newGreeting = "";
        foreach (byte b in message)
            newGreeting += b;
        Greeting = newGreeting;

        while (true)
            await Task.Delay(1000);

        port.Stop();

        sshClient.Disconnect();
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