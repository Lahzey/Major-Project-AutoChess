using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class AbsorptionGuard : ItemEffect {

    private const string STATS_PREFIX = "Absorption Guard ";
    private const float RANGE_SQUARED = 3f * 3f;
    private const float TENACITY_VALUE = 0.15f;
    private const float ARMOR_VALUE = 20f;
    
    private const double RECALCULATE_INTERVAL = 0.25;
    
    private int instanceCounter = 0; // used for a more readable id in the stats
    [ProtoMember(1)] private int instanceId;
    
    private string StatsId => STATS_PREFIX + instanceId;
    
    private double accumulatedDelta = 0.0;
    private List<Unit> appliedTo = new List<Unit>();

    public AbsorptionGuard() {
        if (ServerController.Instance.IsServer) {
            instanceId = instanceCounter++;
        }
    }

    public override void Apply(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        Vector2 position = unit.Position;
        foreach (UnitInstance ally in unit.Allies) {
            if (position.DistanceSquaredTo(ally.Position) > RANGE_SQUARED) continue;
            ally.Stats.GetCalculation(StatType.TENACITY).AddFlat(item.ScaleValue(TENACITY_VALUE), StatsId);
            ally.Stats.GetCalculation(StatType.ARMOR).AddFlat(item.ScaleValue(ARMOR_VALUE), StatsId);
        }
    }

	public override void Process(Item item, UnitInstance unit, double delta) {
        if (!ServerController.Instance.IsServer) return;
        if (unit.IsCombatInstance) return;

        // currently not processing for non combat units
        
        
        // accumulatedDelta += delta;
        // if (accumulatedDelta < RECALCULATE_INTERVAL) return;
        // accumulatedDelta = 0.0;
        //
        // GD.Print("AbsorptionGuard processing for unit: " + unit.Unit.Id);
        //
        // if (unit.Unit.Container is not Board board) {
        //     Remove(item, unit);
        //     return;
        // }
        //
        // Vector2 position = board.GetPlacement(unit.Unit) + unit.Unit.GetSize() * 0.5f;
        // List<Unit> unitsToRemove = appliedTo.ToList();
        // foreach (Unit boardUnit in board.GetUnits()) {
        //     Vector2 boardUnitPosition = board.GetPlacement(boardUnit) + boardUnit.GetSize() * 0.5f;
        //     if (position.DistanceSquaredTo(boardUnitPosition) > RANGE_SQUARED) continue;
        //     unitsToRemove.Remove(boardUnit);
        //     UnitInstance boardUnitInstance = boardUnit.GetOrCreatePassiveInstance();
        //     
        //     float tenacityValue = item.ScaleValue(TENACITY_VALUE);
        //     Value existingTenacityValue = boardUnitInstance.Stats.GetCalculation(StatType.TENACITY).GetFlat(StatsId);
        //     if (existingTenacityValue == null || existingTenacityValue.Get() != tenacityValue)
        //         boardUnitInstance.Stats.GetCalculation(StatType.TENACITY).AddFlat(tenacityValue, StatsId);
        //     
        //     float armorValue = item.ScaleValue(ARMOR_VALUE);
        //     Value existingArmorValue = boardUnitInstance.Stats.GetCalculation(StatType.ARMOR).GetFlat(StatsId);
        //     if (existingArmorValue == null || existingArmorValue.Get() != armorValue)
        //         boardUnitInstance.Stats.GetCalculation(StatType.ARMOR).AddFlat(armorValue, StatsId);
        //     
        //     appliedTo.Add(boardUnit);
        // }
        //
        // foreach (Unit toRemove in unitsToRemove) {
        //     UnitInstance unitInstance = toRemove.GetOrCreatePassiveInstance();
        //     unitInstance.Stats.GetCalculation(StatType.TENACITY).RemoveFlat(StatsId);
        //     unitInstance.Stats.GetCalculation(StatType.ARMOR).RemoveFlat(StatsId);
        //     appliedTo.Remove(toRemove);
        // }
    }

    public override void Remove(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (unit.IsCombatInstance) return;
        
        foreach (Unit appliedToUnit in appliedTo) {
            UnitInstance unitInstance = appliedToUnit.GetOrCreatePassiveInstance();
            unitInstance.Stats.GetCalculation(StatType.TENACITY).RemoveFlat(StatsId);
            unitInstance.Stats.GetCalculation(StatType.ARMOR).RemoveFlat(StatsId);
        }
        appliedTo.Clear();
    }
}