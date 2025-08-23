using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions
{
    public static void Move(Vector2Int direction)
    {
        foreach (Affliction affliction in GameManager.player.afflictions)
        {
            if (affliction.affliction == AfflictionType.Drown)
            {
                return;
            }
        }

        GameManager.player.standingNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + direction.x, GameManager.player.standingNode.gridY + direction.y];
        GameManager.player.gameObject.transform.position = GameManager.player.standingNode.worldPosition - (Vector2.up * 0.25f);

        GameManager.player.ChangeDirection(direction);
    }
    
    public static bool CanMove(Vector2Int direction)
    {
        if (GameManager.player.standingNode.gridX + direction.x == 9 || GameManager.player.standingNode.gridX + direction.x == 10)
        {
            if (GameManager.player.standingNode.gridY + direction.y == 10)
            {
                MapManager.mapManager.SwitchRoom(Vector2Int.up);
            }

            if (GameManager.player.standingNode.gridY + direction.y == -1)
            {
                MapManager.mapManager.SwitchRoom(Vector2Int.down);
            }
        }

        if (GameManager.player.standingNode.gridY + direction.y == 4 || GameManager.player.standingNode.gridY + direction.y == 5)
        {
            if (GameManager.player.standingNode.gridX + direction.x == 20)
            {
                MapManager.mapManager.SwitchRoom(Vector2Int.right);
            }

            if (GameManager.player.standingNode.gridX + direction.x == -1)
            {
                MapManager.mapManager.SwitchRoom(Vector2Int.left);
            }
        }

        if ((GameManager.player.standingNode.gridX + direction.x) >= 0 && (GameManager.player.standingNode.gridX + direction.x) < GameManager.nodeGrid.grid.GetLength(0))
        {
            if ((GameManager.player.standingNode.gridY + direction.y) >= 0 && (GameManager.player.standingNode.gridY + direction.y) < GameManager.nodeGrid.grid.GetLength(1))
            {
                if (GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + direction.x, GameManager.player.standingNode.gridY + direction.y].walkable)
                {
                    bool moveOnEnemy = false;

                    foreach (Enemy enemy in GameManager.enemyList)
                    {
                        if (GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + direction.x, GameManager.player.standingNode.gridY + direction.y] == enemy.standingNode)
                        {
                            moveOnEnemy = true;
                            break;
                        }
                    }

                    if (!moveOnEnemy)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static void Attack(SpellItem spell, Vector2Int direction)
    {
        switch (spell.spellType)
        {
            case (SpellType.Point):
                Enemy targetEnemy = null;

                foreach (Enemy enemy in GameManager.enemyList)
                {
                    if (GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * spell.range), GameManager.player.standingNode.gridY + (direction.y * spell.range)] == enemy.standingNode)
                    {
                        targetEnemy = enemy;
                        break;
                    }
                }

                if (targetEnemy != null)
                {
                    DamageEnemy(targetEnemy, spell, direction);
                }

                MonoBehaviour.Instantiate(spell.vfx, GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * spell.range), GameManager.player.standingNode.gridY + (direction.y * spell.range)].worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                break;

            case (SpellType.Line):
                LineAttack(direction, spell);
                break;

            case (SpellType.Wave):
                int startingIndex, startingIndexX = 0, startingIndexY = 0;

                if (spell.width % 2 != 0)
                    startingIndex = (int)(0 - (spell.width / 2f) + 0.5f);
                else
                    startingIndex = (int)(0 - (spell.width / 2f));

                if (direction.x != 0)
                    startingIndexY = startingIndex;
                else if (direction.y != 0)
                    startingIndexX = startingIndex;

                for (int k = startingIndexX; k < 0 - startingIndexX + 1; k++)
                {
                    for (int j = startingIndexY; j < 0 - startingIndexY + 1; j++)
                    {
                        if ((GameManager.player.standingNode.gridX + direction.x + k) >= 0 && (GameManager.player.standingNode.gridX + direction.x + k) < GameManager.nodeGrid.grid.GetLength(0))
                        {
                            if ((GameManager.player.standingNode.gridY + direction.y + j) >= 0 && (GameManager.player.standingNode.gridY + direction.y + j) < GameManager.nodeGrid.grid.GetLength(1))
                            {
                                List<Node> waveNodes = new();

                                for (int i = 1; i < spell.range + 1; i++)
                                {
                                    Node consideredNode;

                                    if (!((GameManager.player.standingNode.gridX + (direction.x * i) + k) >= 0 && (GameManager.player.standingNode.gridX + (direction.x * i) + k) < GameManager.nodeGrid.grid.GetLength(0)
                                        && (GameManager.player.standingNode.gridY + (direction.y * i) + j) >= 0 && (GameManager.player.standingNode.gridY + (direction.y * i) + j) < GameManager.nodeGrid.grid.GetLength(1))
                                        || !GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * i) + k, GameManager.player.standingNode.gridY + (direction.y * i) + j].walkable)
                                    {
                                        break;
                                    }

                                    consideredNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * i) + k, GameManager.player.standingNode.gridY + (direction.y * i) + j];
                                    waveNodes.Add(consideredNode);
                                    MonoBehaviour.Instantiate(spell.vfx, consideredNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                                }

                                foreach (Enemy prospectiveEnemy in GameManager.enemyList)
                                {
                                    if (waveNodes.Contains(prospectiveEnemy.standingNode))
                                    {
                                        DamageEnemy(prospectiveEnemy, spell, direction);
                                    }
                                }
                            }
                        }
                    }
                }
                break;

            case (SpellType.Cross):
                LineAttack(Vector2Int.up, spell);
                LineAttack(Vector2Int.down, spell);
                LineAttack(Vector2Int.left, spell);
                LineAttack(Vector2Int.right, spell);
                break;

            case (SpellType.Radius):
                List<Node> radiusNodes = new();

                for (int y = -spell.range + GameManager.player.standingNode.gridY; y <= spell.range + GameManager.player.standingNode.gridY; y++)
                {
                    for (int x = -spell.range + GameManager.player.standingNode.gridX; x <= spell.range + GameManager.player.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0))
                        {
                            if (y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                            {
                                if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode || !GameManager.nodeGrid.grid[x,y].walkable)
                                    continue;

                                radiusNodes.Add(GameManager.nodeGrid.grid[x, y]);
                                MonoBehaviour.Instantiate(spell.vfx, GameManager.nodeGrid.grid[x, y].worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                            }
                        }
                    }
                }

                foreach (Enemy prospectiveEnemy in GameManager.enemyList)
                {
                    if (radiusNodes.Contains(prospectiveEnemy.standingNode))
                    {
                        DamageEnemy(prospectiveEnemy, spell, direction);
                    }
                }
                break;

            case (SpellType.Horseshoe):
                List<Node> horseshoeNodes = new();
                
                for (int y = -spell.range + GameManager.player.standingNode.gridY; y <= spell.range + GameManager.player.standingNode.gridY; y++)
                {
                    for (int x = -spell.range + GameManager.player.standingNode.gridX; x <= spell.range + GameManager.player.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode || !GameManager.nodeGrid.grid[x, y].walkable)
                                continue;

                            if (direction == Vector2Int.up)
                            {
                                if (Mathf.Sign(y - GameManager.player.standingNode.gridY) == -1)
                                    continue;
                            }
                            else if (direction == Vector2Int.down)
                            {
                                if (Mathf.Sign(y - GameManager.player.standingNode.gridY) == 1 && (y - GameManager.player.standingNode.gridY != 0))
                                    continue;
                            }
                            else if (direction == Vector2Int.left && (x - GameManager.player.standingNode.gridX != 0))
                            {
                                if (Mathf.Sign(x - GameManager.player.standingNode.gridX) == 1)
                                    continue;
                            }
                            else if (direction == Vector2Int.right)
                            {
                                if (Mathf.Sign(x - GameManager.player.standingNode.gridX) == -1)
                                    continue;
                            }

                            horseshoeNodes.Add(GameManager.nodeGrid.grid[x, y]);
                            MonoBehaviour.Instantiate(spell.vfx, GameManager.nodeGrid.grid[x, y].worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
                        }
                    }
                }

                foreach (Enemy prospectiveEnemy in GameManager.enemyList)
                {
                    if (horseshoeNodes.Contains(prospectiveEnemy.standingNode))
                    {
                        DamageEnemy(prospectiveEnemy, spell, direction);
                    }
                }
                break;
        }

        switch(spell.affliction.affliction)
        {
            case (AfflictionType.Burn):
                GameManager.player.RemoveAffliction(AfflictionType.Drown);
                break;

            case (AfflictionType.Drown):
                GameManager.player.RemoveAffliction(AfflictionType.Burn);
                break;
        }

        GameManager.player.ChangeDirection(direction);
    }

    public static bool CanAttack(SpellItem spell, Vector2Int direction)
    {
        switch (spell.spellType)
        {
            case (SpellType.Point):
                if ((GameManager.player.standingNode.gridX + (direction.x * spell.range)) >= 0 && (GameManager.player.standingNode.gridX + (direction.x * spell.range)) < GameManager.nodeGrid.grid.GetLength(0))
                {
                    if ((GameManager.player.standingNode.gridY + (direction.y * spell.range)) >= 0 && (GameManager.player.standingNode.gridY + (direction.y * spell.range)) < GameManager.nodeGrid.grid.GetLength(1))
                    {
                        if (!GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * spell.range), GameManager.player.standingNode.gridY + (direction.y * spell.range)].walkable)
                            return false;

                        return true;
                    }
                }
                break;

            case (SpellType.Line):
                if ((GameManager.player.standingNode.gridX + direction.x) >= 0 && (GameManager.player.standingNode.gridX + direction.x) < GameManager.nodeGrid.grid.GetLength(0))
                {
                    if ((GameManager.player.standingNode.gridY + direction.y) >= 0 && (GameManager.player.standingNode.gridY + direction.y) < GameManager.nodeGrid.grid.GetLength(1))
                    {
                        if (!GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + direction.x, GameManager.player.standingNode.gridY + direction.y].walkable)
                            return false;

                        return true;
                    }
                }
                break;

            case (SpellType.Wave):
                int startingIndex, startingIndexX = 0, startingIndexY = 0;

                if (spell.width % 2 != 0)
                    startingIndex = (int)(0 - (spell.width / 2f) + 0.5f);
                else
                    startingIndex = (int)(0 - (spell.width / 2f));

                if (direction.x != 0)
                {
                    startingIndexY = startingIndex;
                }
                else if (direction.y != 0)
                {
                    startingIndexX = startingIndex;
                }

                for (int i = startingIndexX; i < 0 - startingIndexX + 1; i++)
                {
                    for (int j = startingIndexY; j < 0 - startingIndexY + 1; j++)
                    {
                        if ((GameManager.player.standingNode.gridX + direction.x + i) >= 0 && (GameManager.player.standingNode.gridX + direction.x + i) < GameManager.nodeGrid.grid.GetLength(0))
                        {
                            if ((GameManager.player.standingNode.gridY + direction.y + j) >= 0 && (GameManager.player.standingNode.gridY + direction.y + j) < GameManager.nodeGrid.grid.GetLength(1))
                            {
                                if (!GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + direction.x + i, GameManager.player.standingNode.gridY + direction.y + j].walkable)
                                    continue;
                                
                                return true;
                            }
                        }
                    }
                }
                break;

            case (SpellType.Cross):
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if ((GameManager.player.standingNode.gridX + x) >= 0 && (GameManager.player.standingNode.gridX + x) < GameManager.nodeGrid.grid.GetLength(0))
                        {
                            if ((GameManager.player.standingNode.gridY + y) >= 0 && (GameManager.player.standingNode.gridY + y) < GameManager.nodeGrid.grid.GetLength(1))
                            {
                                if (Mathf.Abs(x) + Mathf.Abs(y) > 1 || !GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + x, GameManager.player.standingNode.gridY + y].walkable)
                                    continue;

                                return true;
                            }
                        }
                    }
                }
                break;

            case (SpellType.Radius):
                return true;

            case (SpellType.Horseshoe):
                for (int y = -spell.range + GameManager.player.standingNode.gridY; y <= spell.range + GameManager.player.standingNode.gridY; y++)
                {
                    for (int x = -spell.range + GameManager.player.standingNode.gridX; x <= spell.range + GameManager.player.standingNode.gridX; x++)
                    {
                        if (x >= 0 && x < GameManager.nodeGrid.grid.GetLength(0) && y >= 0 && y < GameManager.nodeGrid.grid.GetLength(1))
                        {
                            if (GameManager.nodeGrid.grid[x, y] == GameManager.player.standingNode || !GameManager.nodeGrid.grid[x, y].walkable)
                                continue;

                            if (direction == Vector2Int.up)
                            {
                                if (Mathf.Sign(y - GameManager.player.standingNode.gridY) == -1)
                                    continue;
                            }
                            else if (direction == Vector2Int.down)
                            {
                                if (Mathf.Sign(y - GameManager.player.standingNode.gridY) == 1 && (y - GameManager.player.standingNode.gridY != 0))
                                    continue;
                            }
                            else if (direction == Vector2Int.left && (x - GameManager.player.standingNode.gridX != 0))
                            {
                                if (Mathf.Sign(x - GameManager.player.standingNode.gridX) == 1)
                                    continue;
                            }
                            else if (direction == Vector2Int.right)
                            {
                                if (Mathf.Sign(x - GameManager.player.standingNode.gridX) == -1)
                                    continue;
                            }

                            return true;
                        }
                    }
                }
                break;
        }

        return false;
    }

    public static void LineAttack(Vector2Int direction, SpellItem spell)
    {
        List<Node> lineNodes = new();

        for (int i = 1; i < spell.range + 1; i++)
        {
            Node consideredNode;

            if (!((GameManager.player.standingNode.gridX + (direction.x * i)) >= 0 && (GameManager.player.standingNode.gridX + (direction.x * i)) < GameManager.nodeGrid.grid.GetLength(0)
                && (GameManager.player.standingNode.gridY + (direction.y * i)) >= 0 && (GameManager.player.standingNode.gridY + (direction.y * i)) < GameManager.nodeGrid.grid.GetLength(1))
                || !GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * i), GameManager.player.standingNode.gridY + (direction.y * i)].walkable)
            {
                break;
            }

            consideredNode = GameManager.nodeGrid.grid[GameManager.player.standingNode.gridX + (direction.x * i), GameManager.player.standingNode.gridY + (direction.y * i)];
            lineNodes.Add(consideredNode);
            MonoBehaviour.Instantiate(spell.vfx, consideredNode.worldPosition - (Vector2.up * 0.5f), Quaternion.identity);
        }

        foreach (Enemy prospectiveEnemy in GameManager.enemyList)
        {
            if (lineNodes.Contains(prospectiveEnemy.standingNode))
            {
                DamageEnemy(prospectiveEnemy, spell, direction);
            }
        }
    }

    public static void DamageEnemy(Enemy target, SpellItem spell, Vector2Int direction)
    {
        target.Damage(spell.damage, spell.affliction.affliction);

        if (spell.affliction.affliction != AfflictionType.None)
        {
            target.AddAffliction(spell);
        }

        if (spell.knockback > 0)
        {
            Node consideredNode;
            
            for (int i = 1; i < spell.knockback + 1; i++)
            {
                bool outOfBounds = false;
                bool blocked = false;

                if (!(target.standingNode.gridX + (direction.x * i) >= 0 && target.standingNode.gridX + (direction.x * i) < GameManager.nodeGrid.grid.GetLength(0) 
                    && target.standingNode.gridY + (direction.y * i) >= 0 && target.standingNode.gridY + (direction.y * i) < GameManager.nodeGrid.grid.GetLength(1)))
                {
                    outOfBounds = true;
                    blocked = true;
                }

                if (!outOfBounds)
                {
                    consideredNode = GameManager.nodeGrid.grid[target.standingNode.gridX + (direction.x * i), target.standingNode.gridY + (direction.y * i)];

                    if (!consideredNode.walkable)
                        blocked = true;

                    if (!blocked)
                    {
                        foreach (Enemy blockingEnemies in GameManager.enemyList)
                        {
                            if (blockingEnemies.standingNode == consideredNode)
                            {
                                blockingEnemies.Damage(IngameChar.collisionDamage, AfflictionType.None);
                                blockingEnemies.AddAffliction(blockingEnemies.stun);
                                blocked = true;
                                break;
                            }
                        }
                    }
                }

                if (blocked)
                {
                    target.standingNode = GameManager.nodeGrid.grid[target.standingNode.gridX + (direction.x * (i - 1)), target.standingNode.gridY + (direction.y * (i - 1))];
                    target.transform.position = target.standingNode.worldPosition - (Vector2.up * 0.25f);
                    target.Damage(IngameChar.collisionDamage, AfflictionType.None);
                    target.AddAffliction(target.stun);
                    return;
                }
            }

            target.standingNode = GameManager.nodeGrid.grid[target.standingNode.gridX + (direction.x * spell.knockback), target.standingNode.gridY + (direction.y * spell.knockback)];
            target.transform.position = target.standingNode.worldPosition - (Vector2.up * 0.25f);
        }
    }
}
