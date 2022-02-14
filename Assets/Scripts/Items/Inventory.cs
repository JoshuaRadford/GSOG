using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Assets/Inventory")]
public class Inventory : ScriptableObject
{
    [SerializeField] private Dictionary<string, Item> items = new Dictionary<string, Item>();

    public Dictionary<string, Item> Items { get { return items; }}

    public void AddItem(Item _item)
    {
        items.Add(_item.UniqueID, _item);
    }

    public bool RemoveItem(Item _item)
    {
        return items.Remove(_item.UniqueID);
    }

    public int GetCountOfName(string _name)
    {
        int count = 0;

        foreach(Item _item in items.Values)
        {
            if (_item.Name == _name) count++;
        }

        return count;
    }
}
