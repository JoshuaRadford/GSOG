public class ActorState_Attacking : IActorState {
    Actor parent;

    public ActorState_Attacking(Actor parent) {
        this.parent = parent;
    }

    public void Enter(params object[] args) {
        if (parent != null)

        // Everything after this is AI instructions
        if (!parent.enableAI) return;

        // Hostile data
        Actor closestHostile = ActorUtils.GetClosestHostile(parent, parent.NavGrid.navigationGrid);

        // Try melee attack on closest hostile
        parent.Wait(0.5f, () => parent.TryAttack(closestHostile));
    }

    public void DoState() {
    }

    public void Exit() {
    }
}
