using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CardSharp.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CardSharp.Controls;

public class CardStack : Border
{
    public enum CardStackTypes
    {
        Default,
        Show5,
        Hand
    }

    public class CardStackChangedEventArgs : EventArgs
    {
        public IEnumerable<CardViewModel> CardsToShow;
        public IEnumerable<CardViewModel>? CardsToNotShow;
        public double CardOffset;

        public CardStackChangedEventArgs(IEnumerable<CardViewModel> cardsToShow, IEnumerable<CardViewModel>? cardsToNotShow, double cardOffset)
        {
            CardsToShow = cardsToShow;
            CardsToNotShow = cardsToNotShow;
            CardOffset = cardOffset;
        }
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
    private static Random s_random = new Random();
    private static double s_cardWidth = (double)Application.Current!.FindResource("CardWidth")!;
    
    private AvaloniaList<CardViewModel> _cards = new AvaloniaList<CardViewModel>();
    private CardStackTypes _cardStackType;
    private string _cardCount = "0";


    private double CardOffset =>
        _cardStackType == CardStackTypes.Default ? 0
        : _cardStackType == CardStackTypes.Show5 ? (s_cardWidth * 5 / 12 / Math.Max(1, Math.Min(5, _cards.Count) - 1))
        : 0;


    private int CardsToShow =>
        _cardStackType == CardStackTypes.Default ? 2
        : _cardStackType == CardStackTypes.Show5 ? 5
        : 0;

    public event EventHandler<CardStackChangedEventArgs>? CardStackChanged;

    /// <summary>
    /// Gets or sets CardCount value
    /// </summary>
    public string CardCount { get => _cardCount; set => SetAndRaise(CardCountProperty, ref _cardCount, value); }

    /// <summary>
    /// Gets or sets CardCount value
    /// </summary>
    public double CardStackWidth => s_cardWidth + CardOffset * Math.Max(1, Math.Min(5, _cards.Count) - 1);

    public CardStack()
    {
        _cards.CollectionChanged += (_, _) => CardCount = _cards.Count.ToString(); ;
        DataContext = this;
    }

    public void AddCards(CardStack cardStack)
    {
        AddCards(cardStack._cards);
    }

    public void AddCards(IEnumerable<CardViewModel> cards)
    {
        List<CardViewModel> prevTopCards = new List<CardViewModel>(GetCardsToShow());
        prevTopCards.AddRange(cards);

        _cards.AddRange(cards);
        RaiseCardsChanged(prevTopCards);
    }

    public void RemoveCard(CardViewModel card)
    {
        _cards.Remove(card);
        RaiseCardsChanged();
    }

    public bool ContainsCard(CardViewModel card)
    {
        return _cards.Contains(card);
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

    public void Shuffle()
    {
        List<CardViewModel> prevTopCards = new List<CardViewModel>(GetCardsToShow());
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = s_random.Next(n + 1);
            CardViewModel value = _cards[k];
            _cards[k] = _cards[n];
            _cards[n] = value;
        }
        RaiseCardsChanged(prevTopCards);
    }

    public void Show5Cards()
    {
        var temp = CardStackWidth;
        _cardStackType = CardStackTypes.Show5;
        RaisePropertyChanged(CardStackWidthProperty, temp, CardStackWidth);
        RaiseCardsChanged(_cards);
    }

    public IEnumerable<CardViewModel> GetCardsToShow()
    {
        return new List<CardViewModel>(_cards.Skip(Math.Max(0, _cards.Count() - CardsToShow)));
    }

    private void RaiseCardsChanged(IEnumerable<CardViewModel>? cardsToNotShow = null)
    {
        if (CardStackChanged is null)
            throw new Exception("Subscribe to event before modifying CardStack");

        CardStackChanged.Invoke(this, new CardStackChangedEventArgs(GetCardsToShow(), cardsToNotShow, CardOffset));
    }
}