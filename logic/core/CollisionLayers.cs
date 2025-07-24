using System;

namespace MPAutoChess.logic.core;

[Flags]
public enum CollisionLayers : uint {
    NONE = 0,
    PASSIVE_UNIT_INSTANCE = 1 << 0,
    UNIT_DROP_TARGET = 1 << 1,
    COMBAT_UNIT_INSTANCE = 1 << 2,
    PROJECTILE = 1 << 3,
}