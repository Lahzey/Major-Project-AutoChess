using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public partial class Combat : Node2D {
    public const float NAVIGATION_GRID_SCALE = 0.25f;
    private const double TIME_UNTIL_START = 3.0; // seconds until combat starts after preparation

    [ProtoMember(1)] public Rect2 CombatArea { get; private set; }
    [ProtoMember(2)] public Player PlayerA { get; private set; }
    [ProtoMember(3)] public Player PlayerB { get; private set; }
    [ProtoMember(4)] public bool IsCloneFight { get; private set; }
    [ProtoMember(5)] public List<UnitInstance> TeamA { get; private set; } = new List<UnitInstance>();
    [ProtoMember(6)] public List<UnitInstance> TeamB { get; private set; } = new List<UnitInstance>();
    
    // Units without a valid target always look for the shortest path to an enemy.
    // Each process, one unit per team which is still walking its path will recalculate its path.
    // These indices are so that within enough process cycles, each unit will get a chance to recalculate its path.
    private int teamAUnitProcessingIndex = 0;
    private int teamBUnitProcessingIndex = 0;

    // these properties will only matter when reconnecting during a combat
    [ProtoMember(7)] public double CombatTime { get; private set; } = -TIME_UNTIL_START;
    [ProtoMember(8)] public bool Running { get; private set; } = false;

    public Rect2 GlobalBounds => new Rect2(GlobalPosition + CombatArea.Position, CombatArea.Size);

    public void Prepare(Player playerA, Player playerB, bool isCloneFight) {
        PlayerA = playerA;
        PlayerB = playerB;
        IsCloneFight = isCloneFight;
        
        foreach (Unit unit in playerA.Board.GetUnits()) {
            UnitInstance unitInstance = CreateUnitInstance(unit, playerA.Board.GetPlacement(unit), true);
            TeamA.Add(unitInstance);
        }

        foreach (Unit unit in playerB.Board.GetUnits()) {
            UnitInstance unitInstance = CreateUnitInstance(unit, playerB.Board.GetPlacement(unit), false);
            TeamB.Add(unitInstance);
        }

        Vector2 combatSize = new Vector2(playerA.Board.Columns, playerA.Board.Rows + playerB.Board.Rows);
        CombatArea = new Rect2(new Vector2(0f, combatSize.Y * -0.5f), combatSize);
        if (!PathFinder.IsValidBounds(CombatArea, NAVIGATION_GRID_SCALE)) {
            throw new ArgumentException($"CombatArea {CombatArea} is not valid for pathfinding with grid scale {NAVIGATION_GRID_SCALE}. Decrease size or increase grid scale.");
        }
    }

    private UnitInstance CreateUnitInstance(Unit unit, Vector2 placement, bool teamA) {
        UnitInstance unitInstance = unit.CreateInstance(true);
        AddChild(unitInstance);
        
        Vector2 position = placement + (unit.GetSize() * 0.5f);
        if (teamA) position.Y *= -1f;
        unitInstance.Position = position;
        unitInstance.CurrentCombat = this;
        unitInstance.IsInTeamA = teamA;
        
        return unitInstance;
    }

    public void Start() {
        Running = true;
    }

    public override void _PhysicsProcess(double delta) {
        if (!Running) return;
        
        CombatTime += delta;
        if (CombatTime < 0) return;
        
        ProcessUnits(delta, true);
        ProcessUnits(delta, false);
    }


    public void ProcessUnits(double delta, bool teamA) {
        ref int processingIndex = ref (teamA ? ref teamAUnitProcessingIndex : ref teamBUnitProcessingIndex);
        List<UnitInstance> units = teamA ? TeamA : TeamB;
        bool hasProcessed = false;
        for (int i = 0; i < units.Count; i++) {
            UnitInstance unitInstance = units[i];
            if (!unitInstance.IsAlive()) continue;
            
            bool shouldProcess = ServerController.Instance.IsServer && processingIndex == i && !hasProcessed;
            if (shouldProcess) processingIndex++;
            if (processingIndex >= units.Count) processingIndex = 0; // reset index for next process cycle

            if (!unitInstance.HasTarget()) {
                if (ServerController.Instance.IsServer) SelectNewTarget(unitInstance, teamA);
            } else if (!unitInstance.CanReachTarget()) {
                if (shouldProcess) {
                    SelectNewTarget(unitInstance, teamA);
                    hasProcessed = true;
                }
            }

            unitInstance.ProcessCombat(delta);
        }
    }

    private void SelectNewTarget(UnitInstance unitInstance, bool teamA) {
        List<UnitInstance> enemies = teamA ? TeamB : TeamA;
        UnitInstance closestEnemy = null;
        float closestSquaredDistance = float.MaxValue;
        float range = unitInstance.Stats.GetValue(StatType.RANGE);

        // try finding a target within range
        foreach (UnitInstance enemy in enemies) {
            if (enemy == null || !IsInstanceValid(enemy) || !enemy.IsAlive()) continue;

            float squaredDistance = unitInstance.Position.DistanceSquaredTo(enemy.Position);
            if (squaredDistance < closestSquaredDistance) {
                closestSquaredDistance = squaredDistance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null) {// no enemies left
            if (unitInstance.CurrentTarget != null) unitInstance.SetTarget(null);
            return; // TODO: finish combat
        }

        if (closestSquaredDistance <= range * range) {
            unitInstance.SetTarget(closestEnemy);
            return;
        }

        // if no target was found within range, try finding the closest enemy based on pathing distance
        PathFinder.Path closestPath = null;
        Vector2 sizeVec = unitInstance.GetSize();
        float radius = Mathf.Max(sizeVec.X, sizeVec.Y) * 0.5f;
        IsWalkableCache isWalkableCache = new IsWalkableCache(TeamA, TeamB, NAVIGATION_GRID_SCALE, unitInstance);
        foreach (UnitInstance enemy in enemies) {
            if (enemy == null || !IsInstanceValid(enemy) || !enemy.IsAlive()) continue;
            sizeVec = enemy.GetSize();
            float enemyRadius = Mathf.Max(sizeVec.X, sizeVec.Y) * 0.5f;
            PathFinder.Path path = PathFinder.FindPath(unitInstance.Position, enemy.Position, radius + enemyRadius + range, CombatArea, isWalkableCache.IsWalkable, NAVIGATION_GRID_SCALE);
            if (path == null) continue; // unreachable enemy
            if (closestPath == null || path.TotalLength < closestPath.TotalLength) {
                closestPath = path;
                closestEnemy = enemy;
            }
        }

        if (closestPath != null) {
            unitInstance.SetTarget(closestEnemy, closestPath);
        } else {
            unitInstance.SetTarget(null);
        }
    }
}

public class IsWalkableCache {
    private List<Vector2I> gridPositions = new List<Vector2I>();
    private List<int> gridRadii = new List<int>();
    private Dictionary<int, bool> walkableCache = new Dictionary<int, bool>();
    private float activeGridRadius;

    public IsWalkableCache(List<UnitInstance> teamA, List<UnitInstance> teamB, float gridScale, UnitInstance activeUnit) {
        float oneOverGridScale = 1f / gridScale;
        AddUnits(teamA, oneOverGridScale, activeUnit);
        AddUnits(teamB, oneOverGridScale, activeUnit);
        activeGridRadius = Mathf.RoundToInt(Mathf.Max(activeUnit.GetSize().X, activeUnit.GetSize().Y) * 0.5f * oneOverGridScale);
    }

    private void AddUnits(List<UnitInstance> units, float oneOverGridScale, UnitInstance activeUnit) {
        foreach (UnitInstance unit in units) {
            if (unit == activeUnit) continue;
            Vector2I gridPosition = PathFinder.ToGridCoord(unit.Position, oneOverGridScale);
            gridPositions.Add(gridPosition);
            Vector2 size = unit.GetSize();
            gridRadii.Add(Mathf.RoundToInt(Mathf.Max(size.X, size.Y) * 0.5f * oneOverGridScale));
        }
    }

    public bool IsWalkable(Vector2I position) {
        if (walkableCache.TryGetValue(PathFinder.Hash(position), out bool isWalkableCached)) {
            return isWalkableCached;
        }

        for (int i = 0; i < gridPositions.Count; i++) {
            Vector2I nodePos = gridPositions[i];
            int nodeRadius = gridRadii[i];
            float minDistanceSquared = (activeGridRadius + nodeRadius) * (activeGridRadius + nodeRadius);
            if (position.DistanceSquaredTo(nodePos) < minDistanceSquared) {
                walkableCache[PathFinder.Hash(position)] = false;
                return false;
            }
        }

        walkableCache[PathFinder.Hash(position)] = true;
        return true;
    }
}