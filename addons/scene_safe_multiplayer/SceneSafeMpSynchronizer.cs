using Godot;
using System;
using System.Collections.Generic;

public partial class SceneSafeMpSynchronizer : MultiplayerSynchronizer {
    [Export] public bool IsSpawnerVisibilityController { get; set; } = false;

    private List<int> _internalHandshakeVisibility = new();
    private Dictionary<string, bool> _internalVisibilityMap = new();
    private bool _internalPublicVisibility;
    private SceneSafeMpSpawner _parentSpawner;

    public override void _Ready() {
        _internalPublicVisibility = PublicVisibility;
        PublicVisibility = false;

        SceneSafeMpManager.Instance.RegisterSynchronizer(
            GetPath(),
            Multiplayer.GetUniqueId(),
            GetMultiplayerAuthority()
        );

        if (IsSpawnerVisibilityController)
            _parentSpawner = GetParentSpawner(GetParent(), new List<Node>());
    }

    public override void _ExitTree() {
        if (IsSpawnerVisibilityController && _parentSpawner != null)
            SceneSafeMpManager.Instance.UnlinkVisibilitySyncFromSpawner(_parentSpawner.GetPath(), this);

        SceneSafeMpManager.Instance.UnregisterSynchronizer(
            GetPath(),
            Multiplayer.GetUniqueId(),
            GetMultiplayerAuthority()
        );
    }

    public void SetPublicVisibility(bool visible) {
        _internalPublicVisibility = visible;
        UpdateUnderlyingVisibilityForAll();
    }

    public void EnableDataFlowFor(IEnumerable<int> peers) {
        foreach (var peer in peers) {
            if (_internalHandshakeVisibility.Contains(peer))
                continue;

            _internalHandshakeVisibility.Add(peer);
            UpdateUnderlyingVisibilityFor(peer);
        }
    }

    public void DisableDataFlowFor(int peer) {
        if (_internalHandshakeVisibility.Contains(peer))
            _internalHandshakeVisibility.Remove(peer);

        UpdateUnderlyingVisibilityFor(peer);
    }
    
    public new void SetVisibilityFor(int peer, bool visible) {
        string key = peer.ToString();
        if (!visible && _internalVisibilityMap.ContainsKey(key)) {
            _internalVisibilityMap.Remove(key);
        } else if (visible && !_internalVisibilityMap.ContainsKey(key)) {
            _internalVisibilityMap[key] = true;
        }

        UpdateUnderlyingVisibilityFor(peer);
    }

    private SceneSafeMpSpawner GetParentSpawner(Node node, List<Node> checkedParents) {
        foreach (var child in node.GetChildren()) {
            if (child is SceneSafeMpSpawner spawner) {
                if (checkedParents.Contains(spawner.GetNode(spawner.SpawnPath)) &&
                    spawner.GetMultiplayerAuthority() == GetMultiplayerAuthority()) {
                    SceneSafeMpManager.Instance.LinkVisibilitySyncToSpawner(spawner.GetPath(), this);
                    return spawner;
                }
            }
        }

        var parent = node.GetParent();
        if (parent != null) {
            checkedParents.Add(node);
            return GetParentSpawner(parent, checkedParents);
        }

        return null;
    }

    private void UpdateUnderlyingVisibilityFor(int peer) {
        bool normalVisibility = _internalPublicVisibility;
        string key = peer.ToString();

        if (_internalVisibilityMap.TryGetValue(key, out var overrideVisibility))
            normalVisibility = overrideVisibility;

        bool finalVisibility = _internalHandshakeVisibility.Contains(peer) && normalVisibility;
        base.SetVisibilityFor(peer, finalVisibility);
    }

    private void UpdateUnderlyingVisibilityForAll() {
        var peers = new HashSet<int>(_internalHandshakeVisibility);

        foreach (var key in _internalVisibilityMap.Keys) {
            if (int.TryParse(key, out int parsedPeer))
                peers.Add(parsedPeer);
        }

        foreach (var peer in peers) {
            UpdateUnderlyingVisibilityFor(peer);
        }
    }
}