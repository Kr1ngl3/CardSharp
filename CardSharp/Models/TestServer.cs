using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Models;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
public class TestServer : Server
{
    private int _playerCount;

    public TestServer(int playerCount)
    {
         _playerCount = playerCount;
    }

    public async override Task<string> GetHost()
    {
        return "Test Server";
    }

    public async override IAsyncEnumerable<string> GetNames()
    {
        yield return "Host";
        for (int i = 1; i < _playerCount; i++)
        {
            yield return $"Player {i}";
        }
    }

    public async override void CancelSearch()
    {
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

