using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Platform;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.ViewModels;

public class GameViewModel : ViewModelBase
{
    private int _playerCount;
    private int _player;

    public event Action<(byte[] cardHashes, Point? point, int player, bool isStack)>? CardMoved;
    public void Move()
    {
        CardMoved?.Invoke(([16 + 13, 48 + 13], null, 0, false));
    }

    public void Move(byte[] cardHashes, Point point, int player, bool isStack)
    {
        foreach (var card in cardHashes)
            Trace.Write($"{card}, ");
        Trace.WriteLine("to: ");
        if (player != _playerCount)
            Trace.WriteLine($"player {player}");
        else
            Trace.WriteLine(point);
        Trace.WriteLine($"Is stack: {isStack}");
    }

    public int PlayerCount => _playerCount;
    public int Player => _player;

    public GameViewModel(int playerCount, int player)
    {
        _playerCount = playerCount;
        _player = player;
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
