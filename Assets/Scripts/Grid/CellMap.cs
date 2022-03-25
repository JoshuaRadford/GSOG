using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellMap {
    public event EventHandler OnLoaded;
    private GridSystem<Cell> grid;

    public CellMap(int width, int height, float cellSize, Vector3 originPosition) {
        grid = new GridSystem<Cell>(width, height, cellSize, originPosition, (GridSystem<Cell> g, int x, int y) => new Cell(g, x, y), false);
    }

    public GridSystem<Cell> GetGrid() {
        return grid;
    }

    public void SetCellTile(Vector3 worldPosition, Tile tile) {
        Cell cell = grid.GetGridObject(worldPosition);
        if (cell != null) {
            cell.ToSetTile = tile;
        }
    }

    public void SetCellTile(Vector2Int index, Tile tile) {
        Cell cell = grid.GetGridObject(index.x, index.y);
        if (cell != null) {
            cell.ToSetTile = tile;
        }
    }

    public void SetAllCellTile(Tile tile) {
        for (int x = 0; x < grid.Width; x++) {
            for (int y = 0; y < grid.Height; y++) {
                SetCellTile(new Vector2Int(x, y), tile);
            }
        }
    }

    public void SetCellColor(Vector3 worldPosition, Color color) {
        Cell cell = grid.GetGridObject(worldPosition);
        if (cell != null) {
            cell.ToSetColor = color;
        }
    }

    public void SetCellColor(Vector2Int index, Color color) {
        Cell cell = grid.GetGridObject(index.x, index.y);
        if (cell != null) {
            cell.ToSetColor = color;
        }
    }

    public void SetAllCellColor(Color color) {
        for (int x = 0; x < grid.Width; x++) {
            for (int y = 0; y < grid.Height; y++) {
                SetCellColor(new Vector2Int(x, y), color);
            }
        }
    }

    public Vector2Int GetRandomWalkableCellIndex() {
        List<Vector2Int> cellPositions = new List<Vector2Int>();
        for (int x = 0; x < grid.Width; x++) {
            for (int y = 0; y < grid.Height; y++) {
                if (grid.GetGridObject(x, y).PathNode.isWalkable) cellPositions.Add(new Vector2Int(x, y));
            }
        }
        int randIndex = UnityEngine.Random.Range(0, cellPositions.Count);
        Vector2Int randCellPos = cellPositions[randIndex];

        return randCellPos;
    }

    /*
     * Save - Load
     */
    /*
     * Details:
     * Can be made more robust by adding additional fieds.
     * Could support width and height for saving and loading different size tilemaps.
     */
    public class SaveObject {
        public Cell.SaveObject[] tilemapObjectSaveObjectArray;
    }

    public void Save() {
        List<Cell.SaveObject> tilemapObjectSaveObjectList = new List<Cell.SaveObject>();
        for (int x = 0; x < grid.Width; x++) {
            for (int y = 0; y < grid.Height; y++) {
                Cell cell = grid.GetGridObject(x, y);
                tilemapObjectSaveObjectList.Add(cell.Save());
            }
        }

        SaveObject saveObject = new SaveObject { tilemapObjectSaveObjectArray = tilemapObjectSaveObjectList.ToArray() };

        SaveSystem.SaveObject(saveObject);
    }

    public void Load() {
        SaveObject saveObject = SaveSystem.LoadMostRecentObject<SaveObject>();
        foreach (Cell.SaveObject tilemapObjectSaveObject in saveObject.tilemapObjectSaveObjectArray) {
            Cell cell = grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.y);
            cell.Load(tilemapObjectSaveObject);
        }
        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

    /*
     * Represents a single Tilemap Object that exists in each Grid Cell Position
     */

}
