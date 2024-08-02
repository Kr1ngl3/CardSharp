using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using CardSharp.Controls;
using CardSharp.Converters;
using ReactiveUI;
using System.Diagnostics;

namespace CardSharp.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected Size _screenSize;

    public double ScreenWidth => _screenSize.Width;
    public double ScreenHeight => _screenSize.Height;

    public void UpdateScreenSize(Size screenSize)
    {
        _screenSize = screenSize;
        this.RaisePropertyChanged(nameof(ScreenWidth));
        this.RaisePropertyChanged(nameof(ScreenHeight));
    }
}
