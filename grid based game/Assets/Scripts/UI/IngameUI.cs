using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameUI : MonoBehaviour
{
    public static IngameUI ingameUI;
    [SerializeField] private GameObject damageText;
    [SerializeField] private float dmgTxtLife;

    private void Awake()
    {
        if (ingameUI == null)
        {
            ingameUI = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnDamageText(int damage, Vector2 position)
    {
        GameObject dmgText = Instantiate(damageText, Vector2.zero, Quaternion.identity, transform);
        dmgText.GetComponent<TextMeshProUGUI>().text = "" + damage;
        dmgText.transform.position = position + Vector2.up * 0.75f;
        Destroy(dmgText, dmgTxtLife);
    }
}
