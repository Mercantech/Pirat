using System.Net.WebSockets;

public class Player
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public System.Net.WebSockets.WebSocket Socket { get; set; }
    public List<Card> Hand { get; set; }
    public int Score { get; set; }
    public int? CurrentBet { get; set; }
    public int TricksTaken { get; set; }
    public int NumMatches { get; set; }

    public Player(string name, System.Net.WebSockets.WebSocket socket)
    {
        Id = Guid.NewGuid().ToString("N");
        Name = name;
        Socket = socket;
        Hand = new List<Card>();
        Score = 0;
        TricksTaken = 0;
    }
}
