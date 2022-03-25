public class ActorState_Seeking : IActorState {
    public IActorState DoState(ActorBehavior ai) {
        if (!ai.parentActor.Movement.seekCoroutineRunning) {
            return ai.waitingState;
        }

        return ai.seekingState;
    }
}
