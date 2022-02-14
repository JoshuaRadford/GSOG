using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using CodeMonkey;
using GridPathfindingSystem;

public class Testing : MonoBehaviour
{
    [SerializeField] private TilemapVisual tilemapVisual;
    private TilemapSystem tilemap;
    private TilemapSprite tilemapSprite;

    [SerializeField] private PathfindingMovement pathfindingMovement;
    private Pathfinding pathfinding;
    private TilemapSprite tms;

    private void Awake()
    {
        int mapWidth = 20;
        int mapHeight = 20;
        float cellSize = 10f;
        Vector3 origin = new Vector3(0, 0);

        tilemap = new TilemapSystem(mapWidth, mapHeight, cellSize, origin);
        tilemap.SetTilemapVisual(tilemapVisual);
        pathfinding = new Pathfinding(tilemap.GetGrid());
    }

    private void Start() 
    {
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();

        if (Input.GetKeyDown(KeyCode.Q)) tms = TilemapSprite.None;
        if (Input.GetKeyDown(KeyCode.W)) tms = TilemapSprite.Wall;
        if (Input.GetKeyDown(KeyCode.E)) tms = TilemapSprite.Mud;

        pathfinding.GetGrid().GetXY(pathfindingMovement.GetPosition(), out int cx, out int cy);
        //pathfinding.GetNode(cx, cy).SetIsWalkable(false);


        #region Testing Pathfinding
        if (Input.GetMouseButton(0))
        {
            // Map click location to grid position
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            // W

            List<PathNode> path = pathfinding.FindPath(cx, cy, x, y);
            if(path != null)
            {
                for(int i = 0; i < path.Count - 1; i++)
                {
                    // Draw the path to the hovered cell
                    float cellSize = pathfinding.GetGrid().GetCellSize();
                    Vector3 thisCellOrigin = path[i].parentGrid.GetWorldPosition(path[i].x, path[i].y);
                    Vector3 nextCellOrigin = path[i + 1].parentGrid.GetWorldPosition(path[i + 1].x, path[i + 1].y);
                    Debug.DrawLine(thisCellOrigin + Vector3.one * cellSize / 2, nextCellOrigin + Vector3.one * cellSize / 2, Color.green);
                }
            } 
        }
        if(Input.GetMouseButtonUp(0))
        {
            // Send transform of "characterPathfinding" to the target position clicked
            pathfindingMovement.SetTargetPosition(mouseWorldPosition);
        }

        if(Input.GetMouseButtonUp(1))
        {
            // Make the clicked cell unwalkable on the pathfinding grid
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            //pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
            pathfinding.GetNode(x, y).SetWeight(15);
            pathfinding.GetGrid().GetGridObject(mouseWorldPosition).SetTilemapSprite(tms);
            pathfinding.GetNode(x, y).SetIsWalkable(tms != TilemapSprite.Wall);
        }
        #endregion
    }
}
