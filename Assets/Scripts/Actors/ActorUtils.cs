using CodeMonkey.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ActorUtils {
    public static HashSet<Vector2Int> GetWalkables(Actor source, CellGrid grid) {
        if (source == null || grid == null) return null;
        return Pathfinding.GetMovesWithinCost(source.NavGridPosition, source.GetMoveRange(), true, source.NavGrid.navigationGrid);
    }

    public static bool CanMove(Actor source, CellGrid grid) {
        return GetWalkables(source, grid)?.Count > 0;
    }

    public static bool IsNeighbor(Actor source, Actor target) {
        if (target == null || source == null) return false;
        return Pathfinding.GetNeighborList(source.NavGridPosition, source.NavGrid.navigationGrid).Contains(target.NavGridCell.PathNode);
    }

    public static Actor GetClosestHostile(Actor source, CellGrid grid) {
        if (grid == null || source.Hostiles?.Count <= 0) return null;

        int lowDist = int.MaxValue;
        Actor toReturn = null;

        foreach (Actor hostile in source.Hostiles) {
            if (hostile.NavGrid.navigationGrid != source.NavGrid.navigationGrid) continue;

            int distCost = Pathfinding.CalculateDistanceCost(source.NavGridPosition, hostile.NavGridPosition, grid);

            if (distCost < lowDist) {
                lowDist = distCost;
                toReturn = hostile;
            }
        }

        return toReturn;
    }

    public static void PlaceActor(NavGrid grid, Actor actor, Vector2Int index, bool makeActive = false) {
        if (grid == null || actor == null || grid.navigationGrid.GetCell(index) == null) {
            if (grid == null) Debug.Log("PlaceActor() FAILED: NavGrid is null");
            if (actor == null) Debug.Log("PlaceActor() FAILED: Actor is null");
            if (grid.navigationGrid.GetCell(index) == null) Debug.Log("PlaceActor() FAILED: Cell Index is null");
            return;
        }

        if (makeActive) grid.activeActor = actor;
        actor.NavGrid = grid;
        actor.WorldPosition = grid.navigationGrid.GetWorldPosition(index);
        actor.SetBaseNavGridPosition(index);
        actor.NavGridCell.ContainsActor = true;
        actor.NavGridCell.PathNode.Walkable = false;
        //Debug.Log($"Actor Placed at {index} in {grid.ToString()}");
    }

    public static void PlaceActorFromWorld(NavGrid grid, Actor actor, bool makeActive = false) {
        if (grid == null || actor == null) {
            if (grid == null) Debug.Log("PlaceActor() FAILED: NavGrid is null");
            if (actor == null) Debug.Log("PlaceActor() FAILED: Actor is null");
            return;
        }

        Vector2Int index = grid.navigationGrid.GetIndex(actor.transform.position);
        Cell cell = grid.navigationGrid.GetCell(index);

        if(cell == null) {
            Debug.Log("PlaceActor() FAILED: Cell Index is null");
            return;
        }

        PlaceActor(grid, actor, index, makeActive);
    } 

    public static List<Vector2Int> GetPathToActor(Actor source, Actor target) {
        if (target == null || source == null) return null;
        if(source.NavGrid.navigationGrid != target.NavGrid.navigationGrid) {
            Debug.Log("GetPathToActor() FAILED - Actors not on same CellGrid");
        }
        CellGrid grid = source.NavGrid.navigationGrid;

        Vector2Int thisTerrainIndex = source.NavGridPosition;
        Vector2Int hostileTerrainIndex = target.NavGridPosition;
        HashSet<Vector2Int> toIgnoreWalkable = new HashSet<Vector2Int>()
        {
            target.NavGridPosition,
        };

        HashSet<Vector2Int> walkables = GetWalkables(source, grid);

        List<Vector2Int> fullPath = Pathfinding.FindPath(thisTerrainIndex, hostileTerrainIndex, grid, toIgnoreWalkable).Select(node => node.GetXY()).ToList();
        List<Vector2Int> walkablePath = Pathfinding.FilterPath(fullPath, walkables, source.NavGrid.navigationGrid);

        return walkablePath;
    }

    public static List<Vector2Int> GetPathToClosestHostile(Actor source) {
        return GetPathToActor(source, GetClosestHostile(source, source.NavGrid.navigationGrid));
    }

    public static bool TryMoveAtActor(Actor source, Actor target, Action onArrive = null) {
        List<Vector2Int> path = GetPathToActor(source, target);

        if (path != null && path.Count > 0) {
            Vector2Int pathToActorEnd = path[path.Count - 1];
            source.SetTargetCell(pathToActorEnd, onArrive);
            return true;
        }
        else {
            onArrive?.Invoke();
        }

        return false;
    }

    public static bool CanStaticAttack(Actor source, Actor target) {
        if (source.NavGrid.navigationGrid != target.NavGrid.navigationGrid) return false;

        bool waiting = source.GetBehaviorState() is ActorState_Waiting;
        bool usingAI = !source.enableAI;
        bool neighbor = IsNeighbor(source, target);
        return waiting && usingAI && neighbor;
    }

    public static bool CanMoveAndAttack(Actor source, Actor target) {
        List<Vector2Int> path = GetPathToActor(source, target);
        return (path != null && path.Count > 0) || CanStaticAttack(source, target);
    }

    public static void TryManualAttack(Actor source, Actor target) {
        if (CanStaticAttack(source, target)) {
            source.TryAttack(target);
        }
    }

    public static bool TryMoveAtClosestHostile(Actor source) {
        return TryMoveAtActor(source, GetClosestHostile(source, source.NavGrid.navigationGrid));
    }

    public static bool TryDashAction(Actor source) {
        if (!source.ExpendedSA) {
            source.ExpendedSA = true;
            source.actorData.Stamina.SetBaseToMax();
            UtilsClass.CreateWorldTextPopup(null, "DASH",
                source.WorldPosition, 5, Color.white,
                source.WorldPosition + new Vector3(0, 3, 0), 1f);
        }

        return source.ExpendedSA;
    }

    public static void TryDie(Actor source) {
        if (source.GetHP() <= 0) Die(source);
    }

    public static void Die(Actor source) {
        source.TriggerDeath();
        UnityEngine.Object.Destroy(source.gameObject);
    }
}
