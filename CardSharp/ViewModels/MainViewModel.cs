using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace CardSharp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private TopLevel _topLevel;

    // property and backingfield for the current view model, changes what is shown based on view model and the viewlocator
    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel { get => _currentViewModel; set => this.RaiseAndSetIfChanged(ref _currentViewModel, value, nameof(CurrentViewModel)); }

    public MainViewModel(TopLevel topLevel)
    {
        _currentViewModel = new TitleScreenViewModel(() => CurrentViewModel = new HostViewModel(i => CurrentViewModel = new GameViewModel(i), topLevel.Clipboard));
        _topLevel = topLevel;
        _topLevel.SizeChanged += OnSizeChanged;

        if (_topLevel.InsetsManager is not null)
        {
            _topLevel.InsetsManager.DisplayEdgeToEdge = true;
            _topLevel.InsetsManager.IsSystemBarVisible = false;
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Size screen = (sender as TopLevel)!.ClientSize;

        if (screen.AspectRatio > 16.0 / 9)
            screen = new Size(screen.Height / 9 * 16, screen.Height);
        else if (screen.AspectRatio < 16.0 / 9)
            screen = new Size(screen.Width, screen.Width / 16 * 9);

        UpdateScreenSize(screen);
        _currentViewModel.UpdateScreenSize(screen);
    }
}
