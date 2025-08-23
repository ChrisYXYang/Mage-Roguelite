using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Managers
    public static GameManager gameManager;

    //statics
    public static Player player;
    public static NodeGrid nodeGrid;
    public static Pathfinding pathfinding;

    public static List<Enemy> enemyList = new();
    public static List<Enemy> enemiesToBeRemoved = new();
    public static List<Node> nextNodeList = new();
    public static List<Node> path = new();

    public static bool canInput = true, canSpawnRoom = false;
    public static int enemyDamage = 0;

    [Header("Player Settings and Items")]
    public List<SpellSlot> activeSpells = new();
    public List<SpellSlot> storedSpells = new();
    public List<PassiveItem> passiveItems = new();
    public ActiveItem activeItem;
    public int activeSpellSlots;
    public int storedSpellSlots;

    //others
    [Header("Game Settings")]
    [SerializeField] private float moveDelay;
    [SerializeField] private float enemyAttackDelay, enemyAfflictionDelay, playerAfflictionDelay;

    [Header("Other")]
    [SerializeField] private Transform playerTransform;

    [HideInInspector] public int playerStartX, playerStartY;

    private void Awake()
    {
        //Game Manager
        if (gameManager == null)
        {
            gameManager = this;
            DontDestroyOnLoad(gameObject);

            //Others
            pathfinding = GetComponent<Pathfinding>();
            nodeGrid = GetComponent<NodeGrid>();

            //spell
            while (activeSpells.Count < activeSpellSlots)
                activeSpells.Add(new SpellSlot(null));

            for (int i = 0; i < storedSpellSlots; i++)
            {
                storedSpells.Add(new SpellSlot(null));
            }

            playerStartX = (int)(playerTransform.position.x + (nodeGrid.worldSize.x / 2));
            playerStartY = (int)(playerTransform.position.y + (nodeGrid.worldSize.y / 2));

            NewRoomPt2();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        canSpawnRoom = true;
    }

    private void Update()
    {
        if (enemyList.Count <= 0)
        {
            MapManager.currentRoom.roomCleared = true;
        }
    }

    public IEnumerator NewTurn(Vector2Int direction)
    {
        canInput = false;

        bool canMove = PlayerActions.CanMove(direction);

        if (player.afflictions.Count > 0)
        {
            player.UseAffliction();
            yield return new WaitForSeconds(playerAfflictionDelay);
        }

        if (canMove)
        {
            if (enemyList.Count > 0)
            {
                bool skipEnemyAttackDelay = true;
                enemyDamage = 0;

                for (int i = 0; i < enemyList.Count; i++)
                {
                    bool currentCanAttack = EnemyActions.CanAttack(i);

                    if (currentCanAttack)
                    {
                        EnemyActions.Attack(i);
                        skipEnemyAttackDelay = false;
                    }
                }

                if (enemyDamage > 0)
                    player.Damage(enemyDamage, AfflictionType.None);

                ClearEnemies();

                if (!skipEnemyAttackDelay)
                    yield return new WaitForSeconds(enemyAttackDelay);
            }
            
            //in case player is displaced check again
            if (PlayerActions.CanMove(direction))
                PlayerActions.Move(direction);

            if (enemyList.Count > 0)
            {
                path.Clear();
                nextNodeList.Clear();

                for (int i = 0; i < enemyList.Count; i++)
                {
                    bool currentCanMove = EnemyActions.CanMove(i);

                    if (currentCanMove)
                    {
                        EnemyActions.Move(i);
                    }
                }

                bool afflictionDelay = false;
                foreach (Enemy enemy in enemyList)
                {
                    foreach(Affliction affliction in enemy.afflictions)
                    {
                        if (affliction.affliction != AfflictionType.Stun)
                        {
                            afflictionDelay = true;
                            break;
                        }
                    }
                    
                    if (afflictionDelay)
                    {
                        yield return new WaitForSeconds(enemyAfflictionDelay);
                        break;
                    }
                }

                foreach (Enemy enemy in enemyList)
                {
                    enemy.UseAffliction();
                }

                ClearEnemies();
            }

            yield return new WaitForSeconds(moveDelay);
            player.FreshAttack();
        }
;
        canInput = true;
    }

    public void PlayerAttack(SpellItem spell, Vector2Int direction)
    {
        PlayerActions.Attack(spell, direction);

        ClearEnemies();
    }

    public void NewRoom(int x, int y)
    {
        enemyList.Clear();
        StopAllCoroutines();

        playerStartX = x;
        playerStartY = y;
    }

    public void NewRoomPt2()
    {
        nodeGrid.CreateGrid();
        player = FindObjectOfType<Player>();
    }

    public void NewRoomPt3()
    {
        foreach (Enemy enemy in enemyList)
        {
            enemy.standingNode = nodeGrid.NodeFromWorldPoint(enemy.transform.position);
            enemy.transform.position = enemy.standingNode.worldPosition - (Vector2.up * 0.25f);
        }

        player.standingNode = nodeGrid.grid[playerStartX, playerStartY];
        player.transform.position = player.standingNode.worldPosition - (Vector2.up * 0.25f);
    }

    private void ClearEnemies()
    {
        foreach (Enemy enemyCheck in enemiesToBeRemoved)
        {
            enemyList.Remove(enemyCheck);
        }

        enemiesToBeRemoved.Clear();
    }
}
