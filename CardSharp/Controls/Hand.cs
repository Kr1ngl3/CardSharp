using Avalonia.LogicalTree;
using Avalonia.Media;
using System;

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
    protected override double CardOffset => App.SCardSize.Width * Math.Min(1, (_handWidth - 1.0) / Math.Max(1, _cards.Count - 1));

    public double HandWidth => App.SCardSize.Width * _handWidth;
    public HandTypes HandType => _handType;
    public bool IsRotated => _angle != 0;

    public Hand(HandTypes handType, int handWidth, int angle) : base()
    {
        _handType = handType;
        _handWidth = handWidth;
        _angle = angle; 
        TransformGroup transform = new TransformGroup();
        RenderTransform = transform;
        transform.Children.Add(new RotateTransform(angle));
        if (angle != 0)
            transform.Children.Add(new TranslateTransform(-HandWidth/4, HandWidth/4));
    }

    public void SelectAll()
    {
        if (this.GetLogicalParent() is not Table table)
            return;
        table.AddSelectedCards(_cards);
    }
}
