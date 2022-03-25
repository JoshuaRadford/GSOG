public class PathNode {
    // External Data
    public Cell parentCell;

    // Internal Data
    public int x;
    public int y;
    private int _weight;
    public int Weight { get => _weight; set { _weight = value; parentCell.ParentGrid.TriggerGridObjectChanged(x, y); } }

    // Dependent Data
    public bool isWalkable;

    // Temp Data
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public PathNode cameFromNode;

    public PathNode(Cell cell, int x, int y, int weight = 0) {
        this.parentCell = cell;
        this.x = x;
        this.y = y;
        this._weight = weight;
        this.isWalkable = true;
    }

    public bool SetIsWalkable(bool isWalkable) {
        this.isWalkable = isWalkable;
        parentCell.ParentGrid.TriggerGridObjectChanged(x, y);
        return this.isWalkable;
    }

    public override string ToString() {
        return x + "," + y;
    }
}
