public class ActorState_Idle : IActorState {
    Actor parent;

    public ActorState_Idle(Actor parent) {
        this.parent = parent;
    }

    public void Enter(params object[] args) {
    }

    public void DoState() {
    }

    public void Exit() {
    }
}