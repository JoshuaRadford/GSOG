using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : Singleton<Pathfinding> {
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static PathNode GetNode(int x, int y, GridSystem<Cell> grid) {
        return grid.GetGridObject(x, y).PathNode;
    }

    // Retreive a list of all adjecent cells' PathNodes
    public static List<PathNode> GetNeighborList(PathNode currentNode, GridSystem<Cell> grid) {
        List<PathNode> neighborList = new List<PathNode>();

        if (currentNode.x - 1 >= 0) {
            // Left
            neighborList.Add(GetNode(currentNode.x - 1, currentNode.y, grid));
            // Left Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1, grid));
            // Left Up
            if (currentNode.y + 1 < grid.Height) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1, grid));
        }
        if (currentNode.x + 1 < grid.Width) {
            // Right
            neighborList.Add(GetNode(currentNode.x + 1, currentNode.y, grid));
            // Right Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1, grid));
            // Right Up
            if (currentNode.y + 1 < grid.Height) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1, grid));
        }
        // Down
        if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1, grid));
        // Up
        if (currentNode.y + 1 < grid.Height) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1, grid));

        return neighborList;
    }

    public static HashSet<PathNode> GetWalkablesByCost(PathNode origin, int cost, bool ignoreOrigin, GridSystem<Cell> grid) {
        Dictionary<PathNode, int> nodeCosts = new Dictionary<PathNode, int>();
        Queue<PathNode> queue = new Queue<PathNode>();

        if (ignoreOrigin || origin.isWalkable) queue.Enqueue(origin);
        nodeCosts[origin] = 0;

        while (queue.Count > 0) {
            PathNode currentNode = queue.Dequeue();
            foreach (PathNode neighbor in GetNeighborList(currentNode, grid)) {
                // Ignore pinched corners
                int cX = currentNode.x, cY = currentNode.y;
                int nX = neighbor.x, nY = neighbor.y;
                int xDir = nX - cX, yDir = nY - cY;
                if (!(grid.GetGridObject(cX + xDir, cY).PathNode.isWalkable) && !(grid.GetGridObject(cX, cY + yDir).PathNode.isWalkable)) continue;

                // Get cost to move from currentNode to this neighbor
                int dist = CalculateDistanceCost(currentNode, neighbor);

                // If ((we haven't checked) or (we found a cheaper route)) and (this route cost is within range) and (this neighbor is walkable)
                if ((!nodeCosts.ContainsKey(neighbor) || nodeCosts[neighbor] > nodeCosts[currentNode] + dist) && nodeCosts[currentNode] + dist <= cost && neighbor.isWalkable) {
                    nodeCosts[neighbor] = nodeCosts[currentNode] + dist;
                    queue.Enqueue(neighbor);
                }
            }
        }
        nodeCosts.Remove(origin);

        // Transfer all valid PathNodes to a simpler HashSet
        HashSet<PathNode> validMoves = new HashSet<PathNode>();
        foreach (KeyValuePair<PathNode, int> pair in nodeCosts) validMoves.Add(pair.Key);

        return validMoves;
    }

    public static Vector3 GetClosestValidWorldPosition(GridSystem<Cell> grid, Vector3 worldPos) {
        return new Vector3
            (
            Mathf.Clamp(worldPos.x, grid.GetOrigin().x - grid.Width * grid.CellSize / 2, grid.GetOrigin().x + grid.Width * grid.CellSize / 2),
            Mathf.Clamp(worldPos.y, grid.GetOrigin().y - grid.Height * grid.CellSize / 2, grid.GetOrigin().y - grid.Height * grid.CellSize / 2)
            );
    }

    public static List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, GridSystem<Cell> grid, IEnumerable<Cell> ignoreWalkable = null) {
        // Translate world positions to grid coordinates
        Vector2Int start = grid.GetXY(startWorldPosition);
        Vector2Int end = grid.GetXY(endWorldPosition);
        //Debug.Log("(" + startX + "," + startY + ") - (" + endX + "," + endY + ")");
        List<PathNode> path = FindPath(start, end, grid, ignoreWalkable);

        if (path == null) {
            return null;
        }
        else {
            // Compile the world positions of the grid coordinates in our path
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(grid.GetOrigin() + new Vector3(pathNode.x, pathNode.y) * grid.CellSize + Vector3.one * grid.CellSize * 0.5f);
            }
            return vectorPath;
        }
    }

    public static List<PathNode> FindPath(Vector2Int start, Vector2Int end, GridSystem<Cell> grid, IEnumerable<Cell> ignoreWalkable = null) {
        if (ignoreWalkable == null) ignoreWalkable = new HashSet<Cell>();

        bool IsWalkable(PathNode node) {
            return node.isWalkable || ignoreWalkable.Contains(node.parentCell);
        }

        List<PathNode> openList;
        HashSet<PathNode> closedList;
        // Retrieve the PathNode of the start and end cells
        PathNode startNode = grid.GetGridObject(start.x, start.y).PathNode;
        PathNode endNode = grid.GetGridObject(end.x, end.y).PathNode;

        // End process if our target is not valid to begin with
        if (!IsWalkable(endNode)) return null;

        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode>();

        // Initialize the heuristics of each cell's PathNode in the grid
        for (int x = 0; x < grid.Width; x++) {
            for (int y = 0; y < grid.Height; y++) {
                PathNode pathNode = grid.GetGridObject(x, y).PathNode;
                pathNode.gCost = int.MaxValue;
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);

        // Repeat this process until we rule out every cell
        while (openList.Count > 0) {
            // Calculate the node with the lowest F cost
            PathNode currentNode = GetLowestFCostNode(openList);

            // Have we reached our destination?
            if (currentNode == endNode) {
                // Reached final node
                return CalculatePath(endNode);
            }

            // Remove this node from consideration
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Branch out to each adjecent cell
            foreach (PathNode neighborNode in GetNeighborList(currentNode, grid)) {
                // Ignore anything we've already checked
                if (closedList.Contains(neighborNode)) continue;

                // Make sure cell's PathNode is walkable
                if (!IsWalkable(neighborNode)) {
                    closedList.Add(neighborNode);
                    continue;
                }

                // Ignore pinched corners
                int cX = currentNode.x, cY = currentNode.y;
                int nX = neighborNode.x, nY = neighborNode.y;
                int xDir = nX - cX, yDir = nY - cY;
                if (!(IsWalkable(grid.GetGridObject(cX + xDir, cY).PathNode)) && !(IsWalkable(grid.GetGridObject(cX, cY + yDir).PathNode))) continue;

                // Evaluate the G cost of this adjecent node compared to our current node
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode) + currentNode.Weight;
                if (tentativeGCost < neighborNode.gCost) {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);

                    // Add to open list if not in already
                    if (!openList.Contains(neighborNode)) {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    public static List<Cell> FindPath(Cell start, Cell end, GridSystem<Cell> grid, IEnumerable<Cell> ignoreWalkable = null) {
        List<PathNode> path = FindPath(start.GetXY, end.GetXY, grid, ignoreWalkable);
        List<Cell> cellPath = new List<Cell>();
        foreach (PathNode pathNode in path) cellPath.Add(pathNode.parentCell);

        return cellPath;
    }

    private static List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();

        path.Add(endNode);
        PathNode currentNode = endNode;

        while (currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();

        return path;
    }

    public static int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private static PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}