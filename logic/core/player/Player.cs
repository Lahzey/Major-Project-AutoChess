using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.player;

[ProtoContract]
public partial class Player : Node2D {

    [ProtoMember(1)] public Account Account { get; private set; }

    [Export] [ProtoMember(2)] public int Health { get; set; } = 1000;

    [Export] [ProtoMember(3)] public int Experience { get; set; } = 0;

    [Export] [ProtoMember(4)] public int Gold { get; set; } = 100;
    
    [ProtoMember(5)] public Shop Shop { get; private set; }
    
    [ProtoMember(6)] public Inventory Inventory { get; private set; } = new Inventory(10);

    private Arena _arena;
    [ProtoMember(8)]
    public Arena Arena {
        get => _arena;
        private set {
            _arena = value;
            _arena.Board.Player = this;
            _arena.Bench.Player = this;
        }
    }

    public Board Board => Arena.Board;
    public Bench Bench => Arena.Bench;
    
    public Calculation BoardSize { get; private set; } = new Calculation(5);

    public Player() {
        Shop = new Shop(this);
    }

    public void SetAccount(Account account) {
        Account = account;
        Arena = ResourceLoader.Load<PackedScene>(Account.GetSelectedArenaType().GetPath()).Instantiate<Arena>();
        AddChild(Arena);
    }

    public override void _Ready() {
        EventManager.INSTANCE.AddAfterListener((UnitContainerUpdateEvent e) => CheckForUnitLevelUp());
        EventManager.INSTANCE.AddAfterListener((RoundStartEvent e) => CheckForUnitLevelUp());
        EventManager.INSTANCE.AddAfterListener((UnitLevelUpEvent e) => CheckForUnitLevelUp());
    }

    public int GetLevel() {
        switch (Experience) {
            case < 2: return 1;
            case < 6: return 2;
            case < 10: return 3;
            case < 20: return 4;
            case < 50: return 5;
            case < 80: return 6;
            case < 120: return 7;
            case < 190: return 8;
            case < 250: return 9;
            default: return 10; // Level 10 is the maximum level
        }
    }

    private void CheckForUnitLevelUp() {
        Dictionary<Tuple<UnitType, uint>, List<Unit>> units = new Dictionary<Tuple<UnitType, uint>, List<Unit>>();

        if (!GameSession.Instance.IsInCombat(this)) {
            foreach (Unit unit in Board.GetUnits()) {
                Tuple<UnitType, uint> key = new Tuple<UnitType, uint>(unit.Type, unit.Level);
                if (!units.ContainsKey(key)) units.Add(key, new List<Unit>());
                units[key].Add(unit);
            }
        }
        
        foreach (SingleUnitSlot benchSlot in Bench.GetSlots()) {
            if (benchSlot.Unit != null) {
                Tuple<UnitType, uint> key = new Tuple<UnitType, uint>(benchSlot.Unit.Type, benchSlot.Unit.Level);
                if (!units.ContainsKey(key)) units.Add(key, new List<Unit>());
                units[key].Add(benchSlot.Unit);
            }
        }
        
        foreach (List<Unit> unitList in units.Values) {
            Unit firstUnit = unitList[0];
            if (unitList.Count >= firstUnit.GetCopyCountForLevelUp()) {
                Unit[] copies = unitList.GetRange(1, unitList.Count - 1).ToArray();
                firstUnit.LevelUp(copies);
            }
        }
        
        
    }

    public bool TryPurchase(int cost, Action purchaseAction) {
        if (Gold >= cost) {
            Gold -= cost;
            purchaseAction();
            return true;
        } else {
            return false;
        }
    }
    
    public void MoveToTemporaryBench(Unit unit) {
        // TODO: Implement logic to move the unit to a temporary bench
    }

}