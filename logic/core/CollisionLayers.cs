using System;

namespace MPAutoChess.logic.core;

[Flags]
public enum CollisionLayers : uint {
    None = 0,
    PassiveUnitInstance = 1 << 0,
    UnitDropTarget = 1 << 1,
    CombatUnitInstance = 1 << 2,
    Projectile = 1 << 3,
}