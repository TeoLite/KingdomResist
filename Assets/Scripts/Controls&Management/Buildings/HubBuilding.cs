using UnityEngine;
using UnityEngine.Events;

public class HubBuilding : Building, IDamageable
{
    [Header("Team")]
    [SerializeField] private Team team = Team.Player;
    
    [Header("Level Settings")]
    [SerializeField] private int level = 1;
    [SerializeField] private float healthIncreasePerLevel = 100f;
    [SerializeField] private float defenseIncreasePerLevel = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private GameObject destroyVFX;
    [SerializeField] private GameObject levelUpVFX;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Events
    public UnityEvent<float> onHealthChanged;
    public UnityEvent<int> onLevelUp;
    public UnityEvent onDestroyed;

    // IDamageable implementation
    public bool IsDead => base.currentHealth <= 0;
    public Transform Transform => transform;
    public Team Team => team;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        float totalMaxHealth = base.maxHealth + (level - 1) * healthIncreasePerLevel;
        base.currentHealth = totalMaxHealth;
        onHealthChanged?.Invoke(base.currentHealth / totalMaxHealth);
    }

    public override void TakeDamage(float damage)
    {
        if (IsDead) return;

        // Apply defense reduction
        float totalDefense = defense + (level - 1) * defenseIncreasePerLevel;
        float damageReduction = totalDefense / (totalDefense + 100f); // Defense formula
        float actualDamage = damage * (1f - damageReduction);

        base.currentHealth -= actualDamage;
        float totalMaxHealth = base.maxHealth + (level - 1) * healthIncreasePerLevel;
        
        // Visual feedback
        if (damageVFX != null)
        {
            Instantiate(damageVFX, transform.position, Quaternion.identity);
        }
        
        animator?.SetTrigger("Hit");
        
        // Flash red
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }

        onHealthChanged?.Invoke(base.currentHealth / totalMaxHealth);

        if (base.currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    public void LevelUp()
    {
        level++;
        float oldMaxHealth = base.maxHealth + (level - 2) * healthIncreasePerLevel;
        float newMaxHealth = base.maxHealth + (level - 1) * healthIncreasePerLevel;
        
        // Heal by the health increase amount
        base.currentHealth += (newMaxHealth - oldMaxHealth);
        
        if (levelUpVFX != null)
        {
            Instantiate(levelUpVFX, transform.position, Quaternion.identity);
        }
        
        animator?.SetTrigger("LevelUp");
        onLevelUp?.Invoke(level);
        onHealthChanged?.Invoke(base.currentHealth / newMaxHealth);
    }

    private void Die()
    {
        if (destroyVFX != null)
        {
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }
        
        animator?.SetTrigger("Destroy");
        onDestroyed?.Invoke();
        
        // Disable components but don't destroy the object
        // This allows for game over logic to be handled elsewhere
        enabled = false;
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }
    }

    public float GetHealthPercentage()
    {
        float totalMaxHealth = base.maxHealth + (level - 1) * healthIncreasePerLevel;
        return base.currentHealth / totalMaxHealth;
    }

    public int GetLevel()
    {
        return level;
    }
} 