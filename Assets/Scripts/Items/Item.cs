using System;
using UnityEngine;

public abstract class Item : IInteractable {
    private const string DEFAULT_ITEM_NAME = "DEFAULT_ITEM_NAME";
    private const float DEFAULT_ITEM_WEIGHT = 1f;
    private const int DEFAULT_ITEM_DURABILITY = 10;
    private const int DEFAULT_ITEM_PRICE = 1;

    [SerializeField] private string _uniqueID;
    [SerializeField] private string _itemName;
    [SerializeField] private float _weight;
    [SerializeField] private int _durability_current;
    [SerializeField] private int _durability_max;
    [SerializeField] private int _basePrice;

    public string UniqueID { get => _uniqueID; set => _uniqueID = value; }
    public string Name { get => _itemName; set => _itemName = value; }
    public float Weight { get => _weight; set => _weight = value; }
    public int Durability_Current { get => _durability_current; set => _durability_current = Mathf.Clamp(value, 0, Durability_Max); }
    public int Durability_Max { get => _durability_max; set => _durability_max = value; }
    public int BasePrice { get => _basePrice; set => _basePrice = value; }

    public Item(string _name = DEFAULT_ITEM_NAME, float _weight = DEFAULT_ITEM_WEIGHT,
        int _durability_max = DEFAULT_ITEM_DURABILITY, int _basePrice = DEFAULT_ITEM_PRICE) {
        Name = _name;
        UniqueID = Guid.NewGuid().ToString();
        Weight = _weight;
        Durability_Max = _durability_max;
        Durability_Current = _durability_max;
        BasePrice = _basePrice;
    }

    public int GetHP() { return Durability_Current; }
    public void SetHP(int hp) { Durability_Current = hp; }

    public abstract void RangedAttack(IInteractable target);

    public abstract void MeleeAttack(IInteractable target);
}