using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Platform;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;

public class GameViewModel : ViewModelBase
{
    private int _playerCount;
    
    private AvaloniaList<Card> _cardList = new AvaloniaList<Card>();
    private AvaloniaList<Card> _topDeck = new AvaloniaList<Card>();

    public event Action<(byte, Point)>? CardMoved;
    public void Move()
    {
        CardMoved?.Invoke((48 + 13, new Point(500, 500)));
    }

    public IEnumerable<Card> CardList => _cardList;

    public GameViewModel()
    {
        _playerCount = 4;
        for (int i = 0; i < 6; i++)
            _cardList.Add(new Card());
    }
    public GameViewModel(int playerCount)
    {
        _playerCount = playerCount;
    }
    public void Add()
    {
        _cardList.Add(new Card());
        _topDeck.Clear();
        _topDeck.AddRange(_cardList.Skip(Math.Max(0, _cardList.Count() - 5)));
    }


    public IEnumerable<CardViewModel> CreateDeck(int jokerCount)
    {
        if (jokerCount > 4 || jokerCount < 0)
            throw new Exception($"Can't have {jokerCount} jokers");
        List<CardViewModel> cards = new List<CardViewModel>();
        for (int rank = 0; rank < 14; rank++)
        {
            for (int suit = rank == 13 ? 4 - jokerCount : 0; suit < 4; suit++)
            {
                cards.Add(new CardViewModel(rank, suit));
            }
        }
        return cards;
    }


}
