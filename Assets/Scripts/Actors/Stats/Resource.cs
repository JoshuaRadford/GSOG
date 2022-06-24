using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resource : Stat {
    [SerializeField]
    protected int maxValue;

    protected List<StatMod> maxValueModifiers;
    [SerializeField]
    public Resource(string name, int baseValue = 100, int maxValue = 100) : base(name, baseValue) {
        this.maxValue = maxValue;
        maxValueModifiers = new List<StatMod>();
    }

    public int GetMaxValue() { return maxValue; }
    public void SetMaxValue(int val) { maxValue = val; }

    public List<StatMod> GetMaxValueModifiers() { return maxValueModifiers; }

    public override int GetModifiedValue() {
        return (int)Mathf.Min(ApplyModifiers(baseValue, baseValueModifiers), GetModifiedMaxValue());
    }
    public virtual int GetModifiedMaxValue() {
        return (int)ApplyModifiers(maxValue, maxValueModifiers);
    }

    public void SetBaseToMax() {
        SetBaseValue(GetModifiedMaxValue());
    }
}