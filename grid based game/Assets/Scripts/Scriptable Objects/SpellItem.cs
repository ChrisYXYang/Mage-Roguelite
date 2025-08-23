using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType { Point, Line, Wave, Diagonal, Cross, Radius, DiagonalCross, Horseshoe}

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class SpellItem : Item
{
    [Header("Stats")]
    public GameObject vfx;
    public int damage, range, width, knockback, upgradeCost, tier;
    public bool maxLevel;
    public SpellType spellType;
    public Affliction affliction;
    public SpellItem nextLevel;
}
