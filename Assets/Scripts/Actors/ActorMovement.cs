/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class ActorMovement : MonoBehaviour {
    public Actor parentActor;

    public event Action onStopMoving;
    public event Action onStartMoving;
    private Coroutine seekCoroutine;
    public bool seekCoroutineRunning = false;

    private int currentPathIndex;
    private List<Cell> pathVectorList;

    private float elapsedTime;
    private float waitTime = 0.25f;
    private Cell _cellPosition;
    public Cell CellPosition {
        get => _cellPosition;
        set {
            Cell temp = _cellPosition;
            _cellPosition = value;

            if (temp != null && !value.Equals(temp)) {
                // Update walkable terrain path nodes
                temp.PathNode.SetIsWalkable(true);
                temp.ContainsActor = false;
                value.PathNode.SetIsWalkable(false);
                value.ContainsActor = true;

                // Invoke that we have changed cells
                if (parentActor.Equals(Manager_Input.ActiveActor)) Manager_Input.GetInstance().TriggerOnActiveCellChange(temp, value);
            }


        }
    }
    private Vector3 worldPosition;
    private Vector3 initialPosition;
    private const float LOCK_Z = -0.5f;

    private void Awake() {
        if (parentActor == null) parentActor = GetComponent<Actor>();
    }

    private void OnEnable() {
        onStartMoving += Manager_Grid.SyncAllTileMapsWithCellmaps;
    }

    private void Start() {
        worldPosition = transform.position;
    }

    private void Update() {
        if (CellPosition == null) CellPosition = GetMapCell(Manager_Grid.TerrainOverlay); ;
        transform.position = LockZ(worldPosition);
    }

    public void SetPositionToRandomWalkableCell(CellMap cellMap) {
        if(cellMap == null) {
            Debug.Log("COULD NOT \"SetPositionToRandomWalkableCell\" (\"cellMap\" IS NULL)");
            return;
        }

        Vector2Int randCellIndex = cellMap.GetRandomWalkableCellIndex();
        Cell randCell = cellMap.GetGrid().GetGridObject(randCellIndex);
        Vector3 toWorld = randCell.ToWorld();

        worldPosition = toWorld;
        CellPosition = randCell;
    }

    private Vector3 LockZ(Vector3 pos) {
        pos.z = LOCK_Z;
        return pos;
    }

    public Vector3 CellPositionToWorld() {
        return CellPosition.ParentGrid.GetWorldPosition(CellPosition.GetXY);
    }

    public Cell GetMapCell(CellMap cellMap) {
        return cellMap.GetGrid().GetGridObject(transform.position);
    }

    public HashSet<PathNode> GetWalkables(CellMap cellMap) {
        if (CellPosition != null) {
            PathNode thisPathNode = CellPosition.PathNode;
            int activeMoveRange = parentActor.Stats.GetMoveRange();
            return Pathfinding.GetWalkablesByCost(thisPathNode, activeMoveRange, true, cellMap.GetGrid());
        }
        return null;
    }

    public void SetTargetCell(Cell targetCell) {
        onStartMoving?.Invoke();

        elapsedTime = 0;
        currentPathIndex = 0;
        seekCoroutineRunning = true;

        pathVectorList = Pathfinding.FindPath(CellPosition, targetCell, Manager_Grid.TerrainOverlay.GetGrid());

        if (pathVectorList != null) {
            if (pathVectorList.Count > 1) pathVectorList.RemoveAt(0);
            if (seekCoroutine != null) StopCoroutine(seekCoroutine);

            initialPosition = transform.position;
            seekCoroutine = StartCoroutine(SeekTargetPosition());
        }
    }

    public bool CanMove() {
        return GetWalkables(Manager_Grid.TerrainOverlay)?.Count > 0;
    }

    public bool IsNeighbor(Actor actor, CellMap cellMap) {
        if (actor != null && cellMap != null) {
            Cell otherMapCell = actor.Movement.GetMapCell(cellMap);
            Cell thisMapCell = GetMapCell(cellMap);
            PathNode acPathNode = Manager_Grid.TerrainOverlay.GetGrid().GetGridObject(otherMapCell.GetX, otherMapCell.GetY).PathNode;
            PathNode thisPathNode = Manager_Grid.TerrainOverlay.GetGrid().GetGridObject(thisMapCell.GetX, thisMapCell.GetY).PathNode;
            return Pathfinding.GetNeighborList(thisPathNode, Manager_Grid.TerrainOverlay.GetGrid()).Contains(acPathNode);
        }

        return false;
    }

    IEnumerator SeekTargetPosition() {
        while (pathVectorList != null) {
            Vector3 targetPosition = pathVectorList[currentPathIndex].ToWorld();
            Vector3 startingPosition = (currentPathIndex - 1 >= 0) ? pathVectorList[currentPathIndex - 1].ToWorld() : initialPosition;

            for (int i = 0; i < pathVectorList.Count - 1; i++) Debug.DrawLine(pathVectorList[i].ToWorld(), pathVectorList[i + 1].ToWorld(), Color.red, 3f);

            float distanceBefore = Vector2.Distance(transform.position, targetPosition);

            if (distanceBefore > 0.05f) {
                worldPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / waitTime);
                elapsedTime += Time.deltaTime;
            }
            else {
                parentActor.Stats.IncrementStatBaseValue("Stamina", -Pathfinding.CalculateDistanceCost(CellPosition.PathNode, pathVectorList[currentPathIndex].PathNode));
                CellPosition = pathVectorList[currentPathIndex];

                elapsedTime = 0;
                currentPathIndex++;

                if (currentPathIndex >= pathVectorList.Count) {
                    worldPosition = targetPosition;

                    currentPathIndex = 0;
                    pathVectorList.Clear();

                    Manager_Grid.SyncAllTileMapsWithCellmaps();
                    onStopMoving?.Invoke();
                    seekCoroutineRunning = false;

                    StopCoroutine(seekCoroutine);
                }
            }

            yield return null;
        }
    }
}