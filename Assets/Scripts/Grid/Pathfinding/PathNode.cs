using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public GridSystem<Tile> parentGrid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    private int weight;
    public bool isWalkable;
    public PathNode cameFromNode;

    public PathNode(GridSystem<Tile> grid, int x, int y, int weight = 0)
    {
        this.parentGrid = grid;
        this.x = x;
        this.y = y;
        this.weight = weight;
        this.isWalkable = true;
    }

    public int CalculateFCost()
    {
        return fCost = gCost + hCost;
    }

    public int GetWeight()
    {
        return weight;
    }

    public void SetWeight(int weight)
    {
        this.weight = weight;
        parentGrid.TriggerGridObjectChanged(x, y);
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable=isWalkable;
        parentGrid.TriggerGridObjectChanged(x, y);
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    /*
     * Save - Load
     */
    /*
     * Details:
     * Fields included here can be any features you want to save within each object.
     */
    [System.Serializable]
    public class SaveObject
    {
        public int weight;
        public bool isWalkable;
    }

    public SaveObject Save()
    {
        return new SaveObject
        {
            weight = weight,
            isWalkable = isWalkable,
        };
    }

    public void Load(SaveObject saveObject)
    {
        weight = saveObject.weight;
        isWalkable=saveObject.isWalkable;
    }
}
