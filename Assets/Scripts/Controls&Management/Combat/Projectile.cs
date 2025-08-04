using UnityEngine;

public class Projectile : MonoBehaviour, IProjectile
{
    private float damage;
    private float speed;
    private Transform target;
    private bool isInitialized;

    public void Initialize(float damage, float speed, Transform target)
    {
        this.damage = damage;
        this.speed = speed;
        this.target = target;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target
        Vector2 direction = (target.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Check if we've reached the target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 0.1f)
        {
            Hit();
        }
    }

    private void Hit()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == target)
        {
            Hit();
        }
    }
} 