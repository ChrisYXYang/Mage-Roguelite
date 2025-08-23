using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStatsInvDisplay : MonoBehaviour
{
    public static PlayerStatsInvDisplay playerStatsInvDisplay;
    public static SpellDisplay spellDisplay;
    public static HealthBar playerHealthBar;

    [SerializeField] private TextMeshProUGUI goldText;

    private void Awake()
    {
        if (playerStatsInvDisplay == null)
        {
            playerStatsInvDisplay = this;
            spellDisplay = GetComponentInChildren<SpellDisplay>();
            playerHealthBar = GetComponentInChildren<HealthBar>();
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGold(int amount)
    {
        goldText.text = "" + amount;
    }
}
