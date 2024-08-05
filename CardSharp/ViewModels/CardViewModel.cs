using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CardSharp.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;
public class CardViewModel : ViewModelBase
{
    public static Dictionary<Ranks, string> SRanksToSymbol = new Dictionary<Ranks, string>()
    {
        { Ranks.Ace, "A"},
        { Ranks.Two, "2"},
        { Ranks.Three, "3"},
        { Ranks.Four, "4"},
        { Ranks.Five, "5"},
        { Ranks.Six, "6"},
        { Ranks.Seven, "7"},
        { Ranks.Eight, "8"},
        { Ranks.Nine, "9"},
        { Ranks.Ten, "10"},
        { Ranks.Jack, "J"},
        { Ranks.Queen, "Q"},
        { Ranks.King, "K"},
        { Ranks.Joker, "J\nO\nK\nE\nR"},
    };

    private Ranks _rank;
    private Suits _suit;
    private bool _isSelected;
    private bool _flipped;

    public Ranks Rank => _rank;
    public string RankString => SRanksToSymbol[_rank];
    public DrawingImage Suit => GetSuitSource(_suit);
    public ISolidColorBrush Color => (int)_suit <= 1 ? Brushes.Red : Brushes.Black;
    public ISolidColorBrush Background => _isSelected ? Brushes.SkyBlue : Brushes.White;
    public bool IsSelected { get => _isSelected; set => this.RaiseAndSetIfChanged(ref _isSelected, value, nameof(Background)); }
    public bool Flipped { get => _flipped; set => this.RaiseAndSetIfChanged(ref _flipped, value, nameof(Flipped)); }
    
    public CardViewModel(int rank, int suit)
    {
        _suit = (Suits)suit;
        _rank = (Ranks)rank;
    }

    private DrawingImage GetSuitSource(Suits suit)
    {
        if (_rank == Ranks.Joker)
            return new DrawingImage();
        return (DrawingImage)Application.Current!.FindResource(_suit.ToString())!;
    }
}
