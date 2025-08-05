using System.Collections.Generic;
using Godot;

public partial class SceneSafeMpSpawner : MultiplayerSpawner {
    
    public delegate void PeerReadyEventHandler(int peer);
    private int peerReadyCount = 0;
    private PeerReadyEventHandler _peerReady;
    public event PeerReadyEventHandler PeerReady {
        add {
            _peerReady += value;
            peerReadyCount++;
        }
        remove {
            _peerReady -= value;
            peerReadyCount--;
        }
    }

    public delegate void PeerRemovedEventHandler(int peer);
    private int peerRemovedCount = 0;
    private PeerRemovedEventHandler _peerRemoved;
    public event PeerRemovedEventHandler PeerRemoved {
        add {
            _peerRemoved += value;
            peerRemovedCount++;
        }
        remove {
            _peerRemoved -= value;
            peerRemovedCount--;
        }
    }

    private List<int> _missedReadySignals = new();
    private List<int> _missedRemovedSignals = new();

    public override void _Ready() {
        SceneSafeMpManager.Instance.RegisterSpawner(
            GetPath(),
            Multiplayer.GetUniqueId(),
            GetMultiplayerAuthority()
        );
    }

    public override void _ExitTree() {
        SceneSafeMpManager.Instance.UnregisterSpawner(
            GetPath(),
            Multiplayer.GetUniqueId(),
            GetMultiplayerAuthority()
        );
    }

    public void ActivateReadySignal(int peer) {
        if (peerReadyCount == 0) {
            _missedReadySignals.Add(peer);

            if (_missedRemovedSignals.Contains(peer)) {
                _missedReadySignals.Remove(peer);
                _missedRemovedSignals.Remove(peer);
            }
        } else {
            _peerReady?.Invoke(peer);
        }
    }

    public void ActivateRemovedSignal(int peer) {
        if (peerRemovedCount == 0) {
            _missedRemovedSignals.Add(peer);

            if (_missedReadySignals.Contains(peer)) {
                _missedReadySignals.Remove(peer);
                _missedRemovedSignals.Remove(peer);
            }
        } else {
            _peerRemoved?.Invoke(peer);
        }
    }

    public void FlushMissedSignals() {
        foreach (var peer in _missedReadySignals) {
            EmitSignal(nameof(PeerReady), peer);
        }

        foreach (var peer in _missedRemovedSignals) {
            EmitSignal(nameof(PeerRemoved), peer);
        }

        _missedReadySignals.Clear();
        _missedRemovedSignals.Clear();
    }
}