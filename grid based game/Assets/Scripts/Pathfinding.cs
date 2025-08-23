using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private NodeGrid nodeGrid;

    private void Awake()
    {
        nodeGrid = GetComponent<NodeGrid>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        List<Node> openList = new();
        List<Node> closedSet = new();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }


            foreach (Node neighbor in nodeGrid.GetNeighbors(currentNode, startNode))
            {   
                int newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                neighbor.hCost = GetDistance(neighbor, targetNode);

                if (newGCost < neighbor.gCost || (!openList.Contains(neighbor) && !closedSet.Contains(neighbor)))
                {
                    neighbor.gCost = newGCost;
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }

                if (closedSet.Contains(neighbor) && newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.parent = currentNode;

                    closedSet.Remove(neighbor);
                    openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new();

        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int dX = Mathf.Abs(a.gridX - b.gridX);
        int dY = Mathf.Abs(a.gridY - b.gridY);

        if (dX > dY)
            return (14 * dY) + (10 * (dX - dY));

        return (14 * dX) + (10 * (dY - dX));
    }
}
