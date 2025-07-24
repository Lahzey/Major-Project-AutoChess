using System;

namespace MPAutoChess.logic.core.environment;

// NEVER CHANGE THE INT VALUES OF THESE ENUMS, they are saved using the int value, not the name or the resulting prefab path
public enum ArenaType : int {
    DEFAULT = 0,
}

public static class ArenaTypeExtensions {
    public static string GetName(this ArenaType arenaType) {
        return arenaType switch {
            ArenaType.DEFAULT => "Default Arena",
            _ => throw new ArgumentOutOfRangeException(nameof(arenaType), arenaType, null)
        };
    }
    
    public static string GetPath(this ArenaType arenaType) {
        return arenaType switch {
            ArenaType.DEFAULT => "res://prefabs/arenas/DefaultArena.tscn",
            _ => throw new ArgumentOutOfRangeException(nameof(arenaType), arenaType, null)
        };
    }
}