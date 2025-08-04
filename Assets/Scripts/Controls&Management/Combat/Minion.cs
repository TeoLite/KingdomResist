using UnityEngine;
using System.Collections;

public class Minion : MonoBehaviour, IDamageable
{
    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    public animatioPlayer_SpalshLight_Script animationPlay;

    [Header("Runtime Data")]
    [SerializeField] private MinionData data;
    private int level;
    private float currentHealth;
    private IDamageable target;
    private bool isAttacking;
    private float lastAttackTime;
    private bool isInitialized = false;
    private Team team;
    private bool isStunned;
    private float stunEndTime;

    // Stats modified by level
    private float currentMaxHealth;
    private float currentDamage;
    
    // IDamageable implementation
    public bool IsDead => currentHealth <= 0;
    public Transform Transform => transform;
    public Team Team => team;

    private void Awake()
    {
        // Get required components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing SpriteRenderer component!");
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing Rigidbody2D component!");
            return;
        }

        // Set up Rigidbody2D for proper movement
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Verify Collider2D
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing Collider2D component!");
        }

        animator = GetComponent<Animator>();
        // Animator is optional, so no error if missing
    }

    public void Initialize(MinionData minionData, int minionLevel, Team team)
    {
        if (minionData == null)
        {
            Debug.LogError($"[{gameObject.name}] Trying to initialize with null MinionData!");
            return;
        }

        this.team = team;
        data = minionData;
        level = Mathf.Max(1, minionLevel); // Ensure minimum level of 1

        // Apply level multipliers
        float levelMultiplier = 1f + (level - 1) * 0.1f; // 10% increase per level
        currentMaxHealth = data.maxHealth * levelMultiplier;
        currentDamage = data.damage * levelMultiplier;
        
        // Set initial health
        currentHealth = currentMaxHealth;

        // Set visual elements if components exist
        if (spriteRenderer != null && data.minionSprite != null)
        {
            spriteRenderer.sprite = data.minionSprite;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Missing sprite renderer or minion sprite!");
        }

        // Try to play animation if the component exists
        if (animationPlay != null)
        {
            animationPlay.PlayAnimation();
        }

        // Spawn VFX if available
        if (data.spawnVFX != null)
        {
            Instantiate(data.spawnVFX, transform.position, Quaternion.identity);
        }

        // Find initial target
        FindNewTarget();

        isInitialized = true;
        Debug.Log($"[{gameObject.name}] Initialized successfully with {data.minionName} at level {level}. MoveSpeed: {data.moveSpeed}");
    }

    private void Update()
    {
        if (!isInitialized)
        {
            // Instead of error, try to recover if possible
            if (data != null)
            {
                Initialize(data, 1, Team.Neutral);
                return;
            }
            Debug.LogWarning($"[{gameObject.name}] Minion not initialized, waiting for initialization...");
            return;
        }

        // Update stun status
        UpdateStunStatus();
        if (isStunned) return;

        // If no target or target is dead, find new target
        if (target == null || target.IsDead)
        {
            FindNewTarget();
        }

        // If we have a target, handle movement and combat
        if (target != null && !target.IsDead)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.Transform.position);
            
            if (distanceToTarget <= data.attackRange)
            {
                rb.velocity = Vector2.zero;  // Stop when in attack range
                Attack();
            }
            else
            {
                Move();
            }
        }
        else
        {
            // No valid target, try to move towards enemy base
            MoveTowardsEnemyBase();
        }

        // Debug current state
        Debug.Log($"[{gameObject.name}] State: Target={target?.Transform.name ?? "None"}, Velocity={rb.velocity}, Position={transform.position}");
    }

    private void MoveTowardsEnemyBase()
    {
        // Find enemy base based on team
        Transform baseTransform = null;
        if (team == Team.Player)
        {
            var enemyHub = FindObjectOfType<EnemyCamp>();
            if (enemyHub != null) baseTransform = enemyHub.transform;
        }
        else
        {
            var playerHub = FindObjectOfType<PlayerHub>();
            if (playerHub != null) baseTransform = playerHub.transform;
        }

        // If found enemy base, move towards it
        if (baseTransform != null)
        {
            Vector2 direction = (baseTransform.position - transform.position).normalized;
            rb.velocity = direction * data.moveSpeed;

            // Flip sprite based on movement direction
            if (direction.x != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            Debug.Log($"[{gameObject.name}] Moving towards base at {baseTransform.position}. Direction={direction}, Velocity={rb.velocity}");
        }
        else
        {
            rb.velocity = Vector2.zero;
            Debug.LogWarning($"[{gameObject.name}] No enemy base found!");
        }
    }

    private void Move()
    {
        if (!isInitialized || data == null || rb == null || isStunned) return;

        if (target != null && !target.IsDead)
        {
            Vector2 direction = (target.Transform.position - transform.position).normalized;
            Vector2 newVelocity = direction * data.moveSpeed;
            rb.velocity = newVelocity;

            // Flip sprite based on movement direction
            if (direction.x != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            // Debug movement
            Debug.DrawLine(transform.position, target.Transform.position, Color.red, 0.1f);
            Debug.Log($"[{gameObject.name}] Moving towards target {target.Transform.name}. Direction={direction}, NewVelocity={newVelocity}, ActualVelocity={rb.velocity}, Speed={data.moveSpeed}");
        }
    }

    private void Attack()
    {
        if (target == null || target.IsDead) return;

        if (Time.time - lastAttackTime >= 1f / data.attackSpeed)
        {
            lastAttackTime = Time.time;

            // Trigger attack animation
            animator?.SetTrigger("Attack");

            // For ranged units, spawn projectile
            if (data.isRanged && data.projectilePrefab != null)
            {
                GameObject projectileObj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
                if (projectileObj.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Initialize(currentDamage, data.projectileSpeed, target.Transform);
                }
            }
            // For melee units, apply damage directly
            else
            {
                target.TakeDamage(currentDamage);
            }

            // Spawn attack VFX
            if (data.attackVFX != null)
            {
                Instantiate(data.attackVFX, target.Transform.position, Quaternion.identity);
            }

            // Handle special abilities
            HandleSpecialAbility();
        }
    }

    private void HandleSpecialAbility()
    {
        switch (data.specialAbility)
        {
            case MinionSpecialAbility.Heal:
                HealNearbyAllies();
                break;
            case MinionSpecialAbility.Boost:
                BoostNearbyAllies();
                break;
            case MinionSpecialAbility.Stun:
                StunTarget();
                break;
        }
    }

    private void FindNewTarget()
    {
        // Clear current target
        target = null;

        // Try to find enemy hub first
        if (team == Team.Player)
        {
            var enemyHub = FindObjectOfType<EnemyHub>();
            if (enemyHub != null && !enemyHub.IsDead && 
                Vector2.Distance(transform.position, enemyHub.transform.position) <= data.detectionRange)
            {
                target = enemyHub;
                Debug.Log($"[{gameObject.name}] Found enemy hub as target at distance {Vector2.Distance(transform.position, enemyHub.transform.position)}");
                return;
            }
        }
        else
        {
            var playerHub = FindObjectOfType<PlayerHub>();
            if (playerHub != null && !playerHub.IsDead && 
                Vector2.Distance(transform.position, playerHub.transform.position) <= data.detectionRange)
            {
                target = playerHub;
                Debug.Log($"[{gameObject.name}] Found player hub as target at distance {Vector2.Distance(transform.position, playerHub.transform.position)}");
                return;
            }
        }

        // If no hub in range, look for enemy minions
        int layerMask = LayerMask.GetMask("Default", "Player", "Enemy"); // Adjust these layer names as needed
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, data.detectionRange, layerMask);
        float closestDistance = float.MaxValue;
        IDamageable closestTarget = null;

        Debug.Log($"[{gameObject.name}] Found {colliders.Length} potential targets within range {data.detectionRange}");

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out var damageable))
            {
                // Skip if same team or already dead
                if (damageable.Team == team || damageable.IsDead) continue;

                float distance = Vector2.Distance(transform.position, damageable.Transform.position);
                Debug.Log($"[{gameObject.name}] Found potential target {collider.name} at distance {distance}. Team={damageable.Team}, IsDead={damageable.IsDead}");
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = damageable;
                }
            }
        }

        if (closestTarget != null)
        {
            target = closestTarget;
            Debug.Log($"[{gameObject.name}] Found new target: {target.Transform.name} at distance {closestDistance}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No valid targets found within range {data.detectionRange}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        // Trigger hit animation/VFX
        animator?.SetTrigger("Hit");
        if (data.hitVFX != null)
        {
            Instantiate(data.hitVFX, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Spawn death VFX
        if (data.deathVFX != null)
        {
            Instantiate(data.deathVFX, transform.position, Quaternion.identity);
        }

        // Stop movement and disable collider
        if (rb != null) rb.velocity = Vector2.zero;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;

        // Trigger death animation if available
        animator?.SetTrigger("Death");

        // Destroy after delay if death animation exists
        float destroyDelay = animator != null ? 0f : 0f;
        Destroy(gameObject, destroyDelay);
    }

    // Special ability implementations
    private void HealNearbyAllies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, data.specialAbilityRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Minion>(out var ally))
            {
                if (ally.Team == team && !ally.IsDead)
                {
                    ally.Heal(currentDamage * 0.5f); // Heal for 50% of damage
                }
            }
        }
    }

    private void BoostNearbyAllies()
    {
        // Implement boost logic
        // This could temporarily increase damage or attack speed of nearby allies
    }

    private void StunTarget()
    {
        if (target != null && target is Minion targetMinion)
        {
            targetMinion.ApplyStun(1f); // Stun for 1 second
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, currentMaxHealth);
    }

    private void UpdateStunStatus()
    {
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                Debug.Log($"[{gameObject.name}] Stun wore off");
            }
            else
            {
                rb.velocity = Vector2.zero;  // Ensure we don't move while stunned
            }
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        rb.velocity = Vector2.zero;
        Debug.Log($"[{gameObject.name}] Stunned for {duration} seconds");
    }
} 