using UnityEngine;

public class WizardTower : Tower
{
    public enum SpellType
    {
        Fire,
        Heal,
        Boost,
        Lightning
    }

    [Header("Wizard Tower Specific")]
    [SerializeField] private SpellType currentSpellType = SpellType.Fire;
    [SerializeField] private GameObject[] spellEffectPrefabs; // One for each spell type
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private float boostMultiplier = 1.5f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float manaCost = 25f;

    private Transform currentTarget;
    private PlayerHub playerHub;

    protected override void Start()
    {
        base.Start();
        playerHub = FindObjectOfType<PlayerHub>();
        if (playerHub == null)
        {
            Debug.LogError("PlayerHub not found in the scene!");
        }
    }

    protected override void FindTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (Collider2D collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = collider.transform;
            }
        }

        currentTarget = closestTarget;
    }

    protected override void Attack()
    {
        if (currentTarget == null || playerHub == null) return;

        if (playerHub.TrySpendMana(manaCost))
        {
            switch (currentSpellType)
            {
                case SpellType.Fire:
                    CastFireSpell();
                    break;
                case SpellType.Heal:
                    CastHealSpell();
                    break;
                case SpellType.Boost:
                    CastBoostSpell();
                    break;
                case SpellType.Lightning:
                    CastLightningSpell();
                    break;
            }
        }
    }

    private void CastFireSpell()
    {
        GameObject spellEffect = Instantiate(spellEffectPrefabs[(int)SpellType.Fire], currentTarget.position, Quaternion.identity);
        IDamageable target = currentTarget.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(spellEffect, 1f);
    }

    private void CastHealSpell()
    {
        GameObject spellEffect = Instantiate(spellEffectPrefabs[(int)SpellType.Heal], currentTarget.position, Quaternion.identity);
        IHealable target = currentTarget.GetComponent<IHealable>();
        if (target != null)
        {
            target.Heal(healAmount);
        }
        Destroy(spellEffect, 1f);
    }

    private void CastBoostSpell()
    {
        GameObject spellEffect = Instantiate(spellEffectPrefabs[(int)SpellType.Boost], currentTarget.position, Quaternion.identity);
        IBoostable target = currentTarget.GetComponent<IBoostable>();
        if (target != null)
        {
            target.ApplyBoost(boostMultiplier, 5f); // 5 second boost duration
        }
        Destroy(spellEffect, 1f);
    }

    private void CastLightningSpell()
    {
        GameObject spellEffect = Instantiate(spellEffectPrefabs[(int)SpellType.Lightning], currentTarget.position, Quaternion.identity);
        IStunnable target = currentTarget.GetComponent<IStunnable>();
        if (target != null)
        {
            target.ApplyStun(stunDuration);
        }
        Destroy(spellEffect, 1f);
    }

    public void SetSpellType(SpellType spellType)
    {
        currentSpellType = spellType;
    }

    public void UpgradeSpell(float damageIncrease, float manaCostReduction)
    {
        damage += damageIncrease;
        manaCost = Mathf.Max(5f, manaCost - manaCostReduction);
    }
} 