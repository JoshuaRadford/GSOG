using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Stat {
    public string Name;
    [SerializeField]
    protected int baseValue;
    protected List<StatMod> baseValueModifiers;

    public Stat(string name = "[DEFAULT STAT]", int baseValue = 5) {
        Name = name;
        this.baseValue = baseValue;
        baseValueModifiers = new List<StatMod>();
    }

    public int GetBaseValue() { return baseValue; }
    public void SetBaseValue(int val) { baseValue = val; }
    public void IncrementBaseValue(int val) {
        SetBaseValue(baseValue + val);
    }
    public List<StatMod> GetBaseValueModifiers() { return baseValueModifiers; }
    public virtual int GetModifiedValue() {
        return (int)ApplyModifiers(baseValue, baseValueModifiers);
    }

    public void AddModifiers(List<StatMod> addTo, params StatMod[] mods) {
        addTo.AddRange(mods);
    }
    public int RemoveModifiers(List<StatMod> removeFrom, params StatMod[] mods) {
        int count = 0;
        foreach (StatMod mod in mods) {
            count += removeFrom.Remove(mod) ? 1 : 0;
        }
        return count;
    }
    public int RemoveModifiersFromSource(object source, List<StatMod> mods) {
        int count = 0;
        for (int i = mods.Count; i >= 0; i--) {
            if (mods[i].Source == source) {
                bool success = mods.Remove(mods[i]);
                count += success ? 1 : 0;
            }
        }
        return count;
    }

    public void AddBaseValueModifiers(params StatMod[] mods) {
        AddModifiers(baseValueModifiers, mods);
    }
    public int RemoveBaseValueModifiers(params StatMod[] mods) {
        return RemoveModifiers(baseValueModifiers, mods);
    }
    public int RemoveBaseValueModifiersFromSource(object source) {
        return RemoveModifiersFromSource(source, baseValueModifiers);
    }

    protected float ApplyModifiers(float statBase, List<StatMod> mods) {
        float finalValue = statBase;

        if (mods != null && mods.Count > 0) {
            mods.Sort((x, y) => x.Type.CompareTo(y.Type));

            for (int i = 0; i < mods.Count; i++) {
                StatMod modifier = mods[i];
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