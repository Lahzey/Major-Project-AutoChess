using System;
using System.Collections.Generic;

namespace MPAutoChess.logic.core.unit;

public class UnitPool {
    
    private static readonly Dictionary<int, UnitPool> POOLS = new Dictionary<int, UnitPool>();

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
        POOLS[1] = new UnitPool(season.GetUnits().CommonUnits, 30);
        POOLS[2] = new UnitPool(season.GetUnits().UncommonUnits, 24);
        POOLS[3] = new UnitPool(season.GetUnits().RareUnits, 18);
        POOLS[4] = new UnitPool(season.GetUnits().EpicUnits, 12);
        POOLS[5] = new UnitPool(season.GetUnits().LegendaryUnits, 9);
    }

    public static UnitPool For(UnitType unitType) {
        foreach (var pool in POOLS.Values) {
            if (pool.pool.ContainsKey(unitType)) {
                return pool;
            }
        }

        return null;
    }

    public static UnitPool OfRarity(int rarity) {
        return POOLS[rarity];
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