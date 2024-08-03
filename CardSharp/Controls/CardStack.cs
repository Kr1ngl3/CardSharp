using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;

namespace CardSharp.Controls;

public class CardStack : Border
{
    AvaloniaList<Card> _cards = new AvaloniaList<Card>();

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

#if DEBUG
    public CardStack() : this([new Card()])
    {
    }
#endif

    public CardStack(Card card) : this([card])
    {
    }

    public CardStack(IEnumerable<Card> cards)
    {
        _cards.CollectionChanged += (_, _) => CardCount = _cards.Count.ToString(); ;
        DataContext = this;
        _cards.AddRange(cards);
    }

    public void AddCards(IEnumerable<Card> card)
    {
        _cards.AddRange(card);
    }

    public void RemoveCard(Card card)
    {
        _cards.Remove(card);
    }

    public bool ContainsCard(Card card)
    {
        return _cards.Contains(card);
    }

    public bool IsEmpty()
    {
        return _cards.Count == 0;
    }
}