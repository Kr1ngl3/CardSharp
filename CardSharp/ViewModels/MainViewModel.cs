﻿using Avalonia.Controls;
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
        _currentViewModel = new TitleScreenViewModel(() => CurrentViewModel = new HostViewModel(topLevel.Clipboard));
        _topLevel = topLevel;
    }
}
