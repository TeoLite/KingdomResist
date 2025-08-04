using UnityEngine;

public class DefenseTower : Tower
{
    [Header("Defense Tower Specific")]
    [SerializeField] private GameObject defenseProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 10f;

    protected override void Attack()
    {
        if (target != null)
        {
            // Create and setup projectile
            GameObject projectileObj = Instantiate(defenseProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Initialize(damage, projectileSpeed, target);
            }
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

        target = closestTarget;
    }

    public void UpgradeTower(float damageIncrease, float attackSpeedIncrease)
    {
        damage += damageIncrease;
        attackSpeed = Mathf.Max(0.1f, attackSpeed - attackSpeedIncrease);
    }
} 