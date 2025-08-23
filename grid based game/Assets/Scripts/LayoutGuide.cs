using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGuide : MonoBehaviour
{
    [SerializeField] private Vector2 worldSize;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector2.zero, worldSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector2.zero + (Vector2.up * (worldSize.y / 2)), Vector2.zero + (Vector2.down * (worldSize.y / 2)));
        Gizmos.DrawLine(Vector2.zero + (Vector2.left * (worldSize.x / 2)), Vector2.zero + (Vector2.right * (worldSize.x / 2)));
    }
}
