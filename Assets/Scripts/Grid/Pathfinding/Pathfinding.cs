using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : Singleton<Pathfinding> {
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static PathNode GetNode(Vector2Int index, CellGrid grid) {
        return grid.GetCell(index).PathNode;
    }

    // Retreive a list of all adjecent cells' PathNodes
    public static List<PathNode> GetNeighborList(Vector2Int index, CellGrid grid) {
        List<PathNode> neighborList = new List<PathNode>();
        Cell cell = grid.GetCell(index);

        if (cell.Left != null) {
            // Left
            neighborList.Add(cell.Left.PathNode);
            // Left Down
            if (cell.Left.Down != null) neighborList.Add(cell.Left.Down.PathNode);
            // Left Up
            if (cell.Left.Up != null) neighborList.Add(cell.Left.Up.PathNode);
        }
        if (cell.Right != null) {
            // Right
            neighborList.Add(cell.Right.PathNode);
            // Right Down
            if (cell.Right.Down != null) neighborList.Add(cell.Right.Down.PathNode);
            // Right Up
            if (cell.Right.Up != null) neighborList.Add(cell.Right.Up.PathNode);
        }
        // Down
        if (cell.Down != null) neighborList.Add(cell.Down.PathNode);
        // Up
        if (cell.Up != null) neighborList.Add(cell.Up.PathNode);

        return neighborList;
    }

    public static HashSet<Vector2Int> GetMovesWithinCost(Vector2Int originIndex, int cost, bool ignoreOrigin, CellGrid grid) {
        Dictionary<PathNode, int> nodeCosts = new Dictionary<PathNode, int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        PathNode origin = grid.GetCell(originIndex).PathNode;

        if (ignoreOrigin || origin.Walkable) queue.Enqueue(originIndex);
        nodeCosts[origin] = 0;

        while (queue.Count > 0) {
            PathNode currentNode = grid.GetCell(queue.Dequeue()).PathNode;
            foreach (PathNode neighbor in GetNeighborList(currentNode.GetXY(), grid)) {
                // Ignore pinched corners
                int cX = currentNode.X, cY = currentNode.Y;
                int nX = neighbor.X, nY = neighbor.Y;
                int xDir = nX - cX, yDir = nY - cY;
                if (!(grid.GetCell(new Vector2Int(cX + xDir, cY)).PathNode.Walkable) && !(grid.GetCell(new Vector2Int(cX, cY + yDir)).PathNode.Walkable)) continue;

                // Get cost to move from currentNode to this neighbor
                int dist = CalculateDistanceCost(currentNode, neighbor);

                // If ((we haven't checked) or (we found a cheaper route)) and (this route cost is within range) and (this neighbor is walkable)
                if ((!nodeCosts.ContainsKey(neighbor) || nodeCosts[neighbor] > nodeCosts[currentNode] + dist) && nodeCosts[currentNode] + dist <= cost && neighbor.Walkable) {
                    nodeCosts[neighbor] = nodeCosts[currentNode] + dist;
                    queue.Enqueue(neighbor.GetXY());
                }
            }
        }
        nodeCosts.Remove(origin);

        IEnumerable<Vector2Int> IECosts = nodeCosts.Select(keyValuePair => keyValuePair.Key.GetXY());
        HashSet<Vector2Int> hashCosts = new HashSet<Vector2Int>(IECosts);

        return hashCosts;
    }

    public static List<PathNode> FindPath(Vector2Int start, Vector2Int end, CellGrid grid, IEnumerable<Vector2Int> ignoreWalkable = null) {
        if (ignoreWalkable == null) ignoreWalkable = new HashSet<Vector2Int>();
        if(grid == null || grid.GetCell(end) == null || grid.GetCell(start) == null) return null;

        bool Walkable(PathNode node) {
            return node != null && (node.Walkable || ignoreWalkable.Contains(node.GetXY()));
        }

        List<PathNode> openList;
        HashSet<PathNode> closedList;
        // Retrieve the PathNode of the start and end cells
        PathNode startNode = grid.GetCell(start).PathNode;
        PathNode endNode = grid.GetCell(end).PathNode;

        // End process if our target is not valid to begin with
        if (!Walkable(endNode)) return null;

        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode>();

        // Initialize the heuristics of each cell's PathNode in the grid
        foreach(KeyValuePair<Vector2Int, Cell> pair in grid.Grid) {
            PathNode pathNode = pair.Value.PathNode;
            pathNode.gCost = int.MaxValue;
            pathNode.cameFromNode = null;
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
            foreach (PathNode neighborNode in GetNeighborList(currentNode.GetXY(), grid)) {
                // Ignore anything we've already checked
                if (closedList.Contains(neighborNode)) continue;

                // Make sure cell's PathNode is walkable
                if (!Walkable(neighborNode)) {
                    closedList.Add(neighborNode);
                    continue;
                }

                // Ignore pinched corners
                int cX = currentNode.X, cY = currentNode.Y;
                int nX = neighborNode.X, nY = neighborNode.Y;
                int xDir = nX - cX, yDir = nY - cY;
                if (!Walkable(grid.GetCell(new Vector2Int(cX + xDir, cY)).PathNode) && !Walkable(grid.GetCell(new Vector2Int(cX, cY + yDir)).PathNode)) continue;

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

    public static List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, CellGrid grid, IEnumerable<Vector2Int> ignoreWalkable = null) {
        // Translate world positions to grid coordinates
        Vector2Int start = grid.GetIndex(startWorldPosition);
        Vector2Int end = grid.GetIndex(endWorldPosition);
        List<PathNode> path = FindPath(start, end, grid, ignoreWalkable);

        if (path == null) return null;

        // Compile the world positions of the grid coordinates in our path
        List<Vector3> vectorPath = new List<Vector3>();
        foreach (PathNode node in path) {
            vectorPath.Add(grid.Origin + grid.GetWorldPosition(node.GetXY()) * grid.CellSize * grid.CellSize * 0.5f);
        }
        return vectorPath;
    }

    // Mask path with a set of valid nodes
    public static List<Vector2Int> FilterPath(List<Vector2Int> path, HashSet<Vector2Int> valids, CellGrid grid) {
        List<Vector2Int> longestValidPath = new List<Vector2Int>();

        if (path != null && valids != null && path.Count >= 1) {
            for (int i = 0; i < path.Count - 1; i++) {
                if (valids.Contains(path[i + 1])) {
                    longestValidPath.Add(path[i + 1]);
                }
            }
        }

        return longestValidPath;
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
        int xDistance = Mathf.Abs(a.X - b.X);
        int yDistance = Mathf.Abs(a.Y - b.Y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    public static int CalculateDistanceCost(Vector2Int a, Vector2Int b, CellGrid grid) {
        return CalculateDistanceCost(grid.GetCell(a).PathNode, grid.GetCell(b).PathNode);
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