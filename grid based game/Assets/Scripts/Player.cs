using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : IngameChar
{
    [Header("Player")]
    public int gold = 0;
    
    private KeyCode[] keyCodes = { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B };
    [HideInInspector] public int desiredSpellIndex = 0;
    private bool spellSelected = false, spellCasted = false;
    [HideInInspector] public bool inInv = false;

    private void Start()
    {
        PlayerStatsInvDisplay.playerHealthBar.SetMaxHealth(maxHealth);
        ChangeGold(0);
    }

    private void Update()
    {
        if (GameManager.canInput)
        {
            if (!inInv)
            {
                if (Input.anyKeyDown)
                {
                    for (int i = 0; i < keyCodes.Length; i++)
                    {
                        if (Input.GetKeyDown(keyCodes[i]) && i < GameManager.gameManager.activeSpells.Count)
                        {
                            if (!spellSelected && !GameManager.gameManager.activeSpells[i].casted && GameManager.gameManager.activeSpells[i].spell != null)
                            {
                                desiredSpellIndex = i;
                                spellSelected = true;
                                PlayerStatsInvDisplay.spellDisplay.SelectSpell(i, GameManager.gameManager.activeSpells[i].spell.altSprite);
                            }
                            else if (spellSelected)
                            {
                                spellSelected = false;
                                PlayerStatsInvDisplay.spellDisplay.UnselectSpell(GameManager.gameManager.activeSpells[desiredSpellIndex].spell.sprite);

                                if (i != desiredSpellIndex && !GameManager.gameManager.activeSpells[i].casted && GameManager.gameManager.activeSpells[i].spell != null)
                                {
                                    desiredSpellIndex = i;
                                    spellSelected = true;
                                    PlayerStatsInvDisplay.spellDisplay.SelectSpell(i, GameManager.gameManager.activeSpells[i].spell.altSprite);
                                }
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (spellSelected)
                    {
                        PlayerStatsInvDisplay.spellDisplay.UnselectSpell(GameManager.gameManager.activeSpells[desiredSpellIndex].spell.sprite);
                    }

                    GameManager.gameManager.StartCoroutine(GameManager.gameManager.NewTurn(Vector2Int.zero));
                }

                if (!spellSelected)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        GameManager.gameManager.StartCoroutine(GameManager.gameManager.NewTurn(Vector2Int.left));
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        GameManager.gameManager.StartCoroutine(GameManager.gameManager.NewTurn(Vector2Int.right));
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        GameManager.gameManager.StartCoroutine(GameManager.gameManager.NewTurn(Vector2Int.up));
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        GameManager.gameManager.StartCoroutine(GameManager.gameManager.NewTurn(Vector2Int.down));
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        Attack(Vector2Int.left);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        Attack(Vector2Int.right);
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        Attack(Vector2Int.up);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        Attack(Vector2Int.down);
                    }
                    else if (Input.GetKeyDown(KeyCode.A) && GameManager.gameManager.storedSpells[desiredSpellIndex].spell != null && !spellCasted)
                    {
                        PlayerStatsInvDisplay.spellDisplay.SwapSpell(new SpellBarSlot(desiredSpellIndex, SpellIconType.Active), new SpellBarSlot(desiredSpellIndex, SpellIconType.Stored));
                        PlayerStatsInvDisplay.spellDisplay.SelectSpell(desiredSpellIndex, GameManager.gameManager.activeSpells[desiredSpellIndex].spell.altSprite);
                    }
                }
            }

            if (!spellCasted)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    inInv = !inInv;

                    if (inInv)
                        PlayerStatsInvDisplay.spellDisplay.UpdateStats();

                    if (spellSelected)
                    {
                        PlayerStatsInvDisplay.spellDisplay.UnselectSpell(GameManager.gameManager.activeSpells[desiredSpellIndex].spell.sprite);
                        spellSelected = false;
                    }
                }
            }
        }
    }

    private void Attack(Vector2Int direction)
    {
        bool canAttack = PlayerActions.CanAttack(GameManager.gameManager.activeSpells[desiredSpellIndex].spell, direction);

        if (canAttack)
        {
            GameManager.gameManager.PlayerAttack(GameManager.gameManager.activeSpells[desiredSpellIndex].spell, direction);
            GameManager.gameManager.activeSpells[desiredSpellIndex].casted = true;
            spellSelected = false;
            spellCasted = true;
            PlayerStatsInvDisplay.spellDisplay.UseSpell(GameManager.gameManager.activeSpells[desiredSpellIndex].spell.sprite);
        }
    }

    public void ChangeGold(int amount)
    {
        gold += amount;
        PlayerStatsInvDisplay.playerStatsInvDisplay.SetGold(gold);
    }

    public override void Damage(int amount, AfflictionType afflictionType)
    {
        base.Damage(amount, afflictionType);

        PlayerStatsInvDisplay.playerHealthBar.SetHealth(currentHealth);
    }


    public override void AddAffliction(SpellItem spell)
    {
        base.AddAffliction(spell);
        PlayerStatsInvDisplay.playerHealthBar.UpdateAfflictions();
    }

    public override void RemoveAffliction(AfflictionType afflictionType)
    {
        base.RemoveAffliction(afflictionType);
        PlayerStatsInvDisplay.playerHealthBar.UpdateAfflictions();
    }

    public override void UseAffliction()
    {
        base.UseAffliction();
        PlayerStatsInvDisplay.playerHealthBar.UpdateAfflictions();
    }

    public void FreshAttack()
    {
        spellSelected = false;
        spellCasted = false;

        foreach (SpellSlot spell in GameManager.gameManager.activeSpells)
        {
            spell.casted = false;

            if (spell.spell != null)
                PlayerStatsInvDisplay.spellDisplay.ResetSpell(GameManager.gameManager.activeSpells.IndexOf(spell), spell.spell.sprite);
        }

    }
}
