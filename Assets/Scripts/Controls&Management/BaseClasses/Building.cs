using UnityEngine;

public abstract class Building : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float defense = 10f;

    // IDamageable implementation
    public virtual bool IsDead => currentHealth <= 0;
    public Transform Transform => transform;
    public virtual Team Team => Team.Neutral; // Override in derived classes

    protected virtual void Start()
    {
        // Only set currentHealth if it hasn't been set yet
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(0, damage - defense);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);

        if (currentHealth <= 0)
        {
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        // Override in derived classes
        gameObject.SetActive(false);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}