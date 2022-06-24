using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellGrid {
    [SerializeField]
    private string _id;
    private Dictionary<Vector2Int, Cell> cellGrid;
    public Dictionary<Vector2Int, Cell> Grid { get { return cellGrid; } }

    private int _width;
    private int _height;
    private float _cellSize;
    private Vector3 _origin;
    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;
    public Vector3 Origin { get => _origin; set { if (_origin != value) _origin = value; } }
    public string ID => _id;

    public CellGrid(int width, int height, float cellSize, Vector3 origin, Vector2Int indexOrigin = default(Vector2Int), string id = "") {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _origin = origin - new Vector3(width * cellSize, height * cellSize, 0) / 2;
        _id = (id == "") ? Guid.NewGuid().ToString() : id;

        cellGrid = new Dictionary<Vector2Int, Cell>();

        for (int x = indexOrigin.x; x < width; x++) {
            for (int y = indexOrigin.y; y < height; y++) {
                cellGrid.Add(new Vector2Int(x, y), new Cell(this, x, y));
            }
        }
    }

    public CellGrid(Tilemap tilemap, string id = "") {
        _width = tilemap.size.x;
        _height = tilemap.size.y;  
        _cellSize = tilemap.layoutGrid.cellSize.x;
        _origin = tilemap.CellToWorld(Vector3Int.zero);// - new Vector3(_width * cellSize, _height * cellSize, 0) / 2;
        _id = (id == "") ? Guid.NewGuid().ToString() : id;

        cellGrid = new Dictionary<Vector2Int, Cell>();

        foreach(Vector3Int index in tilemap.cellBounds.allPositionsWithin) {
            if (!tilemap.HasTile(index)) continue;
            cellGrid.Add(new Vector2Int(index.x, index.y), new Cell(this, index.x, index.y));
        }
    }

    public Vector3 GetWorldPosition(Vector2Int index) {
        return new Vector3(index.x, index.y) * CellSize + Origin + new Vector3(CellSize / 2, CellSize / 2);
    }

    public Vector2Int GetIndex(Vector3 worldPosition) {
        Vector3 toCellScale = (worldPosition - _origin) / CellSize;
        int x = Mathf.FloorToInt(toCellScale.x), y = Mathf.FloorToInt(toCellScale.y);

        return new Vector2Int(x, y);
    }

    public Cell GetCell(Vector3 worldPosition) {
        Vector2Int index = GetIndex(worldPosition);
        if (!cellGrid.TryGetValue(index, out Cell cell)) Debug.Log($"*ERROR* - {_id} - ({index}) is NULL");
        return cell;
    }

    public Cell GetCell(Vector2Int index) {
        if (!cellGrid.TryGetValue(index, out Cell cell)) Debug.Log($"*ERROR* - {_id} - ({index}) is NULL");
        return cell;
    }

    public bool IsWithinGrid(Vector3 worldPosition) {
        return GetCell(worldPosition) != null;
    }
}
