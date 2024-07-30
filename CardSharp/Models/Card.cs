using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Models;

public class Card
{
    public enum Suits
    {
        Heart,
        Diamond,
        Spade,
        Club
    }

    private static string[] s_iconSources = {
        @"/Assets/heart.png",
        @"/Assets/diamond.png",
        @"/Assets/spade.png",
        @"/Assets/club.png"
    };

    private int _rank;
    private Suits _suit;

    public int Rank => _rank;
    public string Suit => GetSuitSource(_suit);

    public Card()
    {
        Random random = new Random();
        _suit = (Suits)random.Next(Enum.GetValues(typeof(Suits)).Length);
        _rank = random.Next(13);
    }

    private string GetSuitSource(Suits suit)
    {
        return s_iconSources[(int)suit];
    }

}