using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TilemapSprite
{
    None,
    Wall,
    Mud,
}
public class Tile
{
    // Core Data
    private int x;
    private int y;

    // External references
    private GridSystem<Tile> parentGrid;

    // Internal references
    private TilemapSprite tilemapSprite;

    // Functionalities
    private PathNode pathNode;

    public Tile(GridSystem<Tile> grid, int x, int y)
    {
        this.parentGrid = grid;
        this.x = x;
        this.y = y;
        pathNode = new PathNode(grid, x, y);
    }

    public TilemapSprite GetTilemapSprite()
    {
        return tilemapSprite;
    }

    public void SetTilemapSprite(TilemapSprite tilemapSprite)
    {
        this.tilemapSprite = tilemapSprite;
        parentGrid.TriggerGridObjectChanged(x, y);
    }

    public PathNode GetPathNode()
    {
        return pathNode;
    }

    public override string ToString()
    {
        return tilemapSprite.ToString();
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
        public TilemapSprite tilemapSprite;
        public int x;
        public int y;
        public PathNode.SaveObject pathNodeSaveObject;
    }

    public SaveObject Save()
    {
        return new SaveObject
        {
            tilemapSprite = tilemapSprite,
            x = x,
            y = y,
        };
    }

    public void Load(SaveObject saveObject)
    {
        tilemapSprite = saveObject.tilemapSprite;

        PathNode pathNode = new PathNode(parentGrid, x, y);
        pathNode.Load(saveObject.pathNodeSaveObject);
        this.pathNode = pathNode;
    }
}
