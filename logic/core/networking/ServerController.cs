using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.util;
using MPAutoChess.logic.menu;
using MPAutoChess.seasons.season0;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

public partial class ServerController : Node {

    public const string SERVER_IP = "127.0.0.1";
    // TODO: to run multiple lobbies (and therefore servers) on the same machine, this needs to be loaded from the command line params
    // and a matchmaking server (probably with NodeJS) starts servers on free ports and sends the port to the client
    public const int SERVER_PORT = 8080;
    
    [Export] public PackedScene PlayerScene { get; set; }
    
    public bool IsServer { get; private set; }
    public Player[] Players { get; private set; }
    private Dictionary<long, Player> accountIdToPlayer = new Dictionary<long, Player>();
    private Dictionary<string, Player> secretToPlayer = new Dictionary<string, Player>();
    private Dictionary<int, Player> peerIdToPlayer = new Dictionary<int, Player>();
    private Dictionary<Player, bool> readyPlayers = new Dictionary<Player, bool>();

    public GameSession GameSession { get; private set; }

    public static ServerController Instance { get; private set; }

    public override void _EnterTree() {
        if (Instance == null) Instance = this;
        else GD.PrintErr("Multiple ServerController instances detected, this should not happen!");
    }

    public override void _ExitTree() {
        if (Instance == this) Instance = null;
        else GD.PrintErr("Exiting ServerController that is not the current instance, this should not happen!");
    }

    public override void _Ready() {
        ProtoBufSettings.Set();
        GameSession = new GameSession();
        GameSession.Name = "GameSession";
        
        IsServer = OS.HasFeature("dedicated_server");
        CallDeferred(IsServer ? MethodName.SetupServer : MethodName.SetupClient);
    }

    private void SetupServer() {
        GD.Print("Setting up server...");
        
        ENetMultiplayerPeer serverPeer = new ENetMultiplayerPeer();
        serverPeer.CreateServer(SERVER_PORT);
        GetTree().GetMultiplayer().MultiplayerPeer = serverPeer;
        
        // read the secret codes of players that are passed via command line arguments
        // the format is: players=accountId:secretCode,accountId:secretCode,...
        string[] args = OS.GetCmdlineArgs();
        string? playersArg = args.FirstOrDefault(arg => arg.StartsWith("players="));
        if (playersArg == null) throw new ArgumentException("No players argument found in command line arguments. Expected format: players=accountId:secretCode,accountId:secretCode,...");
        string playersString = playersArg.Substring("players=".Length);
        string[] playerPairs = playersString.Split(',');
        Players = new Player[playerPairs.Length];
        for (int i = 0; i < Players.Length; i++) {
            string[] parts = playerPairs[i].Split(':');
            if (parts.Length != 2) {
                throw new ArgumentException($"Invalid player format: {playerPairs[i]}. Expected format: accountId:secretCode");
            }
            if (!long.TryParse(parts[0], out long accountId)) {
                throw new ArgumentException($"Invalid account ID: {parts[0]}");
            }
            Player player = PlayerScene.Instantiate<Player>();
            player.Name = $"Player{i}";
            player.Initialize(Account.FindById(accountId));
            PlayerController controller = new PlayerController(player);
            player.AddChild(controller);
            Players[i] = player;
            accountIdToPlayer[accountId] = player;
            secretToPlayer[parts[1]] = player;
            readyPlayers[player] = false;
            GameSession.AddChild(player);
        }
        
        GameSession.Initialize(new Season0(), new EchoMode(), Players); // TODO: read season and mode from command line arguments
        GD.Print("Game session initialized with " + Players.Length + " players.");
    }

