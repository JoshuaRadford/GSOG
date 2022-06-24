using UnityEngine;
using System.Collections.Generic;

public class ActorState_Waiting : IActorState {
    Actor parent;

    public ActorState_Waiting(Actor parent) {
        this.parent = parent;
    }

    public void Enter(params object[] args) {
        if (parent == null) return;

        // Handle Animation
        parent.Animator.SetTrigger("ToIdle");

        // Everything after this is AI instructions
        if (!parent.enableAI) return;

        // Movement data
        bool canMove = ActorUtils.CanMove(parent, parent.NavGrid.navigationGrid);

        // Hostile data
        Actor closestHostile = ActorUtils.GetClosestHostile(parent, parent.NavGrid.navigationGrid);
        List<Vector2Int> pathToClosestHostile = ActorUtils.GetPathToActor(parent, closestHostile);

        // Found path to closest hostile
        if (canMove && pathToClosestHostile != null && pathToClosestHostile.Count > 0) {
            // Try seek closest hostile
            parent.Wait(0.5f, () => ActorUtils.TryMoveAtActor(parent, closestHostile));
            return;
        }
        // No path connecting to hostile
        else {
            // Hasn't expended standard action
            if (!parent.ExpendedSA) {
                bool foundClosestHostile = ActorUtils.IsNeighbor(parent, closestHostile);
                // A hostile is in range
                if (foundClosestHostile) {
                    // Attack that hostile
                    parent.Wait(0.5f, () => { parent.SetBehaviorState(parent.STATE_ATTACKING);});
                    return;
                }
                // No hostile in range
                else {
                    // Try dash action
                    parent.Wait(0.5f, () => {
                        ActorUtils.TryDashAction(parent);
                        parent.SetBehaviorState(parent.STATE_WAITING);
                    });
                    return;
                }
            }
        }


        // No other possible actions

        // End turn
        parent.Wait(1f, () => CombatManager.StepInitiative());
        return;
    }

    public void DoState() {
    }

    public void Exit() {
    }
}
