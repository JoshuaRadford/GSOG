public interface IActorState {
    // Performed when state begins
    void Enter(params object[] args);
    // Performed every frame
    void DoState();
    // Performed when state ends
    void Exit();
}
