using System;
using UnityEngine;

public class GridSystem<TGridObject> {
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;
    public TGridObject[,] GridArray => gridArray;


    public GridSystem(int width, int height, float cellSize, Vector3 originPosition, Func<GridSystem<TGridObject>, int, int, TGridObject> createGridObject = null, bool center = false) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        if (center) this.originPosition = CenterGrid(width, height, cellSize, this.originPosition);

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                if (createGridObject != null) {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }
        }
    }
    public int Width => width;

    public int Height => height;

    public float CellSize => cellSize;

    public Vector3 GetOrigin() {
        return originPosition;
    }

    public Vector3 GetWorldPosition(Vector2Int index) {
        return new Vector3(index.x, index.y) * cellSize + originPosition;
    }

    public Vector2Int GetXY(Vector3 worldPosition, bool clamp = true) {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);

        if (clamp) {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
        }

        return new Vector2Int(x, y);
    }

    public bool IsWithinGrid(Vector3 worldPosition) {
        Vector2Int xy = GetXY(worldPosition, false);
        return (xy.x >= 0 && xy.x < width && xy.y >= 0 && xy.y < height);
    }

    public void SetGridObject(int x, int y, TGridObject value) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value) {
        Vector2Int xy = GetXY(worldPosition);
        SetGridObject(xy.x, xy.y, value);
    }

    public TGridObject GetGridObject(int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            return gridArray[x, y];
        }
        else {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector2Int xy) {
        return GetGridObject(xy.x, xy.y);
    }

    public TGridObject GetGridObject(Vector3 worldPosition) {
        Vector2Int xy = GetXY(worldPosition);
        return GetGridObject(xy.x, xy.y);
    }

    public static Vector3 CenterGrid(int width, int height, float cellSize, Vector3 originPosition) {
        return new Vector3(originPosition.x - width * cellSize / 2, originPosition.y - height * cellSize / 2, originPosition.z);
    }

    public void DrawGrid(bool showDebug) {
        if (showDebug) {
            TextMesh[,] debugTextArray = new TextMesh[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    //debugTextArray[x, y] = UtilsClass.CreateWorldText(x + ", " + y, null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, (int)cellSize, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(new Vector2Int(x, y)), GetWorldPosition(new Vector2Int(x, y + 1)), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(new Vector2Int(x, y)), GetWorldPosition(new Vector2Int(x + 1, y)), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(new Vector2Int(0, height)), GetWorldPosition(new Vector2Int(width, height)), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(new Vector2Int(width, 0)), GetWorldPosition(new Vector2Int(width, height)), Color.white, 100f);
        }
    }
}
