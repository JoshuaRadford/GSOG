using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Assets/Character")]
public class Character_Base : ScriptableObject
{
    [SerializeField] public List<Stat> stats;
    [SerializeField] public Inventory inventory;

    public Character_Base()
    {
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

    public Stat GetStatByName(string name)
    {
        return stats.Find(s => s.Name == name);
    }

    public float GetHealthValue()
    {
        return GetStatByName("Health").ModifiedValue;
    }
}
