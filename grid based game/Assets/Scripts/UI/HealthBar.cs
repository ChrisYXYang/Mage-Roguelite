using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    //variables
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject afflictionIcon;
    [SerializeField] private Sprite burn, drown;
    [SerializeField] private Vector2 afflictionIconStartingPos;
    [SerializeField] private TextMeshProUGUI healthText;

    private List<GameObject> afflictionIcons = new();

    public void SetHealth(int health)
    {
        slider.value = health;
        healthText.text = health + " / " + slider.maxValue;
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        healthText.text = health + " / " + health;
    }

    public void UpdateAfflictions()
    {
        int i = afflictionIcons.Count - 1;
        while (afflictionIcons.Count > GameManager.player.afflictions.Count)
        {
            Destroy(afflictionIcons[i]);
            afflictionIcons.Remove(afflictionIcons[i]);
            i--;
        }

        i = afflictionIcons.Count;
        while (afflictionIcons.Count < GameManager.player.afflictions.Count)
        {
            GameObject currentIcon = Instantiate(afflictionIcon, Vector2.zero, Quaternion.identity, transform);
            currentIcon.GetComponent<RectTransform>().localPosition = new Vector2(afflictionIconStartingPos.x + (i * 40), afflictionIconStartingPos.y);
            afflictionIcons.Add(currentIcon);
            i++;
        }

        for (int j = 0; j < GameManager.player.afflictions.Count; j++)
        {
            switch (GameManager.player.afflictions[j].affliction)
            {
                case (AfflictionType.Burn):
                    afflictionIcons[j].transform.GetChild(0).GetComponent<Image>().sprite = burn;
                    break;

                case (AfflictionType.Drown):
                    afflictionIcons[j].transform.GetChild(0).GetComponent<Image>().sprite = drown;
                    break;
            }
            
            afflictionIcons[j].GetComponentInChildren<TextMeshProUGUI>().text = "" + GameManager.player.afflictions[j].turns;
        }
    }
}
