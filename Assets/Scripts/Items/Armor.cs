using System.Collections.Generic;

public class Armor : Item {
    public List<StatMod> statModifiers;

    public override void MeleeAttack(IInteractable target) {
        throw new System.NotImplementedException();
    }

    public override void RangedAttack(IInteractable target) {
        throw new System.NotImplementedException();
    }
}
