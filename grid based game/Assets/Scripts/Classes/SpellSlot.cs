using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellSlot
{
    public SpellItem spell;
    public bool casted;

    public SpellSlot(SpellItem _spell)
    {
        spell = _spell;
        casted = false;
    }
}
