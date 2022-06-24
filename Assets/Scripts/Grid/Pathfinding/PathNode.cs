using UnityEngine;

public class PathNode {
    // Internal Data
    private int _x;
    private int _y;
    private int _weight;
    public int X { get => _x; set { if (_x != value) _x = value; } }
    public int Y { get => _y; set { if (_y != value) _y = value; } }
    public int Weight { get => _weight; set { if (_weight != value) _weight = value; } }

    // Dependent Data
    [SerializeField]
    private bool _walkable;
    public bool Walkable { get => _walkable; set { if (_walkable != value) _walkable = value; } }

    // Temp Data
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public PathNode cameFromNode;

    public PathNode(int x, int y, int weight = 0) {
        X = x;
        Y = y;
        Weight = weight;
        Walkable = true;
    }

    public Vector2Int GetXY() { return new Vector2Int(X, Y); }

    public override string ToString() {
        return $"({X},{Y}) : Weight ({Weight}) : Walkable ({Walkable})";
    }
}
