public class ActorState_Idle : IActorState {
    public IActorState DoState(ActorBehavior ai) {
        return ai.idleState;
    }
}