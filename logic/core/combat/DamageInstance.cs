using System;
using Godot;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public class DamageInstance {
    
    [ProtoMember(1)] public UnitInstance Source { get; set; }
    [ProtoMember(2)] public UnitInstance Target { get; set; }
    [ProtoMember(1)] public Medium DamageMedium { get; set; }
    
    [ProtoMember(10)] public float Amount { get; set; }
    [ProtoMember(11)] public DamageType Type { get; set; }
    [ProtoMember(12)] public int CritLevel { get; set; }
    [ProtoMember(13)] public float CritModifier { get; set; }
    [ProtoMember(14)] public bool CritEnabled { get; set; }
    [ProtoMember(15)] public float Effectiveness { get; set; } = 1f;
    
    public bool IsCrit => CritEnabled && CritLevel > 0;

    [ProtoMember(20)] public float Armor { get; set; } = -1f;
    [ProtoMember(21)] public float Aegis { get; set; } = -1f;

    [ProtoMember(30)] public float PreMitigationAmount { get; set; } = -1f;
    [ProtoMember(31)] public float FinalAmount { get; set; } = -1f;

    public DamageInstance() { } // for ProtoBuf deserialization


    public DamageInstance(UnitInstance source, UnitInstance target, Medium damageMedium, float amount, DamageType type, int critLevel, float critModifier, float effectiveness = 1f) {
        Source = source;
        Target = target;
        DamageMedium = damageMedium;
        Amount = amount;
        Type = type;
        CritLevel = critLevel;
        CritModifier = critModifier;
        CritEnabled = type == DamageType.PHYSICAL; // physical damage can crit by default, other types need some effect to override CritEnabled
        Effectiveness = effectiveness;
    }
    
    public void SetResistances(float armor, float aegis) {
        Armor = armor;
        Aegis = aegis;
    }
    
    public void CalculatePreMitigation() {
        PreMitigationAmount = Amount;
        if (IsCrit) {
            PreMitigationAmount *= (1 + CritModifier * CritLevel);
        }
        PreMitigationAmount = Mathf.Max(Amount, 0f); // ensure damage is not negative
    }

    public void CalculateFinalAmount() {
        if (PreMitigationAmount < 0) {
            throw new InvalidOperationException("PreMitigationAmount must be calculated before FinalAmount.");
        }
        
        FinalAmount = PreMitigationAmount;

        switch (Type) {
            case DamageType.PHYSICAL:
                FinalAmount = ApplyResistance(FinalAmount, Armor);
                break;
            case DamageType.MAGICAL:
                FinalAmount = ApplyResistance(FinalAmount, Aegis);
                break;
        }
        
        FinalAmount = Mathf.Max(FinalAmount, 0f); // ensure damage is not negative
    }

    public static float ApplyResistance(float damage, float resistance) {
        return damage * Mathf.Pow(2f, -resistance * 0.01f); // halves damage for every 100 resistance (and doubles for every -100 resistance)
    }
    
    
    public enum Medium {
        ATTACK,
        SPELL,
        ITEM,
    }
    
}