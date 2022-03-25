public class ActorState_Attacking : IActorState {
    public IActorState DoState(ActorBehavior ai) {
        Actor closestHostile = ai.GetClosestHostile(Manager_Grid.TerrainOverlay);
        ai.TryAttack(closestHostile);

        return ai.waitingState;
    }
}
