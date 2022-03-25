using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Cell {
    // Core Data
    private int x;
    public int GetX => x;
    private int y;
    public int GetY => y;
    public Vector2Int GetXY => new Vector2Int(GetX, GetY);

    // External references
    private GridSystem<Cell> _parentGrid;
    public GridSystem<Cell> ParentGrid => _parentGrid;

    // Internal references
    private Tile _baseTile;
    private Tile _toSetTile;
    private Color _baseColor;
    private Color _toSetColor;
    public Tile BaseTile { get => _baseTile; set => _baseTile = value; }
    public Tile ToSetTile { get => _toSetTile; set { if (value != BaseTile) { _toSetTile = value; _parentGrid.TriggerGridObjectChanged(x, y); } } }
    public Color BaseColor { get => _baseColor; set => _baseColor = value; }
    public Color ToSetColor { get => _toSetColor; set { if (value != BaseColor) { _toSetColor = value; _parentGrid.TriggerGridObjectChanged(x, y); } } }

    public bool ContainsActor { get; set; }

    // Functionalities
    private PathNode _pathNode;
    public PathNode PathNode => _pathNode;

    public Cell(GridSystem<Cell> grid, int x, int y) {
        this._parentGrid = grid;
        this.x = x;
        this.y = y;
        _pathNode = new PathNode(this, x, y);
        ContainsActor = false;
    }

    public Vector3 ToWorld() {
        float cellSize = ParentGrid.CellSize;
        Vector3 cellOffset = new Vector3(cellSize, cellSize) / 2;
        return ParentGrid.GetWorldPosition(GetXY) + cellOffset;
    }

    public override string ToString() {
        return _baseTile?.ToString();
    }

    /*
     * Save - Load
     */
    /*
     * Details:
     * Fields included here can be any features you want to save within each object.
     */
    [System.Serializable]
    public class SaveObject {
        public Tile baseSprite;
        public int x;
        public int y;
        public int pathWeight;
        public bool pathIsWalkable;
    }

    public SaveObject Save() {
        return new SaveObject {
            baseSprite = _baseTile,
            x = x,
            y = y,
            pathWeight = _pathNode.Weight,
            pathIsWalkable = _pathNode.isWalkable,
        };
    }

    public void Load(SaveObject saveObject) {
        _baseTile = saveObject.baseSprite;

        PathNode pathNode = new PathNode(this, x, y);
        pathNode.Weight = saveObject.pathWeight;
        pathNode.isWalkable = saveObject.pathIsWalkable;
    }
}
