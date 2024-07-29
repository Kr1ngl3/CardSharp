using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Avalonia.Controls.Platform;
using Avalonia.Logging;

namespace CardSharp.Models;
public class Server
{
    private string _host = string.Empty;


    public async Task<string> GetHost()
    {
        _ = Task.Run(HostServer);
        while (_host == string.Empty)
            await Task.Delay(10);
        return _host;
    }

    private async Task HostServer()
    {
        using var sshClient = new SshClient("eu.a.pinggy.io", 443, "tcp", "");
        sshClient.Connect();
        Stream shlStream = new MemoryStream();

        var shell = sshClient.CreateShellNoTerminal(new MemoryStream(), shlStream, new MemoryStream(), 65535);
        shell.Start();
        var port = new ForwardedPortRemote(0, "127.0.0.1", 5556);
        sshClient.AddForwardedPort(port);

        port.Start();
        await Task.Delay(1000);
        byte[] buffer = new byte[shlStream.Length + 10];
        shlStream.Position = 0;
        shlStream.Read(buffer, 0, (int)shlStream.Length);

        string response = Encoding.UTF8.GetString(buffer, 0, (int)shlStream.Length - 1);
        _host = response.Substring(151);
        shlStream.Close();

        // creates and starts server/listener
        TcpListener listener = new TcpListener(IPAddress.Any, 5556);
        listener.Start();

        // Create the token source.
        CancellationTokenSource cts = new CancellationTokenSource();

        // waits for client
        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
        
        
        _ = Task.Run(async () =>
        {
            try
            {
                await listener.AcceptTcpClientAsync(cts.Token);
                Trace.WriteLine("Task Finished");
            }
            catch (Exception)
            {
                Trace.WriteLine("Task Canceled");
            }
        });
        Trace.WriteLine("Started new listen");
        for (int i = 0; i < 5; i++)
        {
            Trace.WriteLine(i);
            await Task.Delay(1000);
        }
        
        cts.Cancel();
        using NetworkStream netStream = tcpClient.GetStream();


        byte[] messageBuf = new byte[1_024];


        int received = await netStream.ReadAsync(messageBuf, 0, 1_024);
        byte[] message = new byte[received];
        // copies message out of buffer
        for (int i = 0; i < received; i++)
            message[i] = messageBuf[i];
        string newGreeting = "";
        foreach (byte b in message)
            newGreeting += b;

        while (true)
            await Task.Delay(1000);

        port.Stop();
        sshClient.Disconnect();
    }
}
