using Renci.SshNet;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ReactiveUI;
using Avalonia.Input.Platform;
using System;
using Avalonia.Rendering;
using Avalonia.Threading;
using System.Diagnostics;
using CardSharp.Models;

namespace CardSharp.ViewModels;

public class HostViewModel : ViewModelBase
{
    private Server _server = new Server();
    private IClipboard? _clipboard;
    private string _greeting = "Welcome";
    public string Greeting { get => _greeting; set => this.RaiseAndSetIfChanged(ref _greeting, value, nameof(Greeting)); }


    public HostViewModel(IClipboard? clipBoard)
    {
        _clipboard = clipBoard;
    }

    public async Task Host()
    {
        Greeting = await _server.GetHost();
    }

    public async Task Copy()
    {
        await _clipboard!.SetTextAsync(Greeting);
    }
}