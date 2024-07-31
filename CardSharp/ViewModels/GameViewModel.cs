using Avalonia.Collections;
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
}
