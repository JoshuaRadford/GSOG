using System.Collections.Generic;

public class ActorState_Waiting : IActorState {
    public IActorState DoState(ActorBehavior ai) {
        List<PathNode> pathToClosestHostile = ai.GetPathToClosestHostile();
        Actor closestHostile = ai.GetClosestHostile(Manager_Grid.TerrainOverlay);
        HashSet<PathNode> walkables = ai.parentActor.Movement.GetWalkables(Manager_Grid.TerrainOverlay);
        bool foundClosestHostile = ai.parentActor.Movement.IsNeighbor(closestHostile, Manager_Grid.TerrainOverlay);


        if (ai.TryMoveAtClosestHostile()) {
            return ai.seekingState;
        }
        else {
            if (!ai.parentActor.ExpendedSA) {
                if (foundClosestHostile) {
                    return ai.attackingState;
                }

                ai.TryDashAction();
                return ai.waitingState;
            }
        }




        Manager_GridCombat.StepInitiative();
        return ai.idleState;
    }
}
