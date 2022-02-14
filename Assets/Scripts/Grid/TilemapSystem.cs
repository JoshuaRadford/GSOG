using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapSystem
{
    public event EventHandler OnLoaded;
    private GridSystem<Tile> grid;

    public TilemapSystem(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new GridSystem<Tile>(width, height, cellSize, originPosition, (GridSystem<Tile> g, int x, int y) => new Tile(g, x, y), true);
    }

    public GridSystem<Tile> GetGrid()
    {
        return grid;
    }

    public void SetTilemapSprite(Vector3 worldPosition, TilemapSprite tilemapSprite)
    {
        Tile tile = grid.GetGridObject(worldPosition);
        if(tile != null)
        {
            tile.SetTilemapSprite(tilemapSprite);
        }
    }

    public void SetTilemapSprite(int x, int y, TilemapSprite tilemapSprite)
    {
        Tile tile = grid.GetGridObject(x, y);
        if (tile != null)
        {
            tile.SetTilemapSprite(tilemapSprite);
        }
    }

    public void SetAllTilemapSprite(TilemapSprite tilemapSprite)
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                SetTilemapSprite(x, y, tilemapSprite);
            }
        }
    }

    public void SetTilemapVisual(TilemapVisual tilemapVisual)
    {
        tilemapVisual.SetGrid(this, grid);
    }

    /*
     * Save - Load
     */
    /*
     * Details:
     * Can be made more robust by adding additional fieds.
     * Could support width and height for saving and loading different size tilemaps.
     */
    public class SaveObject
    {
        public Tile.SaveObject[] tilemapObjectSaveObjectArray;
    }

    public void Save()
    {
        List<Tile.SaveObject> tilemapObjectSaveObjectList = new List<Tile.SaveObject>();
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                Tile tile = grid.GetGridObject(x, y);
                tilemapObjectSaveObjectList.Add(tile.Save());
            }
        }

        SaveObject saveObject = new SaveObject { tilemapObjectSaveObjectArray = tilemapObjectSaveObjectList.ToArray() };

        SaveSystem.SaveObject(saveObject);
    }

    public void Load()
    {
        SaveObject saveObject = SaveSystem.LoadMostRecentObject<SaveObject>();
        foreach(Tile.SaveObject tilemapObjectSaveObject in saveObject.tilemapObjectSaveObjectArray)
        {
            Tile tile = grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.y);
            tile.Load(tilemapObjectSaveObject);
        }
        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

    /*
     * Represents a single Tilemap Object that exists in each Grid Cell Position
     */
    
}
