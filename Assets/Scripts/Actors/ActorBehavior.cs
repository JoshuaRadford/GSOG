using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[System.Serializable]
public class ActorBehavior : MonoBehaviour {
    public event Action<IActorState, IActorState> OnBehaviorChange;

    public Actor parentActor;
    public ActorState_Idle idleState = new ActorState_Idle(); // Taking no actions, No priority
    public ActorState_Waiting waitingState = new ActorState_Waiting(); // Taking no actions, Has priority
    public ActorState_Seeking seekingState = new ActorState_Seeking(); // Moving at target location
    public ActorState_Attacking attackingState = new ActorState_Attacking(); // Attacking target object


    private IActorState _currentState;
    public IActorState CurrentState {
        get => _currentState;
        set {
            IActorState temp = CurrentState;
            _currentState = value;
            if (value != temp) OnBehaviorChange?.Invoke(temp, value);
        }
    }
    private HashSet<Actor> _hostiles;
    public HashSet<Actor> Hostiles { get => _hostiles; set => _hostiles = value; }

    public bool isEnabled = false;
    public const float ACTION_BUFFER = 1.0f;

    private void Awake() {
        if (parentActor == null) parentActor = GetComponent<Actor>();
    }

    private void OnEnable() {
        parentActor.OnTurnStart += () => { CurrentState = waitingState; };
        parentActor.OnTurnEnd += () => { CurrentState = idleState; };
        //OnBehaviorChange += (IActorState oldState, IActorState newState) => { Debug.Log(parentActor.name + " from " + oldState.ToString() + " to " + newState.ToString()); };
    }

    void Start() {
        Hostiles = new HashSet<Actor>();
        _currentState = idleState;
    }

    void Update() {
        if (isEnabled) {
            CurrentState = CurrentState.DoState(this);
        }
    }

    public void AddHostile(Actor actor) {
        Hostiles.Add(actor);
    }

    public Actor GetClosestHostile(CellMap cellMap) {
        if (cellMap != null && Hostiles?.Count > 0) {
            int lowDist = int.MaxValue;
            Actor toReturn = null;

            foreach (Actor hostile in Hostiles) {
                Cell hCell = hostile.Movement.CellPosition;
                Cell thisCell = parentActor.Movement.CellPosition;
                int distCost = Pathfinding.CalculateDistanceCost(thisCell.PathNode, hCell.PathNode);
                if (distCost < lowDist) {
                    lowDist = distCost;
                    toReturn = hostile;
                }
            }

            return toReturn;
        }

        return null;
    }

    public List<PathNode> GetPathToClosestHostile() {
        Actor closestHostile = GetClosestHostile(Manager_Grid.TerrainOverlay);

        if (closestHostile != null) {
            Vector2Int thisTerrainIndex = parentActor.Movement.CellPosition.GetXY;
            Vector2Int hostileTerrainIndex = closestHostile.Movement.CellPosition.GetXY;
            HashSet<Cell> toIgnoreWalkable = new HashSet<Cell>()
            {
                Manager_Grid.TerrainOverlay.GetGrid().GetGridObject(hostileTerrainIndex),
            };

            HashSet<PathNode> walkables = parentActor.Movement.GetWalkables(Manager_Grid.TerrainOverlay);

            List<PathNode> fullPath = Manager_Grid.GetTerrainPath(thisTerrainIndex, hostileTerrainIndex, toIgnoreWalkable);
            Manager_Grid.DebugPath(fullPath, 5f);
            List<PathNode> walkablePath = Manager_Grid.FilterValidPath(fullPath, walkables);
            Manager_Grid.GetInstance().DEBUGPATH = walkablePath;

            return walkablePath;
        }

        return null;
    }

    public bool TryMoveAtClosestHostile() {
        List<PathNode> path = GetPathToClosestHostile();

        if (path != null && path.Count > 0) {
            PathNode pathToHostileEnd = path[path.Count - 1];
            parentActor.Movement.SetTargetCell(pathToHostileEnd.parentCell);
            return true;
        }

        return false;
    }

    public bool TryAttack(Actor actor) {
        if (!parentActor.ExpendedSA) {
            Debug.Log("ATTACK");
            actor.Stats.IncrementStatBaseValue("Health", -parentActor.Stats.GetStatByName("Strength").ModifiedValue * 3);
            parentActor.ExpendedSA = true;
            actor.TryIsDead();
        }
        return parentActor.ExpendedSA;
    }

    public bool TryDashAction() {
        if (!parentActor.ExpendedSA) {
            parentActor.ExpendedSA = true;
            parentActor.Stats.SetResourceToMax("Stamina");
        }

        return parentActor.ExpendedSA;
    }
}
