using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AfflictionType {None, Burn, Drown, Stun};

[System.Serializable]
public class Affliction
{
    public AfflictionType affliction;
    public int turns;
    public GameObject effect;
    public SpellItem appliedSpell;

    public Affliction(AfflictionType _affliction, int _turns, SpellItem _appliedSpell)
    {
        affliction = _affliction;
        turns = _turns;
        appliedSpell = _appliedSpell;
    }
}
