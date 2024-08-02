using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;
public class CardViewModel : ViewModelBase
{
    private static Random s_random = new Random();

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
        { Ranks.Joker, "Jo"},
    };
    private static string[] s_iconSources = {
        @"avares://CardSharp/Assets/heart.png",
        @"avares://CardSharp/Assets/diamond.png",
        @"avares://CardSharp/Assets/spade.png",
        @"avares://CardSharp/Assets/club.png"
    };

    private Ranks _rank;
    private Suits _suit;

    public Ranks Rank => _rank;
    public string RankString => SRanksToSymbol[_rank];
    public Bitmap Suit => GetSuitSource(_suit);
    public IImmutableSolidColorBrush Color => (int)_suit <= 1 ? Brushes.Red : Brushes.Black;

    public CardViewModel() : this(s_random.Next(14))
    {          
    }          
    public CardViewModel(int rank)
    {
        _suit = (Suits)s_random.Next(Enum.GetValues(typeof(Suits)).Length);
        _rank = (Ranks)rank;
    }

    private Bitmap GetSuitSource(Suits suit)
    {
        return new Bitmap(AssetLoader.Open(new Uri(s_iconSources[(int)suit])));
    }
}
