using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemEffect { IncreaseSpells, IncreaseLife, IncreaseDamage};

public class PassiveItem : Item
{
    public ItemEffect itemEffect;
    public int effectMagnitude;
}
