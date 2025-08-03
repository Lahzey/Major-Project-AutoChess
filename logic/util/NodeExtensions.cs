using System;
using Godot;
using MPAutoChess.logic.core.networking;

namespace MPAutoChess.logic.util;

public static class NodeExtensions {
    
    public static long ServerPeerId { get; set; }
    
    public static Error RpcToServer(this Node node, string methodName, params Variant[] args) {
        if (ServerController.Instance.IsServer) throw new InvalidOperationException("Cannot call RpcToServer on the server.");
        return node.RpcId(ServerPeerId, methodName, args);
    }

    public static Error Respond(this Node node, string methodName, params Variant[] args) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Cannot call Respond on the client, use RpcToServer to talk to server.");
        long peerId = node.Multiplayer.GetRemoteSenderId();
        if (peerId == 0) throw new InvalidOperationException("Cannot call Respond outside of an RPC call.");
        return node.RpcId(peerId, methodName, args);
    }

}