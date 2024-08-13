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
using Avalonia.Collections;
using System.Collections.Generic;

namespace CardSharp.ViewModels;

public class HostViewModel : ViewModelBase
{
    private Server _server = new Server();
    private Action<ViewModelBase> _changeViewModel;
    private IClipboard? _clipboard;
    private string _greeting = "Welcome";
    private AvaloniaList<string> _names = new AvaloniaList<string>();

    public string Greeting { get => _greeting; set => this.RaiseAndSetIfChanged(ref _greeting, value, nameof(Greeting)); }
    public IEnumerable<string> Names => _names;

    public HostViewModel(Action<ViewModelBase> changeViewModel, IClipboard? clipBoard)
    {
        _clipboard = clipBoard;
        _changeViewModel = changeViewModel;
    }

    public async Task Host()
    {
        Greeting = await _server.GetHost();
        await foreach (string name in _server.GetNames())
            _names.Add(name);
    }

    int i = 0;
    public async Task Cancel()
    {
        //_server.CancelSearch();
        //Greeting = "Started game";
        //_changeViewModel(new GameViewModel(_names.Count, 0));
        await _server.TestSend(i++);
    }

    public async Task Copy()
    {
        await _clipboard!.SetTextAsync(Greeting);
    }
}