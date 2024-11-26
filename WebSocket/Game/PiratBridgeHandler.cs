using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class PiratBridgeHandler
{
    private readonly System.Net.WebSockets.WebSocket _webSocket;
    private static readonly List<PiratBridgeGame> Games = new();
    private Player _currentPlayer;

    public PiratBridgeHandler(System.Net.WebSockets.WebSocket webSocket)
    {
        _webSocket = webSocket;
    }

    public async Task HandleConnection()
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        string.Empty,
                        CancellationToken.None
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket Error: {ex.Message}");
            await SendError("Der opstod en fejl i forbindelsen");
        }
    }

    private async Task HandleMessage(string message)
    {
        try
        {
            Console.WriteLine($"Modtaget rå besked: {message}");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var command = JsonSerializer.Deserialize<GameCommand>(message, options);
            Console.WriteLine($"Deserialiseret kommando: {command}");

            if (string.IsNullOrEmpty(command?.Type))
            {
                await SendError("Ugyldig kommando: Type mangler");
                return;
            }

            switch (command.Type.ToUpperInvariant())
            {
                case "CREATE_GAME":
                    Console.WriteLine("Starter HandleCreateGame");
                    await HandleCreateGame(command);
                    break;
                case "JOIN_GAME":
                    await HandleJoinGame(command);
                    break;
                default:
                    await SendError($"Ukendt kommando: {command.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl i HandleMessage: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            await SendError("Der opstod en fejl ved behandling af kommandoen");
        }
    }

    private async Task HandleCreateGame(GameCommand command)
    {
        var playerName = command.Data.GetProperty("playerName").GetString();
        _currentPlayer = new Player(playerName, _webSocket);

        var game = new PiratBridgeGame();
        while (Games.Any(g => g.GameId == game.GameId))
        {
            game = new PiratBridgeGame();
        }

        game.AddPlayer(_currentPlayer);
        Games.Add(game);

        var response = new
        {
            type = "GAME_STATE",
            gameId = game.GameId,
            players = new[] { new { name = _currentPlayer.Name, score = _currentPlayer.Score } },
            state = game.State.ToString(),
            currentRound = game.CurrentRound
        };

        await SendToPlayer(_currentPlayer, response);
    }

    private async Task HandleJoinGame(GameCommand command)
    {
        var gameId = command.Data.GetProperty("gameId").GetString();
        var playerName = command.Data.GetProperty("playerName").GetString();

        var game = Games.FirstOrDefault(g => g.GameId == gameId);
        if (game == null)
        {
            await SendError("Spillet findes ikke");
            return;
        }

        _currentPlayer = new Player(playerName, _webSocket);
        if (!game.AddPlayer(_currentPlayer))
        {
            await SendError("Kan ikke tilslutte til spillet");
            return;
        }

        await BroadcastGameState(game);
    }

    private async Task SendGameState(PiratBridgeGame game)
    {
        var response = new
        {
            type = "GAME_STATE",
            gameId = game.GameId,
            players = game.Players.Select(p => new { p.Name, p.Score }),
            state = game.State.ToString(),
            currentRound = game.CurrentRound
        };

        await SendMessage(response);
    }

    private async Task SendError(string message)
    {
        var error = new { type = "ERROR", message = message };

        if (_currentPlayer != null)
        {
            await SendToPlayer(_currentPlayer, error);
        }
        else
        {
            // Send direkte til WebSocket hvis der ikke er en spiller endnu
            var json = JsonSerializer.Serialize(error);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }

    private async Task SendMessage(object data)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private async Task BroadcastGameState(PiratBridgeGame game)
    {
        var response = new
        {
            type = "GAME_STATE",
            gameId = game.GameId,
            players = game.Players.Select(p => new { name = p.Name, score = p.Score }).ToList(),
            state = game.State.ToString(),
            currentRound = game.CurrentRound
        };

        foreach (var player in game.Players)
        {
            await SendToPlayer(player, response);
        }
    }

    private async Task SendToPlayer(Player player, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await player.Socket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }
}

public class GameCommand
{
    public string Type { get; set; } = string.Empty;
    public JsonElement Data { get; set; }

    public override string ToString()
    {
        return $"Type: {Type}, Data: {Data}";
    }
}
