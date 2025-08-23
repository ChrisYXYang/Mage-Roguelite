using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Normal, Bomb};

public class Enemy : IngameChar
{
    [Header("Enemy")]
    public SpellItem spell;
    public EnemyType enemyType;
    [SerializeField] private int minGold, maxGold;
    
    public override void Awake()
    {
        base.Awake();
        GameManager.enemyList.Add(this);
    }

    public override void Damage(int amount, AfflictionType afflictionType)
    {
        base.Damage(amount, afflictionType);

        if (currentHealth <= 0)
        {
            GameManager.player.ChangeGold(Random.Range(minGold, maxGold + 1));
            GameManager.enemiesToBeRemoved.Add(this);
            Destroy(gameObject);
        }
    }
}
