using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameChar : MonoBehaviour
{
    public const int burnDamage = 5, drownDamage = 3, collisionDamage = 6; 

    [Header("Sprite")]
    [SerializeField] private Sprite up;
    [SerializeField] private Sprite down, left, right;
    [SerializeField] private Material defaultMat, whiteMat;
    [SerializeField] private float damageTime;

    [Header("Health")]
    public int maxHealth;
    public int currentHealth;
    public List<AfflictionType> immunites = new();
    public List<AfflictionType> weaknesses = new();
    public List<Affliction> afflictions = new();

    [Header("Other")]
    public SpellItem stun;

    public Node standingNode;

    private SpriteRenderer sr;

    public virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    public void ChangeDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            sr.sprite = up;
        if (direction == Vector2Int.down)
            sr.sprite = down;
        if (direction == Vector2Int.left)
            sr.sprite = left;
        if (direction == Vector2Int.right)
            sr.sprite = right;
    }

    public virtual void Damage(int amount, AfflictionType afflictionType)
    {
        if (weaknesses.Contains(afflictionType))
            amount *= 2;

        currentHealth -= amount;

        if (amount > 0)
        IngameUI.ingameUI.SpawnDamageText(-amount, standingNode.worldPosition);

        StartCoroutine(Damaged());
    }

    public virtual void AddAffliction(SpellItem spell)
    {
        switch (spell.affliction.affliction)
        {
            case (AfflictionType.Burn):
                if (immunites.Contains(AfflictionType.Burn))
                    return;
                break;
            case (AfflictionType.Drown):
                if (immunites.Contains(AfflictionType.Drown))
                    return;
                break;
        }
        
        foreach (Affliction currentAffliction in afflictions)
        {
            if (currentAffliction.affliction == spell.affliction.affliction)
            {
                currentAffliction.turns += spell.affliction.turns;
                currentAffliction.appliedSpell = spell;
                return;
            }
        }

        afflictions.Add(new Affliction(spell.affliction.affliction, spell.affliction.turns, spell));

        if (spell.affliction.effect != null)
            afflictions[afflictions.Count - 1].effect = Instantiate(spell.affliction.effect, standingNode.worldPosition, Quaternion.identity, transform);

        ParticleSystem particles = afflictions[afflictions.Count - 1].effect.GetComponent<ParticleSystem>();

        if (particles != null)
        {
            var particleShape = particles.shape;
            particleShape.spriteRenderer = sr;
        }
    }

    public virtual void UseAffliction()
    {
        List<Affliction> toBeRemoved = new();

        int afflictionDamage = 0;
        foreach (Affliction affliction in afflictions)
        {
            switch (affliction.affliction)
            {
                case(AfflictionType.Burn):
                    
                    if (weaknesses.Contains(AfflictionType.Burn))
                        afflictionDamage += (burnDamage * 2);
                    else
                        afflictionDamage += burnDamage;

                    break;
                case(AfflictionType.Drown):

                    if (weaknesses.Contains(AfflictionType.Drown))
                        afflictionDamage += (drownDamage * 2);
                    else
                        afflictionDamage += drownDamage;

                    break;
            }
            affliction.turns--;

            if (affliction.turns <= 0)
            {
                toBeRemoved.Add(affliction);
            }
        }

        if (afflictionDamage > 0)
            Damage(afflictionDamage, AfflictionType.None);

        foreach (Affliction remove in toBeRemoved)
        {
            afflictions.Remove(remove);

            if (remove.effect != null)
                Destroy(remove.effect);
        }
    }

    public virtual void RemoveAffliction(AfflictionType afflictionType)
    {
        Affliction toBeRemoved = null;

        foreach (Affliction affliction in afflictions)
        {
           if (affliction.affliction == afflictionType)
            {
                toBeRemoved = affliction;
            }
        }

        if (toBeRemoved != null)
        {
            afflictions.Remove(toBeRemoved);

            if (toBeRemoved.effect != null)
                Destroy(toBeRemoved.effect);
        }
    }

    private IEnumerator Damaged()
    {
        sr.material = whiteMat;
        yield return new WaitForSeconds(damageTime);
        sr.material = defaultMat;
    }
}
