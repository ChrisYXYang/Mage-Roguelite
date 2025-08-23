using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEnemy : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemies = new();
    
    private void Awake()
    {
        Instantiate(enemies[Random.Range(0, enemies.Count)], transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
