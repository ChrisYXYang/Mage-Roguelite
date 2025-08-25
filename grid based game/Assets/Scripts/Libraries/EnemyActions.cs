using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActions
{
    public static Vector2Int direction = Vector2Int.zero;

    public static void Move(int enemyIndex)
    {
        Enemy enemy = GameManager.enemyList[enemyIndex];

        Node pastNode = enemy.standingNode;
        enemy.standingNode = GameManager.nextNodeList[enemyIndex];
        enemy.transform.position = enemy.standingNode.worldPosition - (Vector2.up * 0.25f);
        enemy.ChangeDirection(new Vector2Int((int)(enemy.standingNode.worldPosition.x - pastNode.worldPosition.x), (int)(enemy.standingNode.worldPosition.y - pastNode.worldPosition.y)));

    }

    public static bool CanMove(int enemyIndex)
    {
        Enemy enemy = GameManager.enemyList[enemyIndex];

        foreach (Affliction affliction in enemy.afflictions)
        {
            if (affliction.affliction == AfflictionType.Drown || affliction.affliction == AfflictionType.Stun)
            {
                GameManager.nextNodeList.Add(enemy.standingNode);
                return false;
            }
        }

        if (CanAttack(enemyIndex))
        {
            GameManager.nextNodeList.Add(enemy.standingNode);
            return false;
        }

        List<Node> path = GameManager.pathfinding.FindPath(enemy.standingNode, GameManager.player.standingNode);

        if (path != null)
        {
            foreach (Node n in path)
                GameManager.path.Add(n);

            Node nextNode = path[0];
            GameManager.nextNodeList.Add(nextNode);
        }
        else
        {
            GameManager.nextNodeList.Add(enemy.standingNode);
        }

        if (GameManager.nextNodeList[enemyIndex] != GameManager.player.standingNode)
        {
            return true;
        }

        return false;
    }

    public static void Attack(int enemyIndex)
    {
        Enemy enemy = GameManager.enemyList[enemyIndex];
        Node consideredNode;

        switch (enemy.spell.spellType)
        {
            case (SpellType.Point):
                direction = new Vector2Int(GameManager.player.standingNode.gridX - enemy.standingNode.gridX, GameManager.player.standingNode.gridY - enemy.standingNode.gridY);
                enemy.ChangeDirection(direction);
                DamagePlayer(enemy.spell, direction);
                MonoBehaviour.Instantiate(enemy.spell.vfx, GameManager.player.standingNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                break;

            case (SpellType.Line):
                direction = Vector2Int.zero;

                if ((GameManager.player.standingNode.gridX - enemy.standingNode.gridX) != 0)
                    direction.x = (GameManager.player.standingNode.gridX - enemy.standingNode.gridX) / Mathf.Abs(GameManager.player.standingNode.gridX - enemy.standingNode.gridX);

                if ((GameManager.player.standingNode.gridY - enemy.standingNode.gridY) != 0)
                    direction.y = (GameManager.player.standingNode.gridY - enemy.standingNode.gridY) / Mathf.Abs(GameManager.player.standingNode.gridY - enemy.standingNode.gridY);

                enemy.ChangeDirection(direction);
                DamagePlayer(enemy.spell, direction);

                for (int i = 1; i < enemy.spell.range + 1; i++)
                {
                    if (!((enemy.standingNode.gridX + (direction.x * i)) >= 0 && (enemy.standingNode.gridX + (direction.x * i)) < GameManager.nodeGrid.grid.GetLength(0)
                        && (enemy.standingNode.gridY + (direction.y * i)) >= 0 && (enemy.standingNode.gridY + (direction.y * i)) < GameManager.nodeGrid.grid.GetLength(1))
                        || !GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i), enemy.standingNode.gridY + (direction.y * i)].walkable)
                    {
                        break;
                    }

                    consideredNode = GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i), enemy.standingNode.gridY + (direction.y * i)];
                    MonoBehaviour.Instantiate(enemy.spell.vfx, consideredNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                }
                break;

            case (SpellType.Radius):
                direction = Vector2Int.zero;
                DamagePlayer(enemy.spell, direction);

                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode || !GameManager.nodeGrid.grid[x, y].walkable)
                                continue;

                            MonoBehaviour.Instantiate(enemy.spell.vfx, GameManager.nodeGrid.grid[x, y].worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                        }
                    }
                }
                break;

            case (SpellType.Wave):
                DamagePlayer(enemy.spell, direction);
                enemy.ChangeDirection(direction);

                int startingIndex, startingIndexX = 0, startingIndexY = 0;

                if (enemy.spell.width % 2 != 0)
                    startingIndex = (int)(0 - (enemy.spell.width / 2f) + 0.5f);
                else
                    startingIndex = (int)(0 - (enemy.spell.width / 2f));

                if (direction.x != 0)
                    startingIndexY = startingIndex;
                else if (direction.y != 0)
                    startingIndexX = startingIndex;

                for (int k = startingIndexX; k < 0 - startingIndexX + 1; k++)
                {
                    for (int j = startingIndexY; j < 0 - startingIndexY + 1; j++)
                    {
                        if ((enemy.standingNode.gridX + direction.x + k) >= 0 && (enemy.standingNode.gridX + direction.x + k) < GameManager.nodeGrid.grid.GetLength(0))
                        {
                            if ((enemy.standingNode.gridY + direction.y + j) >= 0 && (enemy.standingNode.gridY + direction.y + j) < GameManager.nodeGrid.grid.GetLength(1))
                            {
                                List<Node> waveNodes = new();

                                for (int i = 1; i < enemy.spell.range + 1; i++)
                                {
                                    if (!((enemy.standingNode.gridX + (direction.x * i) + k) >= 0 && (enemy.standingNode.gridX + (direction.x * i) + k) < GameManager.nodeGrid.grid.GetLength(0)
                                        && (enemy.standingNode.gridY + (direction.y * i) + j) >= 0 && (enemy.standingNode.gridY + (direction.y * i) + j) < GameManager.nodeGrid.grid.GetLength(1))
                                        || !GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i) + k, enemy.standingNode.gridY + (direction.y * i) + j].walkable)
                                    {
                                        break;
                                    }

                                    consideredNode = GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i) + k, enemy.standingNode.gridY + (direction.y * i) + j];
                                    MonoBehaviour.Instantiate(enemy.spell.vfx, consideredNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                                }
                            }
                        }
                    }
                }
                break;

            case (SpellType.Cross):
                direction = Vector2Int.zero;

                if ((GameManager.player.standingNode.gridX - enemy.standingNode.gridX) != 0)
                    direction.x = (GameManager.player.standingNode.gridX - enemy.standingNode.gridX) / Mathf.Abs(GameManager.player.standingNode.gridX - enemy.standingNode.gridX);

                if ((GameManager.player.standingNode.gridY - enemy.standingNode.gridY) != 0)
                    direction.y = (GameManager.player.standingNode.gridY - enemy.standingNode.gridY) / Mathf.Abs(GameManager.player.standingNode.gridY - enemy.standingNode.gridY);
                
                DamagePlayer(enemy.spell, direction);

                CrossSideAttack(Vector2Int.up, enemy);
                CrossSideAttack(Vector2Int.down, enemy);
                CrossSideAttack(Vector2Int.left, enemy);
                CrossSideAttack(Vector2Int.right, enemy);
                break;

            case (SpellType.Horseshoe):
                Vector2Int difference = new Vector2Int(enemy.standingNode.gridX - GameManager.player.standingNode.gridX, enemy.standingNode.gridY - GameManager.player.standingNode.gridY);

                if (difference.x < 0)
                {
                    HorseshoeSideAttack(Vector2Int.right, enemy);
                    break;
                }
                if (difference.x > 0)
                {
                    HorseshoeSideAttack(Vector2Int.left, enemy);
                    break;
                }
                if (difference.y < 0)
                {
                    HorseshoeSideAttack(Vector2Int.up, enemy);
                    break;
                }
                if (difference.y > 0)
                {
                    HorseshoeSideAttack(Vector2Int.down, enemy);
                    break;
                }
                break;
        }

        if (enemy.enemyType == EnemyType.Bomb)
        {
            GameManager.enemiesToBeRemoved.Add(enemy);
            MonoBehaviour.Destroy(enemy.gameObject);
        }
    }

    public static bool CanAttack(int enemyIndex)
    {
        Enemy enemy = GameManager.enemyList[enemyIndex];

        foreach (Affliction affliction in enemy.afflictions)
        {
            if (affliction.affliction == AfflictionType.Stun)
            {
                return false;
            }
        }

        switch (enemy.spell.spellType)
        {
            case (SpellType.Point):
                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y += enemy.spell.range)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x += enemy.spell.range)
                    {
                        int pureX = x - enemy.standingNode.gridX;
                        int pureY = y - enemy.standingNode.gridY;

                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            
                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode || Mathf.Abs(pureX) + Mathf.Abs(pureY) > enemy.spell.range)
                                continue;

                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode)
                            {
                                return true;
                            }
                        }
                    }
                }
                break;

            case (SpellType.Line):
                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            int pureX = x - enemy.standingNode.gridX;
                            int pureY = y - enemy.standingNode.gridY;
                            
                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode || (Mathf.Abs(pureX) != 0 && Mathf.Abs(pureY) != 0))
                                continue;

                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode)
                            {
                                if (pureX != 0)
                                {
                                    for (int i = 1; i < Mathf.Abs(pureX); i++)
                                    {
                                        if (!GameManager.nodeGrid.grid[enemy.standingNode.gridX + ((pureX / Mathf.Abs(pureX) * i)), enemy.standingNode.gridY].walkable)
                                            return false;
                                    }

                                    return true;
                                }
                                else if (pureY != 0)
                                {
                                    for (int i = 1; i < Mathf.Abs(pureY); i++)
                                    {
                                        if (!GameManager.nodeGrid.grid[enemy.standingNode.gridX, enemy.standingNode.gridY + ((pureY / Mathf.Abs(pureY) * i))].walkable)
                                            return false;
                                    }

                                    return true;
                                }
                            }
                        }
                    }
                }
                break;

            case (SpellType.Wave):
                //up
                if (WaveSideCanAttack(Vector2Int.up, enemy))
                    return true;
                //down
                if (WaveSideCanAttack(Vector2Int.down, enemy))
                    return true;
                //left
                if (WaveSideCanAttack(Vector2Int.left, enemy))
                    return true;
                //right
                if (WaveSideCanAttack(Vector2Int.right, enemy))
                    return true;
                break;

            case (SpellType.Radius):
                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode)
                                continue;

                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode)
                            {
                                return true;
                            }
                        }
                    }
                }
                break;

            case (SpellType.Cross):
                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            int pureX = x - enemy.standingNode.gridX;
                            int pureY = y - enemy.standingNode.gridY;

                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode || (Mathf.Abs(pureX) != 0 && Mathf.Abs(pureY) != 0))
                                continue;

                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode)
                            {
                                if (pureX != 0)
                                {
                                    for (int i = 1; i < Mathf.Abs(pureX); i++)
                                    {
                                        if (!GameManager.nodeGrid.grid[enemy.standingNode.gridX + ((pureX / Mathf.Abs(pureX) * i)), enemy.standingNode.gridY].walkable)
                                            return false;
                                    }

                                    return true;
                                }
                                else if (pureY != 0)
                                {
                                    for (int i = 1; i < Mathf.Abs(pureY); i++)
                                    {
                                        if (!GameManager.nodeGrid.grid[enemy.standingNode.gridX, enemy.standingNode.gridY + ((pureY / Mathf.Abs(pureY) * i))].walkable)
                                            return false;
                                    }

                                    return true;
                                }
                            }
                        }
                    }
                }
                break;

            case (SpellType.Horseshoe):
                for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
                {
                    for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode)
                                continue;

                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode)
                            {
                                return true;
                            }
                        }
                    }
                }
                break;
        }

        return false;
    }

    public static bool WaveSideCanAttack(Vector2Int dir, Enemy enemy)
    {
        int startingIndex, startingIndexX = 0, startingIndexY = 0;

        if (enemy.spell.width % 2 != 0)
            startingIndex = (int)(0 - (enemy.spell.width / 2f) + 0.5f);
        else
            startingIndex = (int)(0 - (enemy.spell.width / 2f));

        if (dir.x != 0)
            startingIndexY = startingIndex;
        else if (dir.y != 0)
            startingIndexX = startingIndex;

        for (int k = startingIndexX; k < 0 - startingIndexX + 1; k++)
        {
            for (int j = startingIndexY; j < 0 - startingIndexY + 1; j++)
            {
                if ((enemy.standingNode.gridX + dir.x + k) >= 0 && (enemy.standingNode.gridX + dir.x + k) < GameManager.nodeGrid.grid.GetLength(0))
                {
                    if ((enemy.standingNode.gridY + dir.y + j) >= 0 && (enemy.standingNode.gridY + dir.y + j) < GameManager.nodeGrid.grid.GetLength(1))
                    {
                        List<Node> waveNodes = new();

                        for (int i = 1; i < enemy.spell.range + 1; i++)
                        {
                            Node consideredNode;

                            if (!((enemy.standingNode.gridX + (dir.x * i) + k) >= 0 && (enemy.standingNode.gridX + (dir.x * i) + k) < GameManager.nodeGrid.grid.GetLength(0)
                                && (enemy.standingNode.gridY + (dir.y * i) + j) >= 0 && (enemy.standingNode.gridY + (dir.y * i) + j) < GameManager.nodeGrid.grid.GetLength(1))
                                || !GameManager.nodeGrid.grid[enemy.standingNode.gridX + (dir.x * i) + k,enemy.standingNode.gridY + (dir.y * i) + j].walkable)
                            {
                                break;
                            }

                            consideredNode = GameManager.nodeGrid.grid[enemy.standingNode.gridX + (dir.x * i) + k, enemy.standingNode.gridY + (dir.y * i) + j];
                            
                            if (consideredNode == GameManager.player.standingNode)
                            {
                                direction = dir;
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public static void CrossSideAttack(Vector2Int direction, Enemy enemy)
    {
        for (int i = 1; i < enemy.spell.range + 1; i++)
        {
            if (!((enemy.standingNode.gridX + (direction.x * i)) >= 0 && (enemy.standingNode.gridX + (direction.x * i)) < GameManager.nodeGrid.grid.GetLength(0)
                && (enemy.standingNode.gridY + (direction.y * i)) >= 0 && (enemy.standingNode.gridY + (direction.y * i)) < GameManager.nodeGrid.grid.GetLength(1))
                || !GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i), enemy.standingNode.gridY + (direction.y * i)].walkable)
            {
                break;
            }

            Node consideredNode = GameManager.nodeGrid.grid[enemy.standingNode.gridX + (direction.x * i), enemy.standingNode.gridY + (direction.y * i)];
            MonoBehaviour.Instantiate(enemy.spell.vfx, consideredNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
        }
    }

    public static void HorseshoeSideAttack(Vector2Int direction, Enemy enemy)
    {
        DamagePlayer(enemy.spell, direction);
        enemy.ChangeDirection(direction);

        for (int y = -enemy.spell.range + enemy.standingNode.gridY; y <= enemy.spell.range + enemy.standingNode.gridY; y++)
        {
            for (int x = -enemy.spell.range + enemy.standingNode.gridX; x <= enemy.spell.range + enemy.standingNode.gridX; x++)
            {
                if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                {
                    if (GameManager.nodeGrid.grid[x, y] == enemy.standingNode || !GameManager.nodeGrid.grid[x, y].walkable)
                        continue;

                    if (direction == Vector2Int.up)
                    {
                        if (Mathf.Sign(y - enemy.standingNode.gridY) == -1)
                            continue;
                    }
                    else if (direction == Vector2Int.down)
                    {
                        if (Mathf.Sign(y - enemy.standingNode.gridY) == 1 && (y - enemy.standingNode.gridY != 0))
                            continue;
                    }
                    else if (direction == Vector2Int.left && (x - enemy.standingNode.gridX != 0))
                    {
                        if (Mathf.Sign(x - enemy.standingNode.gridX) == 1)
                            continue;
                    }
                    else if (direction == Vector2Int.right)
                    {
                        if (Mathf.Sign(x - enemy.standingNode.gridX) == -1)
                            continue;
                    }

                    MonoBehaviour.Instantiate(enemy.spell.vfx, GameManager.nodeGrid.grid[x, y].worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                }
            }
        }
    }

    public static void DamagePlayer(SpellItem spell, Vector2Int direction)
    {
        if (spell.affliction.affliction != AfflictionType.None)
        {
            GameManager.player.AddAffliction(spell);
        }

        GameManager.enemyDamage += spell.damage;

        if (spell.knockback > 0)
        {
            Node consideredNode;

            for (int i = 1; i < spell.knockback + 1; i++)
            {
                bool outOfBounds = false;
                bool blocked = false;

                if (!(GameManager.player.standingNode.gridX + (direction.x * i) >= 0 && GameManager.player.standingNode.gridX + (direction.x * i) < GameManager.nodeGrid.grid.GetLength(0)
                    && GameManager.player.standingNode.gridY + (direction.y * i) >= 0 && GameManager.player.standingNode.gridY + (direction.y * i) < GameManager.nodeGrid.grid.GetLength(1)))
                {
                    outOfBounds = true;
                    blocked = true;
                }

                if (!outOfBounds)
                {
                    consideredNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * i), GameManager.player.standingNode.gridY + (direction.y * i)];

                    if (!consideredNode.walkable)
                        blocked = true;

                    if (!blocked)
                    {
                        foreach (Enemy blockingEnemies in GameManager.enemyList)
                        {
                            if (blockingEnemies.standingNode == consideredNode)
                            {
                                blocked = true;
                                break;
                            }
                        }
                    }
                }

                if (blocked)
                {
                    GameManager.enemyDamage += IngameChar.collisionDamage;
                    GameManager.player.standingNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * (i - 1)), GameManager.player.standingNode.gridY + (direction.y * (i - 1))];
                    GameManager.player.transform.position = GameManager.player.standingNode.worldPosition - (Vector2.up * 0.25f);
                    return;
                }
            }

            GameManager.player.standingNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * spell.knockback), GameManager.player.standingNode.gridY + (direction.y * spell.knockback)];
            GameManager.player.transform.position = GameManager.player.standingNode.worldPosition - (Vector2.up * 0.25f);
        }
    }
}