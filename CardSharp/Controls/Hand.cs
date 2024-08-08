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
    private int _handWidth;

    protected override int CardsToShow => _cards.Count;

    protected override double CardOffset => s_cardWidth * Math.Min(1, (_handWidth - 1.0) / Math.Max(1, _cards.Count - 1));

    public double HandWidth => s_cardWidth * _handWidth;
    public HandTypes HandType => _handType;

    public Hand(HandTypes handType, int handWidth) : base()
    {
        _handType = handType;
        _handWidth = handWidth;
    }
}
