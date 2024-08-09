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

    public event Action<(byte[] cardHashes, Point? point, int player, bool isStack)>? CardMoved;
    public void Move()
    {
        CardMoved?.Invoke(([16 + 13, 48 + 13], null, 5, false));
    }

    public int PlayerCount => _playerCount;

    public GameViewModel(int playerCount)
    {
        _playerCount = playerCount;
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
