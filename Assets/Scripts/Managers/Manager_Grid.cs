using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Manager_Grid : Singleton<Manager_Grid> {
    // Tilemap, CellMap data
    private Grid _grid;
    public static Grid MainGrid { get => GetInstance()._grid; set => GetInstance()._grid = value; }
    [SerializeField]
    private Tilemap _terrainTilemap;
    public static Tilemap TerrainTilemap { get => GetInstance()._terrainTilemap; set => GetInstance()._terrainTilemap = value; }
    [SerializeField]
    private Tilemap _movementTilemap;
    public static Tilemap MovementTilemap { get => GetInstance()._movementTilemap; set => GetInstance()._movementTilemap = value; }
    private CellMap _terrainOverlay;
    public static CellMap TerrainOverlay { get => GetInstance()._terrainOverlay; set => GetInstance()._terrainOverlay = value; }
    private CellMap _movementOverlay;
    public static CellMap MovementOverlay { get => GetInstance()._movementOverlay; set => GetInstance()._movementOverlay = value; }


    private const int MAP_WIDTH = 50;
    private const int MAP_HEIGHT = 30;
    private const float CELL_SIZE = 1f;
    private Vector3 ORIGIN = new Vector3(0, 0, 0);
    public bool showDebug = true;

    public List<PathNode> DEBUGPATH = new List<PathNode>();

    protected override void Awake() {
        base.Awake();
    }

    private void OnEnable() {

    }

    void Start() {

    }

    void Update() {
        DebugPath(GetInstance().DEBUGPATH);
    }

    public static void InitMaps() {
        MainGrid = new GameObject("Grid").AddComponent<Grid>();
        MainGrid.cellSize = new Vector3(CELL_SIZE, CELL_SIZE, 0);
        MainGrid.transform.localPosition = GetInstance().ORIGIN;

        TerrainTilemap = new GameObject("TerrainTilemap").AddComponent<Tilemap>();
        TerrainTilemap.gameObject.AddComponent<TilemapRenderer>();
        TerrainTilemap.transform.SetParent(MainGrid.gameObject.transform);
        MovementTilemap = new GameObject("MovmementTilemap").AddComponent<Tilemap>();
        MovementTilemap.gameObject.AddComponent<TilemapRenderer>();
        MovementTilemap.transform.SetParent(MainGrid.gameObject.transform);

        TerrainTilemap.transform.localPosition = MainGrid.transform.localPosition;
        MovementTilemap.transform.localPosition = TerrainTilemap.transform.localPosition;
        TerrainOverlay = new CellMap(MAP_WIDTH, MAP_HEIGHT, MainGrid.cellSize.x, GetInstance().ORIGIN);
        MovementOverlay = new CellMap(MAP_WIDTH, MAP_HEIGHT, MainGrid.cellSize.x, GetInstance().ORIGIN);
        TerrainOverlay.SetAllCellTile(GameManager.GetInstance().TILE_TERRAINBASE);
        MovementOverlay.SetAllCellTile(null);

        TerrainOverlay.GetGrid().OnGridObjectChanged += (object sender, GridSystem<Cell>.OnGridObjectChangedEventArgs eventArgs) => {
            SyncTileWithCell(TerrainOverlay.GetGrid().GetGridObject(eventArgs.x, eventArgs.y), TerrainTilemap);
            Manager_Input.GetInstance().UpdateWalkables();
            Manager_Input.GetInstance().UpdatePath();
        };
        MovementOverlay.GetGrid().OnGridObjectChanged += (object sender, GridSystem<Cell>.OnGridObjectChangedEventArgs eventArgs) => {
            SyncTileWithCell(MovementOverlay.GetGrid().GetGridObject(eventArgs.x, eventArgs.y), MovementTilemap);
        };

        for (int i = 0; i < 100; i++) {
            Vector2Int index = TerrainOverlay.GetRandomWalkableCellIndex();
            PlaceUnwalkableTerrain(index);
        }
    }

    public static List<PathNode> GetTerrainPath(Vector2Int startIndex, Vector2Int endIndex, IEnumerable<Cell> ignoreWalkable = null) {
        return Pathfinding.FindPath(startIndex, endIndex, TerrainOverlay.GetGrid(), ignoreWalkable);
    }

    // Mask path with a set of valid nodes
    public static List<PathNode> FilterValidPath(List<PathNode> path, HashSet<PathNode> valids) {
        List<PathNode> longestValidPath = new List<PathNode>();

        if (path != null && valids != null) {
            for (int i = 0; i < path.Count - 1; i++) {
                if (valids.Contains(path[i + 1])) {
                    longestValidPath.Add(path[i + 1]);
                }
            }
        }

        return longestValidPath;
    }

    public static void TilePathNodesOnCellMap(HashSet<PathNode> path, CellMap cellMap, Tile tile) {
        if (path != null && cellMap != null) {
            foreach (PathNode node in path) {
                Vector2Int xy = new Vector2Int(node.parentCell.GetX, node.parentCell.GetY);
                cellMap.SetCellTile(xy, tile);
            }
        }
    }

    public static void ColorPathNodesOnCellMap(HashSet<PathNode> path, CellMap cellMap, Color color) {
        if (path != null && cellMap != null) {
            foreach (PathNode node in path) {
                Vector2Int xy = new Vector2Int(node.parentCell.GetX, node.parentCell.GetY);
                cellMap.SetCellColor(xy, color);
                //node.parentCell.ToSetColor = color;
            }
        }
    }

    // Draws debug line along the given path
    public static void DebugPath(List<PathNode> path, float seconds = 0.05f) {
        if (path != null) {
            for (int i = 0; i < path.Count - 1; i++) {
                // Draw the path to the hovered cell
                float cellSize = MainGrid.cellSize.x;
                Vector3 thisCellOrigin = path[i].parentCell.ParentGrid.GetWorldPosition(new Vector2Int(path[i].x, path[i].y));
                Vector3 nextCellOrigin = path[i + 1].parentCell.ParentGrid.GetWorldPosition(new Vector2Int(path[i + 1].x, path[i + 1].y));
                Debug.DrawLine(thisCellOrigin + Vector3.one * cellSize / 2, nextCellOrigin + Vector3.one * cellSize / 2, Color.green, seconds);
            }
        }
    }

    public static void PlaceUnwalkableTerrain(Vector2Int index) {
        // Ensure active character isn't moving and we aren't placing on top of active character
        //if (Manager_Input.ActiveActor == null || Manager_Input.ActiveActor.Behavior.CurrentState is ActorState_Waiting)
        //{
        // Grab grid and cell data
        Vector3Int tileIndex = new Vector3Int(index.x, index.y, 0);
        Cell thisCell = TerrainOverlay.GetGrid().GetGridObject(TerrainTilemap.CellToWorld(tileIndex));

        // Toggle cell's tile and cell's path node "isWalkable"
        if (!thisCell.ContainsActor) thisCell.ToSetTile = thisCell.PathNode.SetIsWalkable(!thisCell.PathNode.isWalkable) ? GameManager.GetInstance().TILE_TERRAINBASE : GameManager.GetInstance().TILE_TERRAINWALL;
        //}
    }

    /* Align tilemap and cellmap in world space
     * Update any changes to cellmap fields
     * */
    public static void SyncTileWithCell(Cell c, Tilemap tilemap) {
        if (c != null) {
            int x = c.GetX, y = c.GetY;

            if (c.ToSetTile != c.BaseTile) {
                c.BaseTile = c.ToSetTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), c.BaseTile);
            }
            if (c.ToSetColor != c.BaseColor) {
                c.BaseColor = c.ToSetColor;
                tilemap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                tilemap.SetColor(new Vector3Int(x, y, 0), c.BaseColor);
            }
        }
    }

    public static void SyncTilemapWithCellMap(CellMap cellMap, Tilemap tilemap) {
        for (int x = 0; x < cellMap.GetGrid().Width; x++) {
            for (int y = 0; y < cellMap.GetGrid().Height; y++) {
                SyncTileWithCell(cellMap.GetGrid().GetGridObject(x, y), tilemap);
            }
        }
    }

    // Preset to sync known tilemaps and cellmaps
    public static void SyncAllTileMapsWithCellmaps() {
        SyncTilemapWithCellMap(TerrainOverlay, TerrainTilemap);
        SyncTilemapWithCellMap(MovementOverlay, MovementTilemap);
    }

    // Sets color of all tiles in given map to given color
    public void SetAllTilesToColor(Tilemap map, Color color) {
        for (int x = 0; x < map.size.x; x++) {
            for (int y = 0; y < map.size.y; y++) {
                map.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                map.SetColor(new Vector3Int(x, y, 0), color);
            }
        }
    }
}