    private void SetupClient() {
        GD.Print("Setting up client...");
        LoadingScreen.Instance.SetStage(LoadingStage.CONNECTING);
        ENetMultiplayerPeer clientPeer = new ENetMultiplayerPeer();
        clientPeer.CreateClient(SERVER_IP, SERVER_PORT);
        GetTree().GetMultiplayer().MultiplayerPeer = clientPeer;
        clientPeer.PeerConnected += (id) => {
            GD.Print("Connected to server with peer ID: " + id);
            CallDeferred(Node.MethodName.Rpc, MethodName.Connect, "secret"); // TODO: load secret code
        };
        GD.Print("Client setup complete");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void Connect(string playerSecret) {
        if (!IsServer) throw new InvalidOperationException("Connect can only be called on the server.");
        
        if (!secretToPlayer.TryGetValue(playerSecret, out Player player)) {
            GD.PrintErr("Player with secret " + playerSecret + " not found.");
            return;
        }
        GD.Print("Connection request from player: " + player.Name);
        int senderId = Multiplayer.GetRemoteSenderId();
        peerIdToPlayer[senderId] = player;
        byte[] serializedGameSession = SerializerExtensions.Serialize(GameSession);
        CallDeferred(Node.MethodName.Rpc, MethodName.TransferGameSession, Players.Select(p => p.Account.Id).ToArray(), serializedGameSession);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private async void TransferGameSession(long[] accountIds, byte[] serializedGameSession) {
        if (IsServer) throw new InvalidOperationException("TransferGameSession can only be called on the client.");
        
        LoadingScreen.Instance.SetStage(LoadingStage.LOADING_GAME);
        
        // Prepare Player instances so Serializer can deserialize into them
        Players = new Player[accountIds.Length];
        for (int i = 0; i < accountIds.Length; i++) {
            Player player = PlayerScene.Instantiate<Player>();
            player.Initialize(Account.FindById(accountIds[i]));
            player.Name = $"Player{i}";
            Players[i] = player;
            GameSession.AddChild(player);
        }
        
        await ToSignal(GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout); // give the engine time to initialize the GameSession and player nodes, so we can find and deserialize into the properly
        
        GameSession receivedGameSession = SerializerExtensions.Deserialize<GameSession>(serializedGameSession);
        if (receivedGameSession != GameSession) {
            GD.PrintErr("Deserialized GameSession does not match the current instance, this should not happen!");
            GameSession = receivedGameSession; // fallback to the deserialized instance
        }
        
        // // create PlayerController for local player
        foreach (Player player in GameSession.Players) {
            if (!player.Account.Equals(Account.GetCurrent())) continue;
            
            PlayerController controller = new PlayerController(player);
            player.AddChild(controller);
            GD.Print("PlayerController created for local player.");
            break;
        }

        Rpc(MethodName.Ready);
        LoadingScreen.Instance.SetStage(LoadingStage.WAITING_FOR_PLAYERS);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void Ready() {
        if (!IsServer) throw new InvalidOperationException("Ready can only be called on the server.");

        Player player = peerIdToPlayer.GetValueOrDefault(Multiplayer.GetRemoteSenderId());
        if (player == null) {
            GD.PrintErr("[ServerController.Ready()] Player not found for peer ID: " + Multiplayer.GetRemoteSenderId());
            return;
        }
        
        readyPlayers[player] = true;
        if (readyPlayers.Values.All(ready => ready)) {
            GD.Print("All players are ready, starting game...");
            Rpc(MethodName.StartGame);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void StartGame() {
        LoadingScreen.Instance.SetStage(LoadingStage.STARTING);
        AddChild(GameSession);
        GameSession.Start();
        LoadingScreen.Instance.SetStage(LoadingStage.STARTED);
    }

    public void OnShopChange(Shop shop) {
        if (!IsServer) throw new InvalidOperationException("OnShopChange can only be called on the server.");
        
        int targetPeerId = 0;
        foreach (KeyValuePair<int, Player> keyValuePair in peerIdToPlayer) {
            if (keyValuePair.Value.Shop == shop) {
                targetPeerId = keyValuePair.Key;
                break;
            }
        }
        if (targetPeerId == 0) {
            GD.PrintErr("Shop seems to not be owned by any player.");
            return;
        }

        RpcId(targetPeerId, MethodName.TransferShop, SerializerExtensions.Serialize(shop));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void TransferShop(byte[] serializedShop) {
        if (IsServer) throw new InvalidOperationException("TransferShop can only be called on the client.");
        
        Shop transferredShop = SerializerExtensions.Deserialize<Shop>(serializedShop);
        if (transferredShop != PlayerController.Current.Player.Shop) // Shop is Identifiable, so deserialize should merge into the existing instance
            GD.PrintErr($"Received shop does not match the current player's shop, this should not happen! Received: {transferredShop.Id} | Present: {PlayerController.Current.Player.Shop.Id}");
    }
    
    public void RunInContext(Action action, PlayerController? controller = null) {
        if (!IsServer) throw new InvalidOperationException("RunInContext can only be called on the server.");
        
        int peerId = Multiplayer.GetRemoteSenderId();
        if (peerId == 0)
            throw new InvalidOperationException("RunInContext ran outside of the context of an RPC, which is not allowed.");
        if (!peerIdToPlayer.TryGetValue(peerId, out Player player))
            throw new InvalidOperationException($"Peer ID {peerId} does not correspond to any player, cannot run action in context.");

        PlayerController allowedController = PlayerController.GetForPlayer(player);
        if (controller != null && controller != allowedController)
            throw new InvalidOperationException($"Action can only be run in the context of the player {allowedController.Player.Name}, not {controller.Player.Name}.");
        
        allowedController.RunInContext(action);
    }
}