using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CodeMonkey.Utils;

public struct CellProfile {
    public Tile tile;
    public Color color;
}
public class NavGrid : MonoBehaviour{
    // Map Info
    public const int MAP_WIDTH = 50;
    public const int MAP_HEIGHT = 50;
    public const float CELL_SIZE = 1f;
    public Vector3 ORIGIN = new Vector3(0, 0, 0);
    public Tile WalkablesTile, PathTile, EmptyTile;

    public CellGrid navigationGrid;
    public Tilemap terrainBaseTilemap;
    public Tilemap terrainOverlayTilemap;
    public Tilemap movementOverlayTilemap;
    public Dictionary<Vector2Int, CellProfile> terrainBaseProfiles;
    public Dictionary<Vector2Int, CellProfile> terrainOverlayProfiles;
    public Dictionary<Vector2Int, CellProfile> movementOverlayProfiles;

    // Actor Info
    public Actor activeActor;

    // Navigation Info
    public HashSet<Vector2Int> activeWalkables;
    public List<Vector2Int> activePath;

    // Input Info
    private Vector3 mouseWorldPosition = Vector3.zero;
    private Vector3 mousePreviousWorldPosition = Vector3.zero;
    private Vector2Int mouseOverIndex = Vector2Int.zero;
    private Vector2Int mouseOverPreviousIndex = Vector2Int.zero;

    public bool Synced() {
        return (terrainBaseTilemap != null && terrainOverlayTilemap != null && movementOverlayTilemap != null
            && terrainBaseProfiles != null && terrainOverlayProfiles != null && movementOverlayProfiles != null
            && navigationGrid != null && activeActor != null);
    }

    private void Awake() {
        
    }

    private void OnEnable() {
        
    }

    private void Start() {
        Init();
        foreach (Actor actor in FindObjectsOfType<Actor>()) {
            bool makeActive = !actor.enableAI;
            PlaceActorAuto(actor, makeActive);
        }
        UpdateDrawSyncMovement();
    }

    public void Update() {
        if (Synced()) {
            RefreshMouseData();

            // check GameManager.GetInstance().GAME_STATE_MAIN != GameState.Menu
            if (activeActor != null) {
                if (!activeActor.enableAI) {
                    HandleInput();
                }
            }
        }
    }

    public void Init(Tilemap TBM = null, Tilemap TOM = null, Tilemap MOM = null) {
        terrainBaseTilemap = (TBM == null) ? terrainBaseTilemap : TBM;
        terrainOverlayTilemap = (TOM == null) ? terrainOverlayTilemap : TOM;
        movementOverlayTilemap = (MOM == null) ? movementOverlayTilemap : MOM;
        movementOverlayProfiles = new Dictionary<Vector2Int, CellProfile>();

        if (terrainBaseTilemap != null) {
            BoundsInt baseBounds = terrainBaseTilemap.cellBounds;
            terrainBaseTilemap.layoutGrid.cellSize = new Vector3(CELL_SIZE, CELL_SIZE, 0);
            terrainBaseProfiles = GetProfilesFromTilemap(terrainBaseTilemap);
            navigationGrid = GetCellGridFromTilemap(terrainBaseTilemap);

            // Assign movement overlay profile for each terrain base tile
            foreach (Vector3Int index in terrainBaseTilemap.cellBounds.allPositionsWithin) {
                movementOverlayProfiles[(Vector2Int)index] = new CellProfile() { tile = EmptyTile, color = Color.white };
            }
        }
        if (terrainOverlayTilemap != null) {
            terrainOverlayProfiles = GetProfilesFromTilemap(terrainOverlayTilemap);

            // Set navgrid walkables based on the terrain overlay tiles
            foreach (Vector3Int index in terrainOverlayTilemap.cellBounds.allPositionsWithin) {
                Tile tb = terrainOverlayTilemap.GetTile<Tile>(index);
                navigationGrid.Grid[(Vector2Int)index].PathNode.Walkable = (tb == null);
            }
        }
        if (movementOverlayTilemap != null) {
            foreach (Vector3Int index in terrainOverlayTilemap.cellBounds.allPositionsWithin) {
                movementOverlayTilemap.RemoveTileFlags(index, TileFlags.LockColor);
            }
        }
    }

    public void PlaceActor(Actor actor, Vector2Int index, bool makeActive = false) {
        ActorUtils.PlaceActor(this, actor, index, makeActive);
    }

    public void PlaceActorAuto(Actor actor, bool makeActive = false) {
        ActorUtils.PlaceActorFromWorld(this, actor, makeActive);
    }

    private void RefreshMouseData() {
        // Refresh mouse position
        if (mouseWorldPosition != null) mousePreviousWorldPosition = mouseWorldPosition;
        mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
        // Refresh mouse cell index
        if (mouseOverIndex != null) mouseOverPreviousIndex = mouseOverIndex;
        mouseOverIndex = navigationGrid.GetIndex(mouseWorldPosition);
    }

