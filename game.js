class PiratBridgeClient {
  constructor() {
    this.socket = null;
    this.gameId = null;
    this.playerName = null;
    this.connected = false;

    this.initializeEventListeners();
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

      this.socket = new WebSocket("wss://pirat.mercantec.tech/ws");

      this.socket.onopen = () => {
        console.log("Forbundet til server");
        this.connected = true;
        resolve();
      };

      this.socket.onclose = () => {
        console.log("Forbindelse lukket");
        this.connected = false;
      };

      this.socket.onerror = (error) => {
        console.error("WebSocket fejl:", error);
        this.connected = false;
        reject(error);
      };

      this.socket.onmessage = (event) => {
        this.handleMessage(JSON.parse(event.data));
      };
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

    document.getElementById("lobby").classList.add("hidden");
    document.getElementById("game-board").classList.remove("hidden");

    const playersDiv = document.getElementById("players");
    playersDiv.innerHTML = state.players
      .map((p) => `<div>${p.name}: ${p.score} point</div>`)
      .join("");
  }
}

// Start spillet når siden er indlæst
window.addEventListener("load", () => {
  window.game = new PiratBridgeClient();
});
