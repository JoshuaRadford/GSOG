using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Item", menuName = "Assets/Item")]
public class Item : ScriptableObject
{
    private const string DEFAULT_ITEM_NAME = "DEFAULT_ITEM_NAME";
    private const float DEFAULT_ITEM_WEIGHT = 1f;
    private const float DEFAULT_ITEM_PRICE = 1f;

    [SerializeField] private string uniqueID;
    [SerializeField]  private string itemName;
    [SerializeField] private float weight;
    [SerializeField] private float basePrice;

    public string UniqueID { get { return uniqueID; } set { uniqueID = value; } }
    public string Name { get { return itemName; } set { itemName = value; } }
    public float Weight { get { return weight; } set { weight = value; } }
    public float BasePrice { get { return basePrice; } set { basePrice = value; } }

    public Item(string _name = DEFAULT_ITEM_NAME, float weight = DEFAULT_ITEM_WEIGHT, float basePrice = DEFAULT_ITEM_PRICE)
    {
        Name = _name;
        uniqueID = Guid.NewGuid().ToString();
        this.weight = weight;
        this.basePrice = basePrice;
    }
}