using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorStats {
    [SerializeField]
    public List<Stat> stats;

    public delegate void CharacterStatsChanged();
    public CharacterStatsChanged OnStatsChanged;

    public ActorStats(List<Stat> statList = null) {
        stats = statList;
    }

    public Stat GetStatByName(string name) {
        return stats.Find(s => s.Name == name);
    }

    public Resource GetResourceByName(string name) {
        return (Resource)GetStatByName(name);
    }

    public bool SetStatBaseValue(string name, int value) {
        Stat stat = GetStatByName(name);
        if (stat == null) return false;
        if (stat is Resource) stat = (Resource)stat;

        stat.SetBaseValue(value);
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool SetResourceMax(string name, int max) {
        Resource resource = GetResourceByName(name);
        if (resource == null) return false;

        resource.SetMaxValue(max);
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool SetResourceToMax(string name) {
        Resource resource = GetResourceByName(name);
        if (resource == null) return false;

        resource.SetBaseValue(resource.GetMaxValue());
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool IncrementStatBaseValue(string name, int amount) {
        return SetStatBaseValue(name, GetStatByName(name).GetBaseValue() + amount);
    }

    public bool IncrementResourceMax(string name, int amount) {
        return SetResourceMax(name, GetResourceByName(name).GetMaxValue() + amount);
    }
}
