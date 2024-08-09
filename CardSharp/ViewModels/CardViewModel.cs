using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Transformation;
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
    private TransformGroup _transform = new TransformGroup();

    public Ranks Rank => _rank;
    public string RankString => SRanksToSymbol[_rank];
    public DrawingImage Suit => GetSuitSource(_suit);
    public ISolidColorBrush Color => (int)_suit <= 1 ? Brushes.Red : Brushes.Black;
    public ISolidColorBrush Background => _isSelected ? Brushes.SkyBlue : Brushes.White;
    public bool IsSelected { get => _isSelected; set => this.RaiseAndSetIfChanged(ref _isSelected, value, nameof(Background)); }
    public bool Flipped { get => _flipped; set => this.RaiseAndSetIfChanged(ref _flipped, value, nameof(Flipped)); }
    public int AngleVal { set 
        {
            (_transform.Children[0] as RotateTransform)!.Angle = value;
            if (value == 90 || value == -90)
            {
                (_transform.Children[1] as TranslateTransform)!.Y = -App.SCardSize.Width / 4;
                (_transform.Children[1] as TranslateTransform)!.X = App.SCardSize.Width / 4;
            }
            else
            {
                (_transform.Children[1] as TranslateTransform)!.Y = 0;
                (_transform.Children[1] as TranslateTransform)!.X = 0;
            }
        }
    }
    public TransformGroup Angle => _transform;
    public byte Hash => (byte)((15 & (int)_suit) << 4 | (15 & (int)_rank));
    
    public CardViewModel(int rank, int suit)
    {
        _suit = (Suits)suit;
        _rank = (Ranks)rank;
        _transform.Children.Add(new RotateTransform(0));
        _transform.Children.Add(new TranslateTransform(0, 0));
    }

    public Suits GetSuitFromHash()
    {
        return (Suits)((Hash >> 4) & 15);
    }

    public Ranks GetRankFromHash()
    {
        return (Ranks)(Hash & 15);
    }

    private DrawingImage GetSuitSource(Suits suit)
    {
        if (_rank == Ranks.Joker)
            return new DrawingImage();
        return (DrawingImage)Application.Current!.FindResource(_suit.ToString())!;
    }
}
