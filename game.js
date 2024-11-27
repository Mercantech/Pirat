class PiratBridgeClient {
  constructor() {
    this.socket = null;
    this.gameId = null;
    this.playerName = null;
    this.connected = false;

    this.initializeEventListeners();
    this.fetchCards();
  }

  initializeEventListeners() {
    document.getElementById("create-game").addEventListener("click", () => {
      this.playerName = prompt("Indtast dit navn:");
      if (this.playerName) {
        this.connectAndCreateGame();
      }
    });

    document.getElementById("join-game").addEventListener("click", () => {
      this.gameId = prompt("Indtast spil-ID:");
      this.playerName = prompt("Indtast dit navn:");
      if (this.gameId && this.playerName) {
        this.connectAndJoinGame();
      }
    });
  }

  async connectAndCreateGame() {
    try {
      if (!this.connected) {
        await this.connectWebSocket();
      }
      console.log("Sender CREATE_GAME kommando:", {
        playerName: this.playerName,
      });
      this.sendCommand("CREATE_GAME", { playerName: this.playerName });
    } catch (error) {
      console.error("Fejl ved oprettelse af spil:", error);
    }
  }

  async connectAndJoinGame() {
    try {
      if (!this.connected) {
        await this.connectWebSocket();
      }
      this.sendCommand("JOIN_GAME", {
        gameId: this.gameId,
        playerName: this.playerName,
      });
    } catch (error) {
      console.error("Fejl ved tilslutning til spil:", error);
    }
  }

  async connectWebSocket() {
    return new Promise((resolve, reject) => {
      if (this.connected && this.socket?.readyState === WebSocket.OPEN) {
        resolve();
        return;
      }

      const urls = [
        "ws://localhost:5293/ws",
        "wss://pirat.mercantec.tech/ws",
      ];

      const tryConnect = (index) => {
        if (index >= urls.length) {
          reject(new Error("Kunne ikke oprette forbindelse til nogen af de angivne URLs"));
          return;
        }

        this.socket = new WebSocket(urls[index]);

        this.socket.onopen = () => {
          console.log(`Forbundet til server: ${urls[index]}`);
          this.connected = true;
          resolve();
        };

        this.socket.onclose = () => {
          console.log(`Forbindelse lukket: ${urls[index]}`);
          this.connected = false;
          tryConnect(index + 1);
        };

        this.socket.onerror = (error) => {
          console.error(`WebSocket fejl: ${urls[index]}`, error);
          this.connected = false;
          tryConnect(index + 1);
        };

        this.socket.onmessage = (event) => {
          this.handleMessage(JSON.parse(event.data));
        };
      };

      tryConnect(0);
    });
  }

  sendCommand(type, data) {
    const command = { type, data };
    console.log("Sender kommando:", command);
    this.socket.send(JSON.stringify(command));
  }

  handleMessage(message) {
    switch (message.type) {
      case "GAME_STATE":
        this.updateGameState(message);
        break;
      case "ERROR":
        alert(message.message);
        break;
    }
  }

  updateGameState(state) {
    if (!this.gameId) {
      this.gameId = state.gameId;
      alert(`Dit spil-ID er: ${this.gameId}`);
    }
    const bettingControls = document.getElementById("betting-controls");
    bettingControls.classList.toggle("hidden", state.state !== "Betting");

    document.getElementById("lobby").classList.add("hidden");
    document.getElementById("game-board").classList.remove("hidden");

    const playersDiv = document.getElementById("players");
    playersDiv.innerHTML = state.players
      .map((player) => {
        console.log("Player data:", player); // Debug logging
        return `<div>${player.name || "Ukendt"}: ${
          player.score || 0
        } point</div>`;
      })
      .join("");
  }

  async fetchCards() {
    const ranks = ['2', '3', '4', '5', '6', '7', '8', '9', '10', 'J', 'Q', 'K', 'A'];
    const suits = ['C', 'D', 'H', 'S'];
    const cards = [];

    for (const suit of suits) {
      for (const rank of ranks) {
        cards.push({ rank, suit });
      }
    }
    cards.push({ rank: 'Joker', suit: 'J' });

    this.displayCards(cards);
  }

  displayCards(cards) {
    const cardDisplay = document.getElementById('card-display');
    const suitOrder = ['S', 'H', 'D', 'C', 'J'];
    const suitNames = {
      'S': 'Spar',
      'H': 'Hjerter',
      'D': 'Ruder',
      'C': 'Klør',
      'J': 'Joker'
    };

    const cardsBySuit = suitOrder.map(suit => {
      const suitCards = cards.filter(card => card.suit === suit);
      return `
        <div class="card-suit">
          <h3>${suitNames[suit]}</h3>
          ${suitCards.map(card => `
            <div class="card">
              <img src="./images/cards/${card.rank}${card.suit}.svg" alt="${card.rank} of ${card.suit}">
            </div>
          `).join('')}
        </div>
      `;
    }).join('');

    cardDisplay.innerHTML = cardsBySuit;
  }
}

// Start spillet når siden er indlæst
window.addEventListener("load", () => {
  window.game = new PiratBridgeClient();
});

document.getElementById("take-matches").addEventListener("click", () => {
  const numMatches = parseInt(document.getElementById("num-matches").value);
  this.sendCommand("TAKE_MATCHES", { gameId: this.gameId, numMatches });
});