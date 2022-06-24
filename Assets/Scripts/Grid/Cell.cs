using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Cell {
    // Core Data
    private int _x;
    private int _y;
    public int X { get => _x; set { if (_x != value) _x = value; } }
    public int Y { get => _y; set { if (_y != value) _y = value; } }

    // External references
    public CellGrid parentGrid;

    // Internal references
    private TileBase _tile;
    private Color _color;
    private bool _containsActor;
    public TileBase Tile { get => _tile; set { if (value != _tile) _tile = value; } }
    public Color Color { get => _color; set { if (value != _color) _color = value; } }
    public bool ContainsActor { get => _containsActor; set { if (_containsActor != value) _containsActor = value; } }

    // Functionalities
    private PathNode _pathNode;
    public PathNode PathNode { get => _pathNode; set { if (_pathNode != value) _pathNode = value; } }

    public Cell(CellGrid grid, int x, int y) {
        parentGrid = grid;
        X = x;
        Y = y;
        PathNode = new PathNode(x, y);
        ContainsActor = false;
    }

    public Vector2Int GetXY() { return new Vector2Int(X, Y); }

    public Vector3 ToWorld() {
        Vector3 cellOffset = new Vector3(parentGrid.CellSize, parentGrid.CellSize) / 2;
        return parentGrid.GetWorldPosition(GetXY()) + cellOffset;
    }

    public Cell Right {
        get {
            return parentGrid.GetCell(new Vector2Int(X+1, Y));
            }
    }
    public Cell Left {
        get {
            return parentGrid.GetCell(new Vector2Int(X-1, Y));
        }
    }
    public Cell Up {
        get {
            return parentGrid.GetCell(new Vector2Int(X, Y+1));
        }
    }
    public Cell Down {
        get {
            return parentGrid.GetCell(new Vector2Int(X, Y-1));
        }
    }

    public override string ToString() {
        return _tile?.ToString();
    }
}
