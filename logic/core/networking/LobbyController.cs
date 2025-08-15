using System;
using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.core.networking;

public partial class LobbyController : Node {

    public const string LOBBY_IP = "mp.pixelframemarketing.ch";
    public const int LOBBY_PORT = 80;

    private WebSocketPeer ws;

    public long AccountId { get; private set; }
    public string AccountName { get; private set; }
    public string Secret { get; private set; }

    public Action OnConnected;
    public Action OnQueueEntered;
    public Action OnQueueLeft;
    public Action<int> OnGameStart;

    public override void _EnterTree() {
        ws = new WebSocketPeer();
        Error err = ws.ConnectToUrl("ws://" + LOBBY_IP + ":" + LOBBY_PORT);
        if (err != Error.Ok)
            GD.PrintErr("Failed to connect");
    }

    public override void _ExitTree() {
        ws.Dispose();
        ws = null;
    }

    public override void _Process(double delta) {
        if (ws == null) return;
        ws.Poll();

        while (ws != null && ws.GetAvailablePacketCount() > 0) { // start game is executed in this loop, which means exit tree may also effectively run in this loop
            string packet = ws.GetPacket().GetStringFromUtf8();
            GD.Print("Received: ", packet);

            Dictionary json = Json.ParseString(packet).As<Dictionary>();

            if (!json.ContainsKey("type")) continue;
            string type = json["type"].ToString();

            switch (type) {
                case "connected":
                    Secret = json["secret"].ToString();
                    AccountId = (long) json["accountId"];
                    GD.Print($"Auth complete. ID: {AccountId}, Secret: {Secret}");
                    OnConnected?.Invoke();
                    break;

                case "game_started":
                    if (json.ContainsKey("port")) {
                        int port = (int) json["port"];
                        GD.Print($"Game started on port {port}");
                        OnGameStart?.Invoke(port);
                    }

                    break;
                case "queued":
                    OnQueueEntered?.Invoke();
                    break;
                case "queue_left":
                    OnQueueLeft?.Invoke();
                    break;

                default:
                    GD.Print($"Unhandled message type: {type}");
                    break;
            }
        }
    }

    public void Connect(string userName) {
        if (!string.IsNullOrEmpty(Secret)) {
            GD.PrintErr("Already connected");
            return;
        }
        
        AccountName = userName;
        Dictionary connectMsg = new Godot.Collections.Dictionary {
            { "type", "connect" },
            { "userName", userName }
        };
        ws.SendText(Json.Stringify(connectMsg));
    }


    public void EnterQueue() {
        if (string.IsNullOrEmpty(Secret)) {
            GD.PrintErr("Cannot enter queue: not connected yet");
            return;
        }

        Dictionary msg = new Dictionary {
            { "type", "enter_game_queue" },
            { "secret", Secret }
        };
        ws.SendText(Json.Stringify(msg));
        GD.Print("Entered game queue");
    }

    public void LeaveQueue() {
        if (string.IsNullOrEmpty(Secret)) {
            GD.PrintErr("Cannot leave queue: not connected yet");
            return;
        }

        Dictionary msg = new Dictionary {
            { "type", "leave_game_queue" },
            { "secret", Secret }
        };
        ws.SendText(Json.Stringify(msg));
        GD.Print("Left game queue");
    }
}