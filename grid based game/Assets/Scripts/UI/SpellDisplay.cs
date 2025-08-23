using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpellIconType { Active, Stored };

public class SpellDisplay : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameObject spellIcon;
    [SerializeField] private Material grayscaleMat;
    [SerializeField] int selectedY, defaultY;
    [SerializeField] private float spacing;
    [SerializeField] private Sprite tier1, tier2, tier3;

    private List<GameObject> activeSpellIcons = new();
    private List<GameObject> storedSpellIcons = new();

    private GameObject selectedSpell, storedHolder, activeHolder, spellSelector, spellSelected, spellStats;
    private Vector2 spellStartingPos;

    private RectTransform spellSelectorRectTransform, spellSelectedRectTransform;
    private int rows, activeSpellIndex, storedSpellIndex;
    private bool selectingActive, slotSelected = false;
    private SpellBarSlot selectedSlot; 

    private void Awake()
    {
        activeHolder = transform.GetChild(0).gameObject;
        storedHolder = transform.GetChild(1).gameObject;
        spellSelector = transform.GetChild(2).gameObject;
        spellSelected = transform.GetChild(3).gameObject;
        spellStats = transform.GetChild(4).gameObject;

        spellSelectorRectTransform = spellSelector.GetComponent<RectTransform>();
        spellSelectedRectTransform = spellSelected.GetComponent<RectTransform>();
    }

    private void Start()
    {
        spellStartingPos = new Vector2(-(spacing * ((GameManager.gameManager.activeSpellSlots / 2f) - 0.5f)), defaultY);

        for (int i = 0; i < GameManager.gameManager.activeSpellSlots; i++)
        {
            CreateActiveSlot(i);
        }

        rows = 1;
        int xSlot = 0;
        for (int i = 0; i < GameManager.gameManager.storedSpellSlots; i++)
        {
            if (i % 5 == 0 && i != 0)
            {
                rows++;
                xSlot = 0;
            }

            CreateStoredSlot(i, xSlot, rows);
            xSlot++;
        }
    }

    private void Update()
    {
        if ( GameManager.player.inInv)
        {
            storedHolder.SetActive(true);
            spellSelector.SetActive(true);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwitchSelection(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwitchSelection(Vector2Int.right);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SwitchSelection(Vector2Int.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SwitchSelection(Vector2Int.down);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                if (slotSelected)
                {
                    if (selectingActive)
                    {
                        SwapSpell(new SpellBarSlot(activeSpellIndex, SpellIconType.Active), selectedSlot);
                    }
                    else
                    {
                        SwapSpell(new SpellBarSlot(storedSpellIndex, SpellIconType.Stored), selectedSlot);
                    }

                    slotSelected = false;
                    spellSelected.SetActive(false);
                }
                else
                {
                    if (selectingActive)
                    {
                        if (GameManager.gameManager.activeSpells[activeSpellIndex].spell != null)
                        {
                            selectedSlot.index = activeSpellIndex;
                            selectedSlot.bar = SpellIconType.Active;
                            slotSelected = true;

                            spellSelected.SetActive(true);
                            spellSelectedRectTransform.localPosition = activeSpellIcons[activeSpellIndex].GetComponent<RectTransform>().localPosition;
                        }
                    }
                    else
                    {
                        if (GameManager.gameManager.storedSpells[storedSpellIndex].spell != null)
                        {
                            selectedSlot.index = storedSpellIndex;
                            selectedSlot.bar = SpellIconType.Stored;
                            slotSelected = true;

                            spellSelected.SetActive(true);
                            spellSelectedRectTransform.localPosition = storedSpellIcons[storedSpellIndex].GetComponent<RectTransform>().localPosition;
                        }
                    }
                }
            }
        }
        else
        {
            storedHolder.SetActive(false);
            spellSelector.SetActive(false);
            spellSelected.SetActive(false);
            spellStats.SetActive(false);
            activeSpellIndex = GameManager.player.desiredSpellIndex;
            spellSelectorRectTransform.localPosition = new Vector2(activeSpellIcons[activeSpellIndex].GetComponent<RectTransform>().localPosition.x, defaultY);
            selectingActive = true;
            slotSelected = false;
        }
    }

    public void SwitchSelection(Vector2Int direction)
    {
        if (selectingActive)
        {
            if (direction.x != 0)
            {
                if (activeSpellIndex + direction.x < GameManager.gameManager.activeSpellSlots && activeSpellIndex + direction.x >= 0)
                {
                    activeSpellIndex += direction.x;
                    spellSelectorRectTransform.localPosition = activeSpellIcons[activeSpellIndex].GetComponent<RectTransform>().localPosition;
                }
            }
            else if (direction.y == 1)
            {
                storedSpellIndex = activeSpellIndex;
                spellSelectorRectTransform.localPosition = storedSpellIcons[storedSpellIndex].GetComponent<RectTransform>().localPosition;
                selectingActive = false;
            }
        }
        else
        {
            int tester = storedSpellIndex;
            int selectedRow = 0;

            while (tester >= 0)
            {
                tester -= GameManager.gameManager.storedSpellSlots / rows;
                selectedRow++;
            }

            selectedRow -= 1;
            
            if (direction.x != 0)
            {
                if (storedSpellIndex + direction.x < (selectedRow + 1) * GameManager.gameManager.storedSpellSlots / rows && storedSpellIndex + direction.x >= 0 + (selectedRow * GameManager.gameManager.storedSpellSlots / rows))
                {
                    storedSpellIndex += direction.x;
                    spellSelectorRectTransform.localPosition = storedSpellIcons[storedSpellIndex].GetComponent<RectTransform>().localPosition;
                }
            }
            else
            {
                if (direction.y == 1 && selectedRow < rows - 1)
                {
                    storedSpellIndex = storedSpellIndex + GameManager.gameManager.storedSpellSlots / rows;
                    spellSelectorRectTransform.localPosition = storedSpellIcons[storedSpellIndex].GetComponent<RectTransform>().localPosition;
                }
                else if (direction.y == -1)
                {
                    if (selectedRow > 0)
                    {
                        storedSpellIndex = storedSpellIndex - GameManager.gameManager.storedSpellSlots / rows;
                        spellSelectorRectTransform.localPosition = storedSpellIcons[storedSpellIndex].GetComponent<RectTransform>().localPosition;
                    }
                    else if (storedSpellIndex < GameManager.gameManager.activeSpellSlots)
                    {
                        activeSpellIndex = storedSpellIndex;
                        spellSelectorRectTransform.localPosition = activeSpellIcons[activeSpellIndex].GetComponent<RectTransform>().localPosition;
                        selectingActive = true;
                    }
                }
            }
        }

        UpdateStats();
    }

    public void AddSpell(int index, Sprite sprite, SpellIconType type)
    {
        if (type == SpellIconType.Active)
        {
            activeSpellIcons[index].GetComponent<Image>().sprite = sprite;
            activeSpellIcons[index].GetComponent<Image>().enabled = true;
            activeSpellIcons[index].transform.GetChild(0).GetComponent<Image>().enabled = true;

            switch (GameManager.gameManager.activeSpells[index].spell.tier)
            {
                case (1):
                    activeSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier1;
                    break;

                case (2):
                    activeSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier3;
                    break;

                case (3):
                    activeSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier3;
                    break;
            }
        }
        else
        {
            storedSpellIcons[index].GetComponent<Image>().sprite = sprite;
            storedSpellIcons[index].GetComponent<Image>().enabled = true;
            storedSpellIcons[index].transform.GetChild(0).GetComponent<Image>().enabled = true;

            switch (GameManager.gameManager.storedSpells[index].spell.tier)
            {
                case (1):
                    storedSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier1;
                    break;

                case (2):
                    storedSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier3;
                    break;

                case (3):
                    storedSpellIcons[index].transform.GetChild(0).GetComponent<Image>().sprite = tier3;
                    break;
            }
        }
    }

    public void CreateActiveSlot(int index)
    {
        activeSpellIcons.Add(Instantiate(spellIcon, Vector2.zero, Quaternion.identity, activeHolder.transform));
        activeSpellIcons[index].GetComponent<RectTransform>().localPosition = spellStartingPos + (Vector2.right * (index * spacing));

        if (GameManager.gameManager.activeSpells[index].spell != null)
        {
            AddSpell(index, GameManager.gameManager.activeSpells[index].spell.sprite, SpellIconType.Active);
            return;
        }

        HideSpell(index, SpellIconType.Active);
    }

    public void CreateStoredSlot(int index, int xSlot, int row)
    {
        storedSpellIcons.Add(Instantiate(spellIcon, Vector2.zero, Quaternion.identity, storedHolder.transform));
        storedSpellIcons[index].GetComponent<RectTransform>().localPosition = spellStartingPos + (Vector2.up * 30) + (Vector2.right * (xSlot * spacing) + (Vector2.up * spacing * rows));

        if (GameManager.gameManager.storedSpells[index].spell != null)
        {
            AddSpell(index, GameManager.gameManager.storedSpells[index].spell.sprite, SpellIconType.Stored);
            return;
        }

        HideSpell(index, SpellIconType.Stored);
    }

    public void HideSpell(int index, SpellIconType type)
    {
        if (type == SpellIconType.Active)
        {
            activeSpellIcons[index].GetComponent<Image>().enabled = false;
            activeSpellIcons[index].transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
        else
        {
            storedSpellIcons[index].GetComponent<Image>().enabled = false;
            storedSpellIcons[index].transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
    }

    public void SwapSpell(SpellBarSlot selectedSpell, SpellBarSlot storedSpell)
    {
        //store temp holder spell//////////////////////////////////////////////////
        SpellItem spellHolder;

        if (selectedSpell.bar == SpellIconType.Active)
            spellHolder = GameManager.gameManager.activeSpells[selectedSpell.index].spell;
        else
            spellHolder = GameManager.gameManager.storedSpells[selectedSpell.index].spell;

        //swap the spells////////////////////////////////////////////////////////////////////
        bool noSelectedSpell = false;

        if (selectedSpell.bar == SpellIconType.Active)
        {
            if (GameManager.gameManager.activeSpells[selectedSpell.index].spell == null)
                noSelectedSpell = true;
        }
        else
        {
            if (GameManager.gameManager.storedSpells[selectedSpell.index].spell == null)
                noSelectedSpell = true;
        }

        if (selectedSpell.bar == SpellIconType.Active)
        {
            if (storedSpell.bar == SpellIconType.Active)
                GameManager.gameManager.activeSpells[selectedSpell.index].spell = GameManager.gameManager.activeSpells[storedSpell.index].spell;
            else
                GameManager.gameManager.activeSpells[selectedSpell.index].spell = GameManager.gameManager.storedSpells[storedSpell.index].spell;
        }
        else
        {
            if (storedSpell.bar == SpellIconType.Active)
                GameManager.gameManager.storedSpells[selectedSpell.index].spell = GameManager.gameManager.activeSpells[storedSpell.index].spell;
            else
                GameManager.gameManager.storedSpells[selectedSpell.index].spell = GameManager.gameManager.storedSpells[storedSpell.index].spell;
        }

        if (noSelectedSpell)
        {
            if (storedSpell.bar == SpellIconType.Active)
                GameManager.gameManager.activeSpells[storedSpell.index].spell = null;
            else
                GameManager.gameManager.storedSpells[storedSpell.index].spell = null;
        }
        else
        {
            if (storedSpell.bar == SpellIconType.Active)
                GameManager.gameManager.activeSpells[storedSpell.index].spell = spellHolder;
            else
                GameManager.gameManager.storedSpells[storedSpell.index].spell = spellHolder;
        }

        //change the sprites for all/////////////////////////////////////////////////////////////
        if (selectedSpell.bar == SpellIconType.Active)
            AddSpell(selectedSpell.index, GameManager.gameManager.activeSpells[selectedSpell.index].spell.sprite, SpellIconType.Active);
        else
            AddSpell(selectedSpell.index, GameManager.gameManager.storedSpells[selectedSpell.index].spell.sprite, SpellIconType.Stored);

        if (noSelectedSpell)
        {
            if (storedSpell.bar == SpellIconType.Active)
                HideSpell(storedSpell.index, SpellIconType.Active);
            else
                HideSpell(storedSpell.index, SpellIconType.Stored);
        }
        else
        {
            if (storedSpell.bar == SpellIconType.Active)
                AddSpell(storedSpell.index, GameManager.gameManager.activeSpells[storedSpell.index].spell.sprite, SpellIconType.Active);
            else
                AddSpell(storedSpell.index, GameManager.gameManager.storedSpells[storedSpell.index].spell.sprite, SpellIconType.Stored);
        }

        UpdateStats();
    }

    public void UpdateStats()
    {
        if (selectingActive)
        {
            if (GameManager.gameManager.activeSpells[activeSpellIndex].spell != null && GameManager.player.inInv)
            {
                spellStats.SetActive(true);
                spellStats.GetComponent<SpellStatsUI>().SetStats(activeSpellIndex, SpellIconType.Active);
            }
            else
                spellStats.SetActive(false);
        }
        else
        {
            if (GameManager.gameManager.storedSpells[storedSpellIndex].spell != null && GameManager.player.inInv)
            {
                spellStats.SetActive(true);
                spellStats.GetComponent<SpellStatsUI>().SetStats(storedSpellIndex, SpellIconType.Stored);
            }
            else
                spellStats.SetActive(false);
        }
    }

    public void SelectSpell(int index, Sprite sprite)
    {
        if (selectedSpell != null)
            selectedSpell.GetComponent<RectTransform>().localPosition = new Vector2(selectedSpell.GetComponent<RectTransform>().localPosition.x, defaultY);

        selectedSpell = activeSpellIcons[index];
        selectedSpell.GetComponent<RectTransform>().localPosition = new Vector2(selectedSpell.GetComponent<RectTransform>().localPosition.x, selectedY);
        selectedSpell.GetComponent<Image>().sprite = sprite;
    }

    public void UnselectSpell(Sprite sprite)
    {
        selectedSpell.GetComponent<RectTransform>().localPosition = new Vector2(selectedSpell.GetComponent<RectTransform>().localPosition.x, defaultY);
        selectedSpell.GetComponent<Image>().sprite = sprite;
        selectedSpell = null;
    }

    public void UseSpell(Sprite sprite)
    {
        selectedSpell.GetComponent<RectTransform>().localPosition = new Vector2(selectedSpell.GetComponent<RectTransform>().localPosition.x, defaultY);
        selectedSpell.GetComponent<Image>().material = grayscaleMat;
        selectedSpell.GetComponent<Image>().sprite = sprite;
        selectedSpell = null;
    }

    public void ResetSpell(int index, Sprite sprite)
    {
        activeSpellIcons[index].GetComponent<Image>().material = null;
        activeSpellIcons[index].GetComponent<Image>().sprite = sprite;
    }
}

public struct SpellBarSlot
{
    public int index;
    public SpellIconType bar;

    public SpellBarSlot(int _index, SpellIconType _bar)
    {
        index = _index;
        bar = _bar;
    }
}
