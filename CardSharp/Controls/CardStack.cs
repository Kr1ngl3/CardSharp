using Avalonia;
using CardSharp.Models;
using CardSharp.ViewModels;
using System;
using System.Collections.Generic;

namespace CardSharp.Controls;

public class CardStack : CardStackBase
{
    public enum CardStackTypes
    {
        Default,
        Show5
    }

    /// <summary>
    /// Defines the CardCount property.
    /// </summary>
    public static readonly DirectProperty<CardStack, string> CardCountProperty =
        AvaloniaProperty.RegisterDirect<CardStack, string>(
            nameof(CardCount),
            cardStack => cardStack.CardCount,
            (cardStack, cardCount) => cardStack.CardCount = cardCount);

    /// <summary>
    /// Defines the CardCount property.
    /// </summary>
    public static readonly DirectProperty<CardStack, double> CardStackWidthProperty =
        AvaloniaProperty.RegisterDirect<CardStack, double>(
            nameof(CardStackWidth),
            cardStack => cardStack.CardStackWidth);
    /* 
     * TO-DO
     * option to rotate
     * option to show first 5 cards (maybe x)
     * maybe option to set as hand
     * oh! also make hand feature
     */    
    private CardStackTypes _cardStackType;
    private string _cardCount = "0";


    protected override double CardOffset =>
        _cardStackType == CardStackTypes.Default ? 0
        : s_cardWidth * 5 / 12 / Math.Max(1, Math.Min(5, _cards.Count) - 1);

    protected override int CardsToShow =>
        _cardStackType == CardStackTypes.Default ? 2
        : _cardStackType == CardStackTypes.Show5 ? 5
        : _cards.Count;

    /// <summary>
    /// Gets or sets CardCount value
    /// </summary>
    public string CardCount { get => _cardCount; set => SetAndRaise(CardCountProperty, ref _cardCount, value); }

    /// <summary>
    /// Gets or sets CardCount value
    /// </summary>
    public double CardStackWidth => s_cardWidth + CardOffset * Math.Max(1, Math.Min(5, _cards.Count) - 1);

    public CardStack() : base()
    {
        _cards.CollectionChanged += (_, _) => CardCount = _cards.Count.ToString();
    }

    public bool IsEmpty()
    {
        return _cards.Count == 0;
    }

    public void Flip()
    {
        foreach (CardViewModel card in _cards)
            card.Flipped ^= true;
    }

    public void SetAsShow5Cards()
    {
        var temp = CardStackWidth;
        _cardStackType = CardStackTypes.Show5;
        RaisePropertyChanged(CardStackWidthProperty, temp, CardStackWidth);
        RaiseCardsChanged(_cards);
    }
}