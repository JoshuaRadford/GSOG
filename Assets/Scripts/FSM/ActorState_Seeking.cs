public class ActorState_Seeking : IActorState {
    Actor parent;

    public ActorState_Seeking(Actor parent) {
        this.parent = parent;
    }

    public void Enter(params object[] args) {
        parent.Animator.SetTrigger("ToWalk");
    }

    public void DoState() {
        if (parent == null) return;

        // Check if the seek coroutine is running
        /*if (!behavior.parentActor.Movement.seeking) {
            behavior.ChangeState(behavior.STATE_WAITING);
            return;
        }*/
    }

    public void Exit() {
    }
}
