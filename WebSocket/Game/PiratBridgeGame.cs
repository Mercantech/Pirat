public class PiratBridgeGame
{
    private static readonly Random _random = new Random();
    public string GameId { get; private set; }
    public List<Player> Players { get; private set; }
    public GameState State { get; private set; }
    public int CurrentRound { get; private set; }
    public Player CurrentPlayer { get; private set; }

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

    public void TakeMatches(Player player, int numMatches)
    {
        player.NumMatches = numMatches;
    }

    public bool AllPlayersBet => Players.All(p => p.NumMatches > 0);

    public void StartNewRound()
    {
        // ...
        State = GameState.Betting;
        // ...
    }

    public void StartRound()
    {
        if (State != GameState.Betting || !AllPlayersBet)
        {
            throw new InvalidOperationException("Kan ikke starte en ny runde i øjeblikket");
        }

        // Nulstil spillernes tændstikker
        foreach (var player in Players)
        {
            player.NumMatches = 0;
        }

        // Bland kortene og del dem ud til spillerne
        ShuffleDeck();
        DealCards();

        // Sæt den første spiller som den aktuelle spiller
        CurrentPlayer = Players[0];

        // Sæt spiltilstanden til Playing
        State = GameState.Playing;

        // Forøg rundetælleren
        CurrentRound++;
    }

    private void ShuffleDeck()
    {
        // Implementer logikken for at blande kortene her
        // Eksempel:
        // _deck = _deck.OrderBy(x => _random.Next()).ToList();
    }

    private void DealCards()
    {
        // Implementer logikken for at dele kort ud til spillerne her
        // Eksempel:
        // foreach (var player in Players)
        // {
        //     var cards = _deck.Take(7).ToList();
        //     player.Hand.AddRange(cards);
        //     _deck.RemoveRange(0, 7);
        // }
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
