using CodeMonkey.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    Wandering,
    Combat,
    Menu,
}

/*
 * The primary function of this singleton is to filter the grid data 
 * through the state of the currently controlled (active) actor,
 * as well as the input of the player.
 */
public class Manager_Input : Singleton<Manager_Input> {
    // External references
    private Actor _activeActor;
    public static Actor ActiveActor {
        get => GetInstance()._activeActor;
        set {
            GetInstance()._activeActor = value;
            ActiveCell = value.Movement.CellPosition;
        }
    }
    private Cell _activeCell;
    public static Cell ActiveCell {
        get => GetInstance()._activeCell;
        set {
            GetInstance().TriggerOnActiveCellChange(GetInstance()._activeCell, value);
            GetInstance()._activeCell = value;
        }
    }
    private List<PathNode> _activePath;
    public static List<PathNode> ActivePath { get => GetInstance()._activePath; set => GetInstance()._activePath = value; }
    private HashSet<PathNode> _activeMoves;
    public static HashSet<PathNode> ActiveMoves { get => GetInstance()._activeMoves; set => GetInstance()._activeMoves = value; }

    // Internal data
    private Vector3 mouseWorldPosition;
    private Vector3 previousMouseWorldPosition = new Vector3();
    Vector2Int mouseoverTileIndex;
    Vector2Int mouseoverPreviousTileIndex = new Vector2Int();

    // Delegates
    public delegate void ActiveCharacterChanged();
    public ActiveCharacterChanged onActiveCharacterChanged;
    public event Action<Cell, Cell> OnActiveCellChange;

    protected override void Awake() {
        base.Awake();

        // Initialize external references
        ActiveActor = FindObjectOfType<Actor>();
    }

    private void OnEnable() {
        OnActiveCellChange += (Cell oldCell, Cell newCell) => {
            UpdateWalkables();
        };
    }

    public void TriggerOnActiveCellChange(Cell oldCell, Cell newCell) {
        OnActiveCellChange?.Invoke(oldCell, newCell);
    }

    void Start() {
        foreach (Actor c in FindObjectsOfType<Actor>()) {
            c.Movement.SetPositionToRandomWalkableCell(Manager_Grid.TerrainOverlay);
            if (c != ActiveActor) c.Behavior.AddHostile(ActiveActor);
        }

        Manager_GridCombat.RestartInitiative(new Vector2Int(3, 3), new Vector2Int(200, 200));
        Manager_Camera.GetInstance().ChangeCinemachineFollow(_activeActor.Movement.transform);

        UpdateWalkables();
        UpdatePath();
    }

    void Update() {
        // Refresh mouse data
        if (mouseWorldPosition != null) previousMouseWorldPosition = mouseWorldPosition;
        mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
        mouseoverTileIndex = Manager_Grid.TerrainOverlay.GetGrid().GetXY(mouseWorldPosition);
        if (!mouseoverTileIndex.Equals(mouseoverPreviousTileIndex)) mouseoverPreviousTileIndex = mouseoverTileIndex;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Manager_Scenes.LoadStartMenu();
        }

        if (ActiveActor != null) {
            if (!ActiveActor.Behavior.isEnabled) {
                // Click or Hold MouseLeft
                if ((Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) && !UtilsClass.IsPointerOverUI()) {
                    if (Manager_Grid.TerrainOverlay.GetGrid().IsWithinGrid(mouseWorldPosition)) {
                        if (ActiveActor.Behavior.CurrentState is ActorState_Waiting) {
                            UpdatePath();
                            TilePath();
                        }
                    }
                }

                // Unclick MouseLeft
                if (Input.GetMouseButtonUp(0) && !UtilsClass.IsPointerOverUI()) {
                    if (Manager_Grid.TerrainOverlay.GetGrid().IsWithinGrid(mouseWorldPosition) && ActivePath != null && ActivePath.Count > 0) {
                        PathNode endActivePath = ActivePath[ActivePath.Count - 1];
                        ActiveActor.Movement.SetTargetCell(endActivePath.parentCell);
                    }
                }

                // Click MouseRight
                if (Input.GetMouseButtonDown(1) && !UtilsClass.IsPointerOverUI()) {
                    if (!Input.GetMouseButton(0)) {
                        Manager_Grid.PlaceUnwalkableTerrain(mouseoverTileIndex);
                    }
                }
            }
        }
    }

    public void UpdateWalkables() {
        if (ActiveActor != null && Manager_Grid.TerrainOverlay != null) {
            if (ActiveActor.Movement != null) {
                PathNode activePathNode = ActiveActor.Movement.GetMapCell(Manager_Grid.TerrainOverlay).PathNode;
                HashSet<PathNode> newMoves = ActiveActor.Movement.GetWalkables(Manager_Grid.TerrainOverlay);
                if (newMoves != null && !newMoves.Equals(_activeMoves)) {
                    ActiveMoves = newMoves;
                    if (ActiveMoves.Count > 0) ActiveMoves.Add(activePathNode);
                    TileWalkables();
                }
            }
        }
    }

    private void TileWalkables() {
        if (Manager_Grid.MovementOverlay != null && ActiveMoves != null) {
            Manager_Grid.MovementOverlay.SetAllCellTile(null);
            Manager_Grid.TilePathNodesOnCellMap(ActiveMoves, Manager_Grid.MovementOverlay, GameManager.GetInstance().TILE_MOVEMENTBASE);
            ColorWalkables();
        }
    }

    public void UpdatePath() {
        if (ActiveActor != null && Manager_Grid.TerrainOverlay != null && ActiveMoves != null) {
            Vector2Int activeTerrainIndex = ActiveActor.Movement.CellPosition.GetXY;
            List<PathNode> newPath = Manager_Grid.FilterValidPath(Manager_Grid.GetTerrainPath(activeTerrainIndex, mouseoverTileIndex), ActiveMoves);
            if (!newPath.Equals(ActivePath)) {
                ActivePath = newPath;
                TileWalkables();
            }
        }
    }

    private void TilePath() {
        if (Manager_Grid.MovementOverlay != null && ActivePath != null) {
            Manager_Grid.TilePathNodesOnCellMap(new HashSet<PathNode>(ActivePath), Manager_Grid.MovementOverlay, GameManager.GetInstance().TILE_MOVEMENTPATH);
            ColorWalkables();
        }
    }

    private void ColorWalkables() {
        if (Manager_Grid.MovementOverlay != null && ActiveMoves != null && ActivePath != null) {
            Manager_Grid.MovementOverlay.SetAllCellColor(Color.white);
            Manager_Grid.ColorPathNodesOnCellMap(_activeMoves, Manager_Grid.MovementOverlay, new Color(1f, 1f, 1f, 0.5f));
        }
    }
}