    private void HandleInput() {
        Vector2Int targetIndex = Vector2Int.zero;
        bool foundIndex = false;

        if (activeActor.GetBehaviorState() is ActorState_Waiting) {
            // Direcitonal Movement
            if (Input.GetKey(KeyCode.W)) {
                activeActor.Facing = FaceDirection.UP;
                targetIndex = activeActor.NavGridCell.Up.GetXY();
                foundIndex = true;
            }
            if (Input.GetKey(KeyCode.A)) {
                activeActor.Facing = FaceDirection.LEFT;
                targetIndex = activeActor.NavGridCell.Left.GetXY();
                foundIndex = true;
            }
            if (Input.GetKey(KeyCode.S)) {
                activeActor.Facing = FaceDirection.DOWN;
                targetIndex = activeActor.NavGridCell.Down.GetXY();
                foundIndex = true;
            }
            if (Input.GetKey(KeyCode.D)) {
                activeActor.Facing = FaceDirection.RIGHT;
                targetIndex = activeActor.NavGridCell.Right.GetXY();
                foundIndex=true;
            }

            // Click or Hold MouseLeft
            if ((Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) && !UtilsClass.IsPointerOverUI()) {
                if (navigationGrid.IsWithinGrid(mouseWorldPosition)) {
                    if (mouseOverIndex != mouseOverPreviousIndex) {
                        if(activeWalkables == null) UpdateWalkables();
                        UpdatePath();
                        DrawMovement();
                        SyncMovementOverlayProfileToTilemap();
                    }
                }
            }

            // Unclick MouseLeft
            if (Input.GetMouseButtonUp(0) && !UtilsClass.IsPointerOverUI()) {
                if (navigationGrid.IsWithinGrid(mouseWorldPosition) && activePath != null && activePath.Count > 0) {
                    targetIndex = activePath[activePath.Count - 1];
                    foundIndex = true;
                }
            }

            // If we input a target cell, move to it
            if (foundIndex && activeWalkables.Count > 0) {
                activeActor.SetTargetCell(targetIndex);
                activePath.Clear();
            }
        }
    }

    public void UpdateWalkables() {
        if (activeActor == null || activeActor.NavGrid == null) return;
        // Update Walkables
        HashSet<Vector2Int> newWalkables = ActorUtils.GetWalkables(activeActor, activeActor.NavGrid.navigationGrid);
        if (activeWalkables != newWalkables) activeWalkables = newWalkables;
    }

    public void UpdatePath() {
        if (activeActor == null || navigationGrid == null) return;

        // Update Path
        List<Vector2Int> newPath = Pathfinding.FindPath(navigationGrid.GetIndex(activeActor.WorldPosition), navigationGrid.GetIndex(mouseWorldPosition), navigationGrid)?.Select(node => node.GetXY()).ToList();
        newPath = Pathfinding.FilterPath(newPath, activeWalkables, navigationGrid);
        if (newPath != activePath) activePath = newPath;
    }

    public void DrawMovement() {
        movementOverlayProfiles.Clear();

        if (activeWalkables == null) return;

        // Apply Walkables
        foreach (Vector2Int index in activeWalkables) {
            movementOverlayProfiles[index] = new CellProfile() { tile = WalkablesTile, color = new Color(1, 0.92f, 0.016f, 0.5f) }; ;
        }

        // Apply Path
        foreach (Vector2Int index in activePath) {
            movementOverlayProfiles[index] = new CellProfile() { tile = PathTile, color = new Color(40/255f, 121/255f, 104/255f) };
        }
    }

    public void UpdateDrawSyncMovement() {
        UpdateWalkables();
        //UpdatePath();
        DrawMovement();
        SyncMovementOverlayProfileToTilemap();
    }

    private CellGrid GetCellGridFromTilemap(Tilemap tilemap) {
        if(tilemap == null) return null;

        return new CellGrid(tilemap);
    }

    private Dictionary<Vector2Int, CellProfile> GetProfilesFromTilemap(Tilemap tilemap) {
        if(tilemap == null) return null;

        Dictionary<Vector2Int, CellProfile> cellProfiles = new Dictionary<Vector2Int, CellProfile>();

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++) {
            for (int y = bounds.min.y; y < bounds.max.y; y++) {
                var tilePosition = new Vector3Int(x, y, 0);
                CellProfile profile = new CellProfile();
                Tile tile = tilemap.GetTile<Tile>(tilePosition);
                Color color = tilemap.GetColor(tilePosition);
                profile.tile = (tile == null) ? EmptyTile : tile;
                profile.color = (color == null) ? Color.white : color;
                cellProfiles[new Vector2Int(x, y)] = profile;
            }
        }

        return cellProfiles;
    }

    public void SyncProfileToTilemap(Dictionary<Vector2Int, CellProfile> cellProfiles, Tilemap tilemap) {
        tilemap.ClearAllTiles();
        foreach (KeyValuePair<Vector2Int, CellProfile> pair in cellProfiles) {
            Vector3Int tilePos = (Vector3Int)pair.Key;
            Tile currentTile = tilemap.GetTile<Tile>(tilePos);
            if (currentTile != pair.Value.tile) tilemap.SetTile(tilePos, pair.Value.tile);
            tilemap.SetTileFlags(tilePos, TileFlags.None);
            if (currentTile?.color != pair.Value.color) tilemap.SetColor(tilePos, pair.Value.color);
        }
    }

    public void SyncMovementOverlayProfileToTilemap() {
        SyncProfileToTilemap(movementOverlayProfiles, movementOverlayTilemap);
    }

    private void OnDrawGizmos() {
        if (navigationGrid != null) {
            Gizmos.color = Color.yellow;
            foreach (KeyValuePair<Vector2Int, Cell> pair in navigationGrid.Grid) {
                Gizmos.DrawSphere(navigationGrid.GetWorldPosition(pair.Key), 0.2f);
            }
        }
        if (activeWalkables != null) {
            Gizmos.color = Color.red;
            foreach (Vector2Int index in activeWalkables) {
                Gizmos.DrawSphere(navigationGrid.GetWorldPosition(index), 0.1f);
            }
        }
        if (activePath != null) {
            Gizmos.color = Color.green;
            foreach (Vector2Int index in activePath) {
                Gizmos.DrawSphere(navigationGrid.GetWorldPosition(index), 0.1f);
            }
        }
        if (mouseWorldPosition != null && navigationGrid != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(navigationGrid.GetWorldPosition(navigationGrid.GetIndex(mouseWorldPosition)), 0.1f);
        }
    }
}