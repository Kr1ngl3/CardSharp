﻿using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia;
using CardSharp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CardSharp.Controls;
public abstract class CardStackBase : Border
{
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

    protected static Random s_random = new Random();

    protected int _angle = 0;
    protected AvaloniaList<CardViewModel> _cards = new AvaloniaList<CardViewModel>();

    protected abstract int CardsToShow { get; }
    protected abstract double CardOffset { get; }

    public event Action<CardStackChangedEventArgs>? CardStackChanged;

    protected CardStackBase()
    {
        DataContext = this;
    }

    public bool ContainsCard(CardViewModel card)
    {
        return _cards.Contains(card);
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
        
        if (this is Hand { HandType: Hand.HandTypes.OtherHand })
        {
            foreach (CardViewModel card in cards)
            {
                card.Flipped = true;
                card.Angle = _angle;
            }
            // wait before shuffeling until after animation has played
            Dispatcher.UIThread.Post(async () => { await Task.Delay(350); Shuffle(); }); 
        }
        else
        {
            foreach (CardViewModel card in cards)
            {
                card.Flipped = false;
                card.Angle = _angle;
            }
        }
    }

    public void InsertCards(CardViewModel card, CardStackBase cardStack)
    {
        InsertCards(card, cardStack._cards);
    }

    public void InsertCards(CardViewModel atCard, IEnumerable<CardViewModel> cards)
    {
        _cards.InsertRange(_cards.IndexOf(atCard) + 1, cards);
        RaiseCardsChanged(_cards);
        foreach (CardViewModel card in cards)
        {
            card.Flipped = false;
            card.Angle = _angle;
        }
    }

    public void RemoveCard(CardViewModel card)
    {
        _cards.Remove(card);
        RaiseCardsChanged();
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

    public IEnumerable<CardViewModel> GetCardsToShow()
    {
        return new List<CardViewModel>(_cards.Skip(Math.Max(0, _cards.Count() - CardsToShow)));
    }

    protected void RaiseCardsChanged(IEnumerable<CardViewModel>? cardsToNotShow = null)
    {
        if (CardStackChanged is null)
            throw new Exception("Subscribe to event before modifying CardStack");

        CardStackChanged.Invoke(new CardStackChangedEventArgs(GetCardsToShow(), cardsToNotShow, CardOffset));
    }
}
