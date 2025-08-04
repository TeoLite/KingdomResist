using UnityEngine;

public class EnemyHub : EnemyCamp
{
    // Properties for UI access
    public float CurrentHealth => base.currentHealth;
    public float MaxHealth => base.maxHealth;
    public float CurrentMana => currentMana;
    public float MaxMana => manaCapacity;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
} 