using Avalonia.Controls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Models;

public class Card
{
    private static Random s_random = new Random();
    public enum Suits
    {
        Heart,
        Diamond,
        Spade,
        Club
    }

    public enum Ranks
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Joker
    }

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
        @"/Assets/heart.png",
        @"/Assets/diamond.png",
        @"/Assets/spade.png",
        @"/Assets/club.png"
    };

    private Ranks _rank;
    private Suits _suit;

    public Ranks Rank => _rank;
    public string Suit => GetSuitSource(_suit);

    public Card() : this(s_random.Next(14))
    {
    }
    public Card(int rank)
    {
        _suit = (Suits)s_random.Next(Enum.GetValues(typeof(Suits)).Length);
        _rank = (Ranks)rank;
    }

    private string GetSuitSource(Suits suit)
    {
        return s_iconSources[(int)suit];
    }

}