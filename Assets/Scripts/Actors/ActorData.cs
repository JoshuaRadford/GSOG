using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction {
    None,
    Greg,
    Mercenary,
}
public enum Inclination {
    Neutral,
    Positive, 
    Negative,
}
public enum Personality {
    Neutral,
    Benevolent,
    Aggressive,
}

[Serializable]
public class ActorData
{
    public string ActorName;
    public string ID;

    public Inventory Inventory;
    [SerializeField]
    public R_Health Health;
    public R_Stamina Stamina;
    public R_Mana Mana;
    public S_Strength Strength;
    public S_Dexterity Dexterity;

    public Faction Faction;
    public StringIntDictionary Inclination_ID;
    public StringIntDictionary Inclination_Name;
    public FactionIntDictionary Inclination_Faction;
    public Personality Personality;

    public ActorData(Faction Faction = Faction.None, Personality Personality = Personality.Neutral, string ActorName = "[DEFAULT]", string ID = "") {
        this.ActorName = ActorName;
        this.ID = (ID == "") ? Guid.NewGuid().ToString() : ID;

        // Initialize Stats
        Health = new R_Health();
        Stamina = new R_Stamina();
        Mana = new R_Mana();
        Strength = new S_Strength();
        Dexterity = new S_Dexterity();

        this.Faction = Faction;
        Inclination_ID = (Inclination_ID == null) ? new StringIntDictionary() : Inclination_ID;
        Inclination_Name = (Inclination_Name == null) ? new StringIntDictionary() : Inclination_Name;
        Inclination_Faction = (Inclination_Faction == null) ? new FactionIntDictionary() : Inclination_Faction;
        this.Personality = Personality;
    }

    public Inclination GetInclination(Actor actor) {
        if(actor == null || actor.actorData == null) return Inclination.Neutral;
        actor.actorData.Inclination_ID.TryGetValue(ID, out int inc_id);
        actor.actorData.Inclination_Faction.TryGetValue(Faction, out int inc_fac);
        actor.actorData.Inclination_Name.TryGetValue(ActorName, out int inc_name);

        int inc = 0;
        inc += inc_id;
        inc += inc_fac;
        inc += inc_name;

        return (inc >= 75) ? Inclination.Negative : (inc <= 25) ? Inclination.Positive : Inclination.Neutral;
    }
}
