using Renci.SshNet;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CardSharp.Models;
public class Server
{
    private bool _endServerFlag = false;
    private string _host = string.Empty;
    private Queue<string> _playerNames = new Queue<string>(["Host"]);
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private List<NetworkStream> _streams = new List<NetworkStream>();

    public virtual async Task<string> GetHost()
    {
        _ = Task.Run(HostServer);
        while (_host == string.Empty)
            await Task.Delay(10);
        return _host;
    }

    public virtual async IAsyncEnumerable<string> GetNames()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_cts.IsCancellationRequested)
                yield break;

            while(_playerNames.Count == 0)
                await Task.Delay(100);
            yield return _playerNames.Dequeue();
        }
    }

    public virtual void CancelSearch()
    {
        _cts.Cancel();
    }

    private async Task HostServer()
    {
        using SshClient sshClient = new SshClient("eu.a.pinggy.io", 443, "tcp", "");
        sshClient.Connect();

        Stream shlStream = new MemoryStream();
        Shell shell = sshClient.CreateShellNoTerminal(new MemoryStream(), shlStream, new MemoryStream(), 65535);
        shell.Start();

        var port = new ForwardedPortRemote(0, "127.0.0.1", 5556);
        sshClient.AddForwardedPort(port);
        port.Start();
        await Task.Delay(1000);

        // the part we wants starts at 151
        shlStream.Position = 151;
        // last character is \n so we dont read it
        int length = (int)shlStream.Length - 152;

        byte[] buffer = new byte[length];
        shlStream.Read(buffer.AsSpan());
        shlStream.Close();
        //shell.Stop();
        _host = Encoding.UTF8.GetString(buffer);

        // creates and starts server/listener
        TcpListener listener = new TcpListener(IPAddress.Any, 5556);
        listener.Start();

        try
        {
            for (int i = 1; i <= 5; i++)
            {
                // waits for client
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                //_playerNames.Enqueue(tcpClient.Client.RemoteEndPoint!.ToString()!);
                _ = Task.Run(() => PlayerHandler(tcpClient, i));
            
                //using NetworkStream netStream = tcpClient.GetStream();


                //byte[] messageBuf = new byte[1_024];
                //int received = await netStream.ReadAsync(messageBuf, 0, 1_024);
                //byte[] message = new byte[received];
                //// copies message out of buffer
                //for (int i = 0; i < received; i++)
                //    message[i] = messageBuf[i];
                //string newGreeting = "";
                //foreach (byte b in message)
                //    newGreeting += b;
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _cts.Dispose();
        }

        while (!_endServerFlag)
            await Task.Delay(1000);
        //port.Stop();
        //sshClient.Disconnect();
    }

    private async Task PlayerHandler(TcpClient tcpClient, int player)
    {
        NetworkStream stream = tcpClient.GetStream();
        _streams.Add(stream);
        try
        {
            while (true)
            {
                byte[] message = await RecieveMessage(stream);
                _playerNames.Enqueue(UnicodeEncoding.ASCII.GetString(message));
            }
        }
        catch (IOException e)
        {
            var temp = e.GetType();
            Trace.WriteLine(e);
        }
    }

    public async Task TestSend(int player)
    {
        try
        {
            await SendMessage(_streams[0], ASCIIEncoding.ASCII.GetBytes($"Hello: {player}"));
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }
        //for (int i = 0; i < _streams.Count; i++)
        //{
        //    if (i == player - 1)
        //        continue;
        //}
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
    ///  revieves message from client given by stream
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
