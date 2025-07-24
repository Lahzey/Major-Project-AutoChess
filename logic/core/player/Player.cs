using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.player;

[ProtoContract]
public partial class Player : Node {

    [ProtoMember(1)] public Account Account { get; set; }

    [Export] [ProtoMember(2)] public int Health { get; private set; } = 1000;

    [Export] [ProtoMember(3)] public int Experience { get; set; } = 0;

    [Export] [ProtoMember(4)] public int Gold { get; set; } = 100;
    
    [ProtoMember(5)] public Shop Shop { get; private set; } = new Shop();

    [ProtoMember(6)] public ArenaType ArenaType { get; private set; }
    
    [ProtoMember(7)] public Arena Arena { get; private set; }
    
    [Export] public PlayerUI UI { get; private set; }
    
    public Board Board { get; private set; }
    public Bench Bench { get; private set; }
    
    public Calculation BoardSize { get; private set; } = new Calculation(5);

    public Player() { }

    public void Initialize(Account account) {
        Account = account;
        ArenaType = Account.GetSelectedArenaType();
        Arena = ResourceLoader.Load<PackedScene>(ArenaType.GetPath()).Instantiate<Arena>();
        AddChild(Arena);
        
        Board = Arena.Board;
        Bench = Arena.Bench;
        Board.Player = this;
        Bench.Player = this;
    }

    public override void _Ready() {
        EventManager.INSTANCE.AddAfterListener((UnitContainerUpdateEvent e) => CheckForUnitLevelUp());
        EventManager.INSTANCE.AddAfterListener((RoundStartEvent e) => CheckForUnitLevelUp());
        EventManager.INSTANCE.AddAfterListener((UnitLevelUpEvent e) => CheckForUnitLevelUp());
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