using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Assets/Item")]
public class Item : ScriptableObject {
    private const string DEFAULT_ITEM_NAME = "DEFAULT_ITEM_NAME";
    private const float DEFAULT_ITEM_WEIGHT = 1f;
    private const float DEFAULT_ITEM_PRICE = 1f;

    [SerializeField] private string uniqueID;
    [SerializeField] private string itemName;
    [SerializeField] private float weight;
    [SerializeField] private float basePrice;

    public string UniqueID { get => uniqueID; set => uniqueID = value; }
    public string Name { get => itemName; set => itemName = value; }
    public float Weight { get => weight; set => weight = value; }
    public float BasePrice { get => basePrice; set => basePrice = value; }

    public Item(string _name = DEFAULT_ITEM_NAME, float weight = DEFAULT_ITEM_WEIGHT, float basePrice = DEFAULT_ITEM_PRICE) {
        Name = _name;
        uniqueID = Guid.NewGuid().ToString();
        this.weight = weight;
        this.basePrice = basePrice;
    }
}