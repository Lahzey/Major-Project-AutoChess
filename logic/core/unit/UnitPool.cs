using System;
using System.Collections.Generic;

namespace MPAutoChess.logic.core.unit;

public class UnitPool {
    
    private static readonly Dictionary<int, UnitPool> pools = new Dictionary<int, UnitPool>();

    private readonly Dictionary<UnitType, int> pool;
    private int totalCount = 0;
    
    public UnitPool(UnitType[] unitTypes, int countPerType) {
        pool = new Dictionary<UnitType, int>(unitTypes.Length);
        foreach (var unitType in unitTypes) {
            pool[unitType] = countPerType;
            totalCount += countPerType;
        }
    }

    public static void Initialize(Season season) {
        pools[1] = new UnitPool(season.Units.CommonUnits, 30);
        pools[2] = new UnitPool(season.Units.UncommonUnits, 24);
        pools[3] = new UnitPool(season.Units.RareUnits, 18);
        pools[4] = new UnitPool(season.Units.EpicUnits, 12);
        pools[5] = new UnitPool(season.Units.LegendaryUnits, 9);
    }

    public static UnitPool For(UnitType unitType) {
        foreach (var pool in pools.Values) {
            if (pool.pool.ContainsKey(unitType)) {
                return pool;
            }
        }

        return null;
    }

    public static UnitPool OfRarity(int rarity) {
        return pools[rarity];
    }


    public Unit? TakeRandomUnit(Random random) {
        int index = random.Next(totalCount);
        int i = -1;
        foreach (UnitType unitType in pool.Keys) {
            i += pool[unitType];
            if (i >= index) return TryTakeUnit(unitType);
        }
        return null;
    }
    
    public Unit? TryTakeUnit(UnitType unitType, bool force = false) {
        if (pool.TryGetValue(unitType, out int count)) {
            if (count <= 0 && !force) return null;
            pool[unitType]--;
            totalCount--;
            return new Unit(unitType, this);
        }
        
        throw new KeyNotFoundException($"Unit type {unitType.Name} not found in pool.");
    }
    
    public void ReturnUnit(UnitType unitType) {
        if (pool.ContainsKey(unitType)) {
            pool[unitType]++;
            totalCount++;
        } else {
            throw new KeyNotFoundException($"Unit type {unitType.Name} not found in pool.");
        }
    }
}