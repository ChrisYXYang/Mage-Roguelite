using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellStatsUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI spellNameTypeText;
    [SerializeField] private TextMeshProUGUI damageText, knockbackText, rangeText, widthText, afflictionText;

    [Header("Icon")]
    [SerializeField] private Image afflictionIcon;
    [SerializeField] private Sprite burnAffliction, drownAffliction;

    public void SetStats(int index, SpellIconType spellIconType)
    {
        if (spellIconType == SpellIconType.Active)
        {
            spellNameTypeText.text = GameManager.gameManager.activeSpells[index].spell.name + " - " + GameManager.gameManager.activeSpells[index].spell.spellType;
            damageText.text = "" + GameManager.gameManager.activeSpells[index].spell.damage;
            knockbackText.text = "" + GameManager.gameManager.activeSpells[index].spell.knockback;
            rangeText.text = "" + GameManager.gameManager.activeSpells[index].spell.range;
            widthText.text = "" + GameManager.gameManager.activeSpells[index].spell.width;
            afflictionText.text = "" + GameManager.gameManager.activeSpells[index].spell.affliction.turns;

            switch (GameManager.gameManager.activeSpells[index].spell.affliction.affliction)
            {
                case (AfflictionType.None):
                    afflictionIcon.enabled = false;
                    afflictionText.enabled = false;
                    break;
                case (AfflictionType.Burn):
                    afflictionIcon.enabled = true;
                    afflictionText.enabled = true; ;
                    afflictionIcon.sprite = burnAffliction;
                    break;
                case (AfflictionType.Drown):
                    afflictionIcon.enabled = true;
                    afflictionText.enabled = true;
                    afflictionIcon.sprite = drownAffliction;
                    break;
            }
        }
        else
        {
            spellNameTypeText.text = GameManager.gameManager.storedSpells[index].spell.name + " - " + GameManager.gameManager.storedSpells[index].spell.spellType;
            damageText.text = "" + GameManager.gameManager.storedSpells[index].spell.damage;
            knockbackText.text = "" + GameManager.gameManager.storedSpells[index].spell.knockback;
            rangeText.text = "" + GameManager.gameManager.storedSpells[index].spell.range;
            widthText.text = "" + GameManager.gameManager.storedSpells[index].spell.width;
            afflictionText.text = "" + GameManager.gameManager.storedSpells[index].spell.affliction.turns;

            switch (GameManager.gameManager.storedSpells[index].spell.affliction.affliction)
            {
                case (AfflictionType.None):
                    afflictionIcon.enabled = false;
                    afflictionText.enabled = false;
                    break;
                case (AfflictionType.Burn):
                    afflictionIcon.enabled = true;
                    afflictionText.enabled = true; ;
                    afflictionIcon.sprite = burnAffliction;
                    break;
                case (AfflictionType.Drown):
                    afflictionIcon.enabled = true;
                    afflictionText.enabled = true;
                    afflictionIcon.sprite = drownAffliction;
                    break;
            }
        }
    }
}
