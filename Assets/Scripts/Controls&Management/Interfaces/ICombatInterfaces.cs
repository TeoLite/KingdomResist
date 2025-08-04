using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    bool IsDead { get; }
    Transform Transform { get; }
    Team Team { get; }
}

public interface IHealable
{
    void Heal(float amount);
}

public interface IBoostable
{
    void ApplyBoost(float multiplier, float duration);
}

public interface IStunnable
{
    void ApplyStun(float duration);
}

public interface IProjectile
{
    void Initialize(float damage, float speed, Transform target);
} 