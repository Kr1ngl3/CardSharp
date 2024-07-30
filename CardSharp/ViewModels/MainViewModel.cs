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
        _topLevel.Loaded += _topLevel_Loaded;

        if (_topLevel.InsetsManager is not null)
        {
            _topLevel.InsetsManager.DisplayEdgeToEdge = true;
            _topLevel.InsetsManager.IsSystemBarVisible = false;
        }
    }

    private void _topLevel_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var temp = _topLevel.Height;
        var temp2 = _topLevel.Width;
    }
}
