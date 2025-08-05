using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.menu;
using MPAutoChess.logic.util;
using MPAutoChess.seasons.season0;

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
    
    private double accumulatedTime = 0.0;

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
    
    private int GetPeerIdFor(Player player) {
        return peerIdToPlayer.FirstOrDefault(kvp => kvp.Value == player).Key;
    }

    private void SetupServer() {
        GD.Print("Setting up server...");
        
        PlayerUI.Instance.QueueFree();
        
        ENetMultiplayerPeer serverPeer = new ENetMultiplayerPeer();
        serverPeer.CreateServer(SERVER_PORT);
        GetTree().GetMultiplayer().MultiplayerPeer = serverPeer;
            
        string[] args = OS.GetCmdlineArgs();
        
        string? gameModeArg = args.FirstOrDefault(arg => arg.StartsWith("mode="));
        if (gameModeArg == null) throw new ArgumentException("No game mode argument found in command line arguments. Expected format: mode=GameModeName");
        string gameModeString = gameModeArg.Substring("mode=".Length);
        GameMode gameMode = GameMode.GetByName(gameModeString);
        
        // read the secret codes of players that are passed via command line arguments
        // the format is: players=accountId:secretCode,accountId:secretCode,...
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
            player.SetAccount(Account.FindById(accountId));
            PlayerController controller = new PlayerController(player);
            controller.Name = $"Player{i}Controller";
            player.AddChild(controller);
            player.Position = new Vector2(i * 1000, 0); // make sure arenas are spaced out (how far does not matter)
            Players[i] = player;
            accountIdToPlayer[accountId] = player;
            secretToPlayer[parts[1]] = player;
            readyPlayers[player] = false;
        }
        
        GameSession.Initialize(new Season0(), gameMode, Players); // TODO: read season and mode from command line arguments
        GD.Print("Game session initialized with " + Players.Length + " players.");  
    }

    private void SetupClient() {
        GD.Print("Setting up client...");
        LoadingScreen.Instance.SetStage(LoadingStage.CONNECTING);
        ENetMultiplayerPeer clientPeer = new ENetMultiplayerPeer();
        clientPeer.CreateClient(SERVER_IP, SERVER_PORT);
        GetTree().GetMultiplayer().MultiplayerPeer = clientPeer;
        clientPeer.PeerConnected += (id) => {
            NodeExtensions.ServerPeerId = id;
            GD.Print($"Connected to server with peer ID: {id}. Is server: {GetTree().GetMultiplayer().IsServer()}.");
            Error error = this.RpcToServer(MethodName.Connect, Account.GetCurrent().SecretKey);
            GD.Print("Called Connect RPC on server : " + error);
            if (error != Error.Ok) {
                GD.PrintErr("Failed to call Connect RPC on server: " + error);
            }
        };
        GD.Print("Client setup complete");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void Connect(string playerSecret) {
        if (!IsServer) throw new InvalidOperationException("Connect can only be called on the server.");
        
        GD.Print("Received connection request with secret: " + playerSecret);
        
        if (!secretToPlayer.TryGetValue(playerSecret, out Player player)) {
            GD.PrintErr("Player with secret " + playerSecret + " not found.");
            return;
        }
        GD.Print("Connection request from player: " + player.Name);
        int senderId = Multiplayer.GetRemoteSenderId();
        peerIdToPlayer[senderId] = player;
        
        byte[] serializedGameSession = SerializerExtensions.Serialize(GameSession);
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.TransferGameSession, serializedGameSession);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void TransferGameSession(byte[] serializedGameSession) {
        if (IsServer) throw new InvalidOperationException("TransferGameSession can only be called on the client.");
        
        LoadingScreen.Instance.SetStage(LoadingStage.LOADING_GAME);
        
        GameSession receivedGameSession = SerializerExtensions.Deserialize<GameSession>(serializedGameSession);
        if (receivedGameSession != GameSession) {
            GD.PrintErr("GameSession was not deserialized into current instance, this should not happen!");
            GameSession = receivedGameSession; // fallback to the deserialized instance
        }
        
        // // create PlayerController for local player
        for (int i = 0; i < GameSession.Players.Length; i++) {
            Player player = GameSession.Players[i];
            if (!player.Account.Equals(Account.GetCurrent())) continue;
            GD.Print($"{System.Environment.ProcessId}: [{Account.GetCurrent().Id}] Creating player controller for player {player.Account.Id} at {player.Position}.");

            PlayerController controller = new PlayerController(player);
            controller.Name = $"Player{i}Controller";
            player.AddChild(controller);
            PlayerUI.Instance.SetPlayer(player);
            break;
        }

        this.RpcToServer(MethodName.Loaded);
        LoadingScreen.Instance.SetStage(LoadingStage.WAITING_FOR_PLAYERS);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void Loaded() {
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

    public void PublishChange(IIdentifiable changedIdentifiable, Player? viewableBy = null) {
        if (!IsServer) return;

        // TODO: collect all changes in a frame and send them in one RPC call (only really useful once reference tracking is fixed with a different or custom serializer)
        if (viewableBy == null) {
            Rpc(MethodName.TransferIdentifiable, SerializerExtensions.Serialize(changedIdentifiable), changedIdentifiable.GetType().AssemblyQualifiedName);
        } else {
            RpcId(GetPeerIdFor(viewableBy), MethodName.TransferIdentifiable, SerializerExtensions.Serialize(changedIdentifiable), changedIdentifiable.GetType().AssemblyQualifiedName);
        }
    }
    
    public void PublishChange(Node changedNode, Player? viewableBy = null) {
        if (!IsServer) return;

        // TODO: collect all changes in a frame and send them in one RPC call (only really useful once reference tracking is fixed with a different or custom serializer)
        if (viewableBy == null) {
            Rpc(MethodName.TransferNode, SerializerExtensions.Serialize(changedNode), changedNode.GetType().AssemblyQualifiedName);
        } else {
            RpcId(GetPeerIdFor(viewableBy), MethodName.TransferNode, SerializerExtensions.Serialize(changedNode), changedNode.GetType().AssemblyQualifiedName);
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void TransferIdentifiable(byte[] serializedIdentifiable, string typeName) {
        if (IsServer) throw new InvalidOperationException("TransferIdentifiable can only be called on the client.");
        
        Type type = typeName != null ? Type.GetType(typeName) : null;
        if (type == null) {
            GD.PrintErr($"TransferIdentifiable received invalid type name '{typeName}', cannot deserialize identifiable.");
            return;
        }
        
        SerializerExtensions.Deserialize<IIdentifiable>(serializedIdentifiable, type);
        // that already does it, IdentifiableSurrogate takes care of the merging
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void TransferNode(byte[] serializedNode, string typeName) {
        if (IsServer) throw new InvalidOperationException("TransferNode can only be called on the client.");
        
        Type type = typeName != null ? Type.GetType(typeName) : null;
        if (type == null) {
            GD.PrintErr($"TransferNode received invalid type name '{typeName}', cannot deserialize node.");
            return;
        }
        
        SerializerExtensions.Deserialize<Node>(serializedNode, type);
        // that already does it, NodeDataSurrogate takes care of the merging
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