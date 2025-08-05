using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SceneSafeMpManager : Node {
    private class SpawnerData {
        public int Authority;
        public List<int> ConfirmedPeers = new();
        public List<SceneSafeMpSynchronizer> LinkedSynchronizers = new();
    }

    private class SynchronizerData {
        public int Authority;
        public List<int> ConfirmedPeers = new();
    }

    private Dictionary<string, SpawnerData> _spawnerMap = new();
    private Dictionary<string, SynchronizerData> _synchronizerMap = new();

    public static SceneSafeMpManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _ExitTree() {
        if (Instance == this) Instance = null;
    }

    public override void _Ready() {
        Multiplayer.PeerDisconnected += CleanupPeerData;
        Multiplayer.ServerDisconnected += CleanupAllData;
    }

    public void RegisterSpawner(string nodeName, int id, int authorityId) {
        if (!_spawnerMap.ContainsKey(nodeName)) {
            _spawnerMap[nodeName] = new SpawnerData {
                Authority = authorityId
            };
        }

        var entry = _spawnerMap[nodeName];
        entry.ConfirmedPeers.Add(id);

        if (Multiplayer.GetUniqueId() == authorityId) {
            var node = GetTree().CurrentScene?.GetNodeOrNull<Node>(nodeName);
            if (node != null) {
                if (id == authorityId && entry.ConfirmedPeers.Count > 1) {
                    foreach (var peer in entry.ConfirmedPeers)
                        (node as SceneSafeMpSpawner)?.ActivateReadySignal(peer);
                } else {
                    (node as SceneSafeMpSpawner)?.ActivateReadySignal(id);
                }
            }

            entry.LinkedSynchronizers = entry.LinkedSynchronizers
                .Where(GodotObject.IsInstanceValid)
                .ToList();

            foreach (var sync in entry.LinkedSynchronizers) {
                if (GodotObject.IsInstanceValid(sync))
                    sync.EnableDataFlowFor(new[] { id });
            }
        } else {
            RpcId(authorityId, nameof(PeerConfirmedSpawner), nodeName, id, authorityId);
        }
    }

    public void UnregisterSpawner(string nodeName, int id, int authorityId) {
        if (!_spawnerMap.TryGetValue(nodeName, out var entry) || !entry.ConfirmedPeers.Contains(id))
            return;

        entry.ConfirmedPeers.Remove(id);

        if (entry.ConfirmedPeers.Count == 0)
            _spawnerMap.Remove(nodeName);

        if (authorityId != Multiplayer.GetUniqueId()) {
            RpcId(authorityId, nameof(PeerUnregisteredSpawner), nodeName, id, authorityId);
        } else if (GetTree().CurrentScene?.HasNode(nodeName) == true) {
            (GetTree().CurrentScene.GetNode<Node>(nodeName) as SceneSafeMpSpawner)?.ActivateRemovedSignal(id);
        }
    }

    public void RegisterSynchronizer(string nodeName, int id, int authorityId) {
        if (!_synchronizerMap.ContainsKey(nodeName)) {
            _synchronizerMap[nodeName] = new SynchronizerData {
                Authority = authorityId
            };
        }

        var entry = _synchronizerMap[nodeName];
        entry.ConfirmedPeers.Add(id);

        if (Multiplayer.GetUniqueId() == authorityId && entry.ConfirmedPeers.Contains(authorityId)) {
            (GetTree().CurrentScene?.GetNode<Node>(nodeName) as SceneSafeMpSynchronizer)?.EnableDataFlowFor(entry.ConfirmedPeers);
        } else if (Multiplayer.GetUniqueId() != authorityId) {
            RpcId(authorityId, nameof(PeerConfirmedSynchronizer), nodeName, id, authorityId);
        }
    }

    public void UnregisterSynchronizer(string nodeName, int id, int authorityId) {
        if (!_synchronizerMap.TryGetValue(nodeName, out var entry))
            return;

        entry.ConfirmedPeers.Remove(id);

        if (entry.ConfirmedPeers.Count == 0)
            _synchronizerMap.Remove(nodeName);

        if (Multiplayer.GetUniqueId() == authorityId && GetTree().CurrentScene?.HasNode(nodeName) == true) {
            (GetTree().CurrentScene.GetNode<Node>(nodeName) as SceneSafeMpSynchronizer)?.DisableDataFlowFor(id);
        } else if (Multiplayer.GetUniqueId() != authorityId) {
            RpcId(authorityId, nameof(PeerUnregisteredSynchronizer), nodeName, id, authorityId);
        }
    }

    public void LinkVisibilitySyncToSpawner(string spawnerPath, SceneSafeMpSynchronizer synchronizer) {
        if (_spawnerMap.TryGetValue(spawnerPath, out var entry)) {
            entry.LinkedSynchronizers.Add(synchronizer);
            synchronizer.EnableDataFlowFor(entry.ConfirmedPeers);
        }
    }

    public void UnlinkVisibilitySyncFromSpawner(string spawnerPath, SceneSafeMpSynchronizer synchronizer) {
        if (_spawnerMap.TryGetValue(spawnerPath, out var entry)) {
            entry.LinkedSynchronizers.Remove(synchronizer);
        }
    }

    private void CleanupPeerData(long peer) {
        foreach (var key in _spawnerMap.Keys.ToList()) {
            var entry = _spawnerMap[key];
            entry.ConfirmedPeers.Remove((int)peer);
            entry.LinkedSynchronizers = entry.LinkedSynchronizers.Where(GodotObject.IsInstanceValid).ToList();
            if (entry.ConfirmedPeers.Count == 0)
                _spawnerMap.Remove(key);
        }

        foreach (var key in _synchronizerMap.Keys.ToList()) {
            var entry = _synchronizerMap[key];
            entry.ConfirmedPeers.Remove((int)peer);
            if (entry.ConfirmedPeers.Count == 0)
                _synchronizerMap.Remove(key);
        }
    }

    private void CleanupAllData() {
        _spawnerMap.Clear();
        _synchronizerMap.Clear();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PeerConfirmedSpawner(string nodeName, int id, int authorityId) {
        RegisterSpawner(nodeName, id, authorityId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PeerUnregisteredSpawner(string nodeName, int id, int authorityId) {
        UnregisterSpawner(nodeName, id, authorityId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PeerConfirmedSynchronizer(string nodeName, int id, int authorityId) {
        RegisterSynchronizer(nodeName, id, authorityId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PeerUnregisteredSynchronizer(string nodeName, int id, int authorityId) {
        UnregisterSynchronizer(nodeName, id, authorityId);
    }
}