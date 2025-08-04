using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MinionAI : MonoBehaviour
{
    private MinionData data;
    private GameObject currentTarget;
    private GameObject homeBase;
    private Rigidbody2D rb;
    private float currentAttackCooldown;
    private bool isInitialized;
    private Vector3 lastTargetPosition;
    private float nextRetargetTime;
    private float stoppingDistance = 1f;
    private float retargetingInterval = 1f;
    private float searchRadius = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.drag = 5f; // Add some drag to prevent sliding
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Initialize(GameObject enemyBase, GameObject ownBase, MinionData minionData)
    {
        homeBase = ownBase;
        currentTarget = enemyBase;
        lastTargetPosition = enemyBase.transform.position;
        data = minionData;
        isInitialized = true;

        // Set stopping distance based on attack range
        stoppingDistance = data.attackRange * 0.8f;
        searchRadius = data.attackRange * 3f;
    }

    private void Update()
    {
        if (!isInitialized || data == null) return;

        // Update attack cooldown
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
        }

        // Check if we need to find a new target
        if (Time.time >= nextRetargetTime)
        {
            FindNewTarget();
            nextRetargetTime = Time.time + retargetingInterval;
        }

        // Update movement and combat
        if (currentTarget != null)
        {
            lastTargetPosition = currentTarget.transform.position;
            float distanceToTarget = Vector3.Distance(transform.position, lastTargetPosition);

            // Check if in attack range
            if (distanceToTarget <= data.attackRange)
            {
                AttackTarget();
            }
            else if (distanceToTarget > stoppingDistance)
            {
                MoveTowardsTarget();
            }
        }
        else
        {
            // If no target, move towards last known position
            float distanceToLastPosition = Vector3.Distance(transform.position, lastTargetPosition);
            if (distanceToLastPosition > stoppingDistance)
            {
                MoveTowardsTarget();
            }
        }
    }

    private void FindNewTarget()
    {
        // First, validate current target
        if (currentTarget != null)
        {
            // Check if current target is still valid
            var damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Check if we can target this type
                bool canTarget = true;
                if (!data.canTargetBuildings && currentTarget.GetComponent<Building>() != null)
                {
                    canTarget = false;
                }
                if (!data.canTargetAir && currentTarget.GetComponent<MinionAI>()?.data.isFlying == true)
                {
                    canTarget = false;
                }
                if (canTarget) return; // Keep current target
            }
        }

        // Find all potential targets in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        GameObject bestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            // Skip if it's our own base
            if (collider.gameObject == homeBase)
                continue;

            // Check if it's a valid target
            var damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Check if we can target this type
                bool canTarget = true;
                if (!data.canTargetBuildings && collider.GetComponent<Building>() != null)
                {
                    canTarget = false;
                }
                if (!data.canTargetAir && collider.GetComponent<MinionAI>()?.data.isFlying == true)
                {
                    canTarget = false;
                }

                if (canTarget)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestTarget = collider.gameObject;
                    }
                }
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            lastTargetPosition = bestTarget.transform.position;
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (lastTargetPosition - transform.position).normalized;
        rb.velocity = direction * data.moveSpeed;

        // Update sprite facing direction
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
    }

    private void AttackTarget()
    {
        if (currentAttackCooldown <= 0 && currentTarget != null)
        {
            // Try to damage the target
            var damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(data.damage);
                currentAttackCooldown = 1f / data.attackSpeed; // Convert attacks per second to cooldown

                // Spawn attack VFX if available
                if (data.attackVFX != null)
                {
                    Vector3 vfxPosition = data.isRanged ? 
                        transform.position + (currentTarget.transform.position - transform.position).normalized :
                        currentTarget.transform.position;
                    Instantiate(data.attackVFX, vfxPosition, Quaternion.identity);
                }

                // Handle special abilities
                HandleSpecialAbility();
            }
        }
    }

    private void HandleSpecialAbility()
    {
        switch (data.specialAbility)
        {
            case MinionSpecialAbility.Heal:
                // Heal nearby friendly units
                Collider2D[] nearbyAllies = Physics2D.OverlapCircleAll(transform.position, data.attackRange);
                foreach (var ally in nearbyAllies)
                {
                    if (ally.gameObject != gameObject && ally.GetComponent<MinionAI>()?.homeBase == homeBase)
                    {
                        var damageable = ally.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            // Heal for 50% of damage value
                            damageable.TakeDamage(-data.damage * 0.5f);
                        }
                    }
                }
                break;

            case MinionSpecialAbility.AreaDamage:
                // Deal area damage around the target
                Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(currentTarget.transform.position, data.attackRange * 0.5f);
                foreach (var enemy in nearbyEnemies)
                {
                    if (enemy.gameObject != currentTarget && enemy.gameObject != homeBase)
                    {
                        var damageable = enemy.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(data.damage * 0.5f);
                        }
                    }
                }
                break;

            // Add other special abilities as needed
        }
    }

    private void OnDestroy()
    {
        if (data?.deathVFX != null)
        {
            Instantiate(data.deathVFX, transform.position, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);

            // Draw stopping distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);

            // Draw search radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
    }
} 