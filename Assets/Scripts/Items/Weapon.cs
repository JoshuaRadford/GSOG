using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public enum WeaponProperty
{
    Finesse,
    Light, 
    Range, 
    Reach,
    TwoHanded,
    Versatile,
}
public class Weapon : Item
{
    public List<StatModifier> statModifiers;
    public WeaponProperty weaponProperty;

    public float damage;
    public float meleeRange;
}
