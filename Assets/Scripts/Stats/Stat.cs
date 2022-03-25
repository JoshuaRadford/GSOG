using System.Collections.Generic;

[System.Serializable]
public class Stat {
    public virtual string Name { get; }
    public virtual int BaseValue { get; set; }
    public virtual int ModifiedValue => (int)ApplyModifiers(BaseValue, BaseValueModifiers);
    public virtual List<StatMod> BaseValueModifiers { get; }

    [Newtonsoft.Json.JsonConstructor]
    public Stat(string name, int baseValue = 5) {
        Name = name;
        BaseValue = baseValue;
        BaseValueModifiers = new List<StatMod>();
    }

    public void AddModifier(StatMod modToAdd, List<StatMod> addTo) {
        addTo.Add(modToAdd);
    }

    public void AddMultipleModifiers(List<StatMod> modsToAdd, List<StatMod> addTo) {
        foreach (StatMod mod in modsToAdd) {
            addTo.Add(mod);
        }
    }

    public bool RemoveModifier(StatMod modToRemove, List<StatMod> removeFrom) {
        if (removeFrom.Remove(modToRemove)) {
            return true;
        }
        return false;
    }

    public bool RemoveModifierFromSource(object source, List<StatMod> removeFrom) {
        bool removed = false;
        for (int i = removeFrom.Count; i >= 0; i--) {
            if (removeFrom[i] == source) {
                removed = true;
                removeFrom.RemoveAt(i);
            }
        }

        return removed;
    }

    protected float ApplyModifiers(float statBase, List<StatMod> statModifiers) {
        float finalValue = statBase;

        if (statModifiers.Count > 0) {
            statModifiers.Sort((x, y) => x.Type.CompareTo(y.Type));

            for (int i = 0; i < statModifiers.Count; i++) {
                StatMod modifier = statModifiers[i];
                if (modifier.Type == StatMod.ModType.Additive) {
                    finalValue += modifier.Value;
                }
                else if (modifier.Type == StatMod.ModType.Multiplicative) {
                    finalValue *= modifier.Value;
                }
            }
        }

        return finalValue;
    }
}