using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CardSharp.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CardSharp.Controls;

public class CardStack : Border
{
    /* 
     * TO-DO
     * option to rotate
     * option to show first 5 cards (maybe x)
     * maybe option to set as hand
     * oh! also make hand feature
     */
    private static Random s_random = new Random();
    private AvaloniaList<CardViewModel> _cards = new AvaloniaList<CardViewModel>();
    private static bool _added;

    public class CardStackChangedEventArgs : EventArgs
    {
        public IEnumerable<CardViewModel> CardsToShow;
        public IEnumerable<CardViewModel>? CardsToNotShow;

        public CardStackChangedEventArgs(IEnumerable<CardViewModel> cardsToShow, IEnumerable<CardViewModel>? cardsToNotShow)
        {
            CardsToShow = cardsToShow;
            CardsToNotShow = cardsToNotShow;
        }
    }

    private event Action<CardStackChangedEventArgs>? _cardStackChanged;
    public event Action<CardStackChangedEventArgs>? CardStackChanged
    {
        add
        {
            _cardStackChanged += value;
            if (!_added)
            {
                RaiseCardsChanged();
                _added = true;
            }
        }
        remove
        {
            _cardStackChanged -= value;
        }
    }

    /// <summary>
    /// Defines the Curve property.
    /// </summary>
    public static readonly DirectProperty<CardStack, string> CardCountProperty =
        AvaloniaProperty.RegisterDirect<CardStack, string>(
            nameof(CardCount),
            cs => cs.CardCount,
            (cs, s) => cs.CardCount = s);


    private string _cardCount = "0";

    /// <summary>
    /// Gets or sets how muc heach card is rotated depending on index from middle
    /// </summary>
    public string CardCount
    {
        get => _cardCount;
        set => SetAndRaise(CardCountProperty, ref _cardCount, value);
    }

    public CardStack(CardViewModel card) : this([card])
    {
    }

    public CardStack(IEnumerable<CardViewModel> cards)
    {
        _cards.CollectionChanged += (_, _) => CardCount = _cards.Count.ToString(); ;
        DataContext = this;
        _cards.AddRange(cards);
    }

    public void AddCards(CardStack cardStack)
    {
        AddCards(cardStack._cards);
    }

    public void AddCards(IEnumerable<CardViewModel> card)
    {
        List<CardViewModel> prevTopCards = new List<CardViewModel>(_cards.Skip(Math.Max(0, _cards.Count() - 2)));
        _cards.AddRange(card);
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

    public void SelectAll()
    {
        foreach (CardViewModel card in _cards)
            card.IsSelected = true;
    }

    public void Flip()
    {
        foreach (CardViewModel card in _cards)
            card.Flipped ^= true;
    }

    public void Shuffle()
    {
        List<CardViewModel> prevTopCards = new List<CardViewModel>(_cards.Skip(Math.Max(0, _cards.Count() - 2)));
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

    private void RaiseCardsChanged(IEnumerable<CardViewModel>? cardsToNotShow = null)
    {
        _cardStackChanged?.Invoke(new CardStackChangedEventArgs(_cards.Skip(Math.Max(0, _cards.Count() - 2)), cardsToNotShow));
    }
}