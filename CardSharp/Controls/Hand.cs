using CardSharp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class Hand : CardStackBase
{
    public enum HandTypes
    {
        MainHand,
        OtherHand
    }

    private HandTypes _handType;
    private double _handWidth;

    protected override int CardsToShow => _cards.Count;

    protected override double CardOffset => s_cardWidth * Math.Min(1, 9.0 / Math.Max(1, _cards.Count - 1));

    public double HandWidth => _handWidth;

    public Hand(HandTypes handType, double handWidth) : base()
    {
        _handType = handType;
        _handWidth = handWidth;
    }

    public void AddCardsAt(CardViewModel card, IEnumerable<CardViewModel> cards)
    {
        _cards.InsertRange(_cards.IndexOf(card) + 1, cards);
        RaiseCardsChanged(_cards);
    }
}
