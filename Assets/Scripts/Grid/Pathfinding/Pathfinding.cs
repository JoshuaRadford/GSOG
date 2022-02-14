using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 15;

    public static Pathfinding Instance { get; private set; }

    private GridSystem<Tile> grid;
    private List<PathNode> openList;
    private HashSet<PathNode> closedList;

    public Pathfinding(GridSystem<Tile> grid)
    {
        Instance = this;
        this.grid = grid;
    }

    public GridSystem<Tile> GetGrid()
    {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        // Translate world positions to grid coordinates
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);
        Debug.Log("(" + startX + "," + startY + ") - (" + endX + "," + endY + ")");
        List<PathNode> path = FindPath(startX, startY, endX, endY);

        if(path == null)
        {
            return null;
        }
        else
        {
            // Compile the world positions of the grid coordinates in our path
            List<Vector3> vectorPath = new List<Vector3>();
            foreach(PathNode pathNode in path)
            {
                vectorPath.Add(grid.GetOrigin() + new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        // Retrieve the PathNode of the start and end tiles
        PathNode startNode = grid.GetGridObject(startX, startY).GetPathNode();
        PathNode endNode = grid.GetGridObject(endX, endY).GetPathNode();

        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode>(); 
        
        // Initialize the heuristics of each Tile's PathNode in the grid
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y).GetPathNode();
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        // Repeat this process until we rule out every tile
        while (openList.Count > 0)
        {
            // Calculate the node with the lowest F cost
            PathNode currentNode = GetLowestFCostNode(openList);

            // Have we reached our destination?
            if(currentNode == endNode)
            {
                // Reached final node
                return CalculatePath(endNode);
            }

            // Remove this node from consideration
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Branch out to each adjecent Tile
            foreach(PathNode neighborNode in GetNeighborList(currentNode))
            {
                // Ignore anything we've already checked
                if (closedList.Contains(neighborNode)) continue;

                // Make sure Tile's PathNode is walkable
                if(!neighborNode.isWalkable)
                {
                    closedList.Add(neighborNode);
                    continue;
                }

                // Ignore pinched corners
                int cX = currentNode.x, cY = currentNode.y;
                int nX = neighborNode.x, nY = neighborNode.y;
                int xDir = nX - cX, yDir = nY - cY;
                if (!(grid.GetGridObject(cX + xDir, cY).GetPathNode().isWalkable) && !(grid.GetGridObject(cX, cY + yDir).GetPathNode().isWalkable)) continue;

                // Evaluate the G cost of this adjecent node compared to our current node
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode) + currentNode.GetWeight();
                if(tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                    neighborNode.CalculateFCost();
                    
                    // Add to open list if not in already
                    if(!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    // Retreive a list of all adjecent Tiles' PathNodes
    private List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        if(currentNode.x - 1 >= 0)
        {
            // Left
            neighborList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if(currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Left Up
            if(currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighborList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        // Down
        if(currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if(currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighborList;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y).GetPathNode();
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();

        path.Add(endNode);
        PathNode currentNode = endNode;

        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();

        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
