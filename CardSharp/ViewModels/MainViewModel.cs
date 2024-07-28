using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;

public class MainViewModel : ViewModelBase
{
    // property and backingfield for the current view model, changes what is shown based on view model and the viewlocator
    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel { get => _currentViewModel; set => this.RaiseAndSetIfChanged(ref _currentViewModel, value, nameof(CurrentViewModel)); }

    public MainViewModel()
    {
        _currentViewModel = new ConnectViewModel();
    }
}
