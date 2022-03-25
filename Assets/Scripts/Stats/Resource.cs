using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resource : Stat {
    public int MaxValue { get; set; }
    public int ModifiedMaxValue => (int)ApplyModifiers(MaxValue, MaxValueModifiers);
    public override int ModifiedValue => (int)Mathf.Min(ApplyModifiers(BaseValue, BaseValueModifiers), ModifiedMaxValue);

    public List<StatMod> MaxValueModifiers { get; }


    public Resource(string name, int baseValue = 50, int maxValue = 50) : base(name, baseValue) {
        MaxValue = maxValue;
        MaxValueModifiers = new List<StatMod>();
    }
}