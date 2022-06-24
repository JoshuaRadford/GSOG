using System.Collections.Generic;

public class Weapon : Item {
    public List<StatMod> StatMods;
    public int Damage;

    public override void MeleeAttack(IInteractable target) {

    }

    public override void RangedAttack(IInteractable target) {

    }
}
