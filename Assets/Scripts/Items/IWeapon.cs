using System.Collections.Generic;

public interface IWeapon {
    public List<StatMod> StatMods { get; set; }
    public int Damage { get; set; }
    void Attack();
}
