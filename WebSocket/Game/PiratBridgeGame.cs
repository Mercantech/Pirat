public class PiratBridgeGame
{
    private static readonly Random _random = new Random();
    public string GameId { get; private set; }
    public List<Player> Players { get; private set; }
    public GameState State { get; private set; }
    public int CurrentRound { get; private set; }

    public PiratBridgeGame()
    {
        GameId = GenerateSimpleId();
        Players = new List<Player>();
        State = GameState.WaitingForPlayers;
        CurrentRound = 1;
    }

    private string GenerateSimpleId()
    {
        return _random.Next(100, 1000).ToString(); // Genererer et tal mellem 100-999
    }

    public bool AddPlayer(Player player)
    {
        if (Players.Count >= 4 || State != GameState.WaitingForPlayers)
            return false;

        Players.Add(player);

        if (Players.Count == 4)
            State = GameState.Ready;

        return true;
    }
}

public enum GameState
{
    WaitingForPlayers,
    Ready,
    Betting,
    Playing,
    RoundEnd,
    GameEnd
}
