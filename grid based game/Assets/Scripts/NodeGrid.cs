using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleMask;
    public Vector2 worldSize;
    [SerializeField] private float nodeRadius;

    private int gridSizeX, gridSizeY;
    [HideInInspector] public Node[,] grid;

    public void CreateGrid()
    {
        gridSizeX = (int)worldSize.x;
        gridSizeY = (int)worldSize.y;
        
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = Vector2.zero - (Vector2.right * worldSize.x / 2) - (Vector2.up * worldSize.y / 2) + (Vector2.one * nodeRadius);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 gridPoint = worldBottomLeft + (Vector2.right * x) + (Vector2.up * y);
                bool walkable = !(Physics2D.OverlapCircle(gridPoint, nodeRadius * 0.9f));
                grid[x, y] = new Node(walkable, gridPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighbors(Node node, Node standingNode)
    {
        List<Node> neighbors = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    Node neighbor = grid[checkX, checkY];

                    bool inEnemy = false;
                    foreach (Enemy otherEnemy in GameManager.enemyList)
                    {
                        if (otherEnemy.standingNode == neighbor)
                        {
                            inEnemy = true;
                            break;
                        }
                    }

                    if (!neighbor.walkable || Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                        continue;

                    if (((GameManager.nextNodeList.Contains(neighbor) && neighbor != GameManager.player.standingNode) || inEnemy) && InRange(standingNode, neighbor))
                        continue;
                    
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    public Node NodeFromWorldPoint(Vector2 position)
    {
        return grid[(int)(position.x + (worldSize.x / 2)), (int)(position.y + (worldSize.y / 2))];
    }

    private bool InRange(Node node, Node neighbor)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (grid[checkX, checkY] == neighbor)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector2.zero, worldSize);

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.gray : Color.red; 
                Gizmos.DrawCube(n.worldPosition, Vector2.one * 0.75f);

                if (GameManager.path != null)
                {
                    if (GameManager.path.Contains(n))
                        Gizmos.color = Color.blue;
                }

                if (GameManager.player.gameObject.GetComponent<Player>().standingNode == n)
                    Gizmos.color = Color.green;

                Gizmos.DrawCube(n.worldPosition, Vector2.one * 0.9f);
            }
        }
    }
}
