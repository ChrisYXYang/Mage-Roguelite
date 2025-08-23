using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public void DestroyEffect()
    {
        Destroy(transform.root.gameObject);
    }
}
