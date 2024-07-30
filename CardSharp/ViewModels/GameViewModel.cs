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
    private Card _card;
    private Card _card2;
    public Card Card => _card;
    public Card Card2 => _card2;

    public GameViewModel()
    {
        _playerCount = 4;
        _card = new Card();
        _card2 = new Card();
    }
    public GameViewModel(int playerCount)
    {
        _playerCount = playerCount;
        _card = new Card();
        _card2 = new Card();
    }

}
