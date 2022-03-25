using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorStats {
    [SerializeField]
    public List<Stat> stats;

    public delegate void CharacterStatsChanged();
    public CharacterStatsChanged OnStatsChanged;

    public ActorStats() {
        stats = new List<Stat>()
        {
            new Stat("Strength"),
            new Stat("Dexterity"),
            new Stat("Endurance"),
            new Stat("Wisdom"),
            new Stat("Charisma"),
            new Resource("Health"),
            new Resource("Stamina"),
            new Resource("Mana"),
        };
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

        stat.BaseValue = value;
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool SetResourceMax(string name, int max) {
        Resource resource = GetResourceByName(name);
        if (resource == null) return false;

        resource.MaxValue = max;
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool SetResourceToMax(string name) {
        Resource resource = GetResourceByName(name);
        if (resource == null) return false;

        resource.BaseValue = resource.MaxValue;
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool IncrementStatBaseValue(string name, int amount) {
        return SetStatBaseValue(name, GetStatByName(name).BaseValue + amount);
    }

    public bool IncrementResourceMax(string name, int amount) {
        return SetResourceMax(name, GetResourceByName(name).MaxValue + amount);
    }

    public int GetHealthValue() {
        Stat s = GetStatByName("Health");
        return (s != null) ? s.ModifiedValue : 0;
    }

    public int GetMoveRange() {
        Stat s = GetStatByName("Stamina");
        return (s != null) ? s.ModifiedValue : 0;
    }

    public int GetInitiative() {
        Stat s = GetStatByName("Dexterity");
        return (s != null) ? s.ModifiedValue : 0;
    }
}
