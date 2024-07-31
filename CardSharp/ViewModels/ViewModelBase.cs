using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System.Diagnostics;

namespace CardSharp.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private static Size s_screenSize;

    public GridLength ScreenWidth => new GridLength(s_screenSize.Width);
    public GridLength ScreenHeight => new GridLength(s_screenSize.Height);
    // TO-DO make converter and have doubles here instead


    protected void SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Size screen = (sender as TopLevel)!.ClientSize;

        if (screen.AspectRatio == 16.0 / 9)
            s_screenSize = screen;
        else if (screen.AspectRatio > 16.0/9)
            s_screenSize = new Size(screen.Height / 9 * 16, screen.Height);
        else
            s_screenSize = new Size(screen.Width, screen.Width / 16 * 9);

        this.RaisePropertyChanged(nameof(ScreenHeight));
        this.RaisePropertyChanged(nameof(ScreenWidth));
    }
}
