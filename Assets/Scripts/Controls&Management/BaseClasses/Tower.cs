using UnityEngine;
using System.Collections;

public class Tower : Building
{
    [Header("Tower Settings")]
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected GameObject projectilePrefab;

    [Header("Kingdom Specific")]
    [SerializeField] protected SpriteRenderer towerSprite;
    [SerializeField] protected Sprite[] kingdomSprites; // Array of sprites for different kingdoms

    protected float attackTimer;
    protected Transform target;
    protected bool isSubscribed = false;

    protected override void Start()
    {
        base.Start();
        
        if (!isSubscribed)
        {
            PlayerHub.OnKingdomTypeChanged += HandleKingdomTypeChanged;
            isSubscribed = true;
        }

        // Wait a frame to ensure PlayerHub is initialized
        StartCoroutine(InitializeTowerAppearance());
    }

    private System.Collections.IEnumerator InitializeTowerAppearance()
    {
        // Wait for PlayerHub to be fully initialized
        while (PlayerHub.Instance == null)
        {
            yield return null;
        }

        // Update appearance with current kingdom type
        UpdateTowerAppearance(PlayerHub.Instance.GetKingdomType());
    }

    protected virtual void OnDestroy()
    {
        if (isSubscribed)
        {
            PlayerHub.OnKingdomTypeChanged -= HandleKingdomTypeChanged;
            isSubscribed = false;
        }
    }

    private void HandleKingdomTypeChanged(KingdomType newKingdomType)
    {
        UpdateTowerAppearance(newKingdomType);
    }

    public virtual void UpdateTowerAppearance(KingdomType kingdomType)
    {
        Debug.Log($"[{gameObject.name}] Updating tower appearance for kingdom: {kingdomType}");
        
        if (towerSprite == null)
        {
            Debug.LogError($"[{gameObject.name}] Tower sprite renderer is null!");
            return;
        }

        if (kingdomSprites == null || kingdomSprites.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] Kingdom sprites array is null or empty!");
            return;
        }

        int spriteIndex = (int)kingdomType;
        Debug.Log($"[{gameObject.name}] Sprite Index: {spriteIndex}, Array Length: {kingdomSprites.Length}");

        if (spriteIndex < kingdomSprites.Length && kingdomSprites[spriteIndex] != null)
        {
            towerSprite.sprite = kingdomSprites[spriteIndex];
            Debug.Log($"[{gameObject.name}] Successfully changed sprite for kingdom: {kingdomType}");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Missing sprite for kingdom type {kingdomType} at index {spriteIndex}");
        }
    }

    protected virtual void Update()
    {
        if (target != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackSpeed)
            {
                Attack();
                attackTimer = 0f;
            }
        }
        else
        {
            FindTarget();
        }
    }

    protected virtual void FindTarget()
    {
        // Implementation will vary based on tower type
    }

    protected virtual void Attack()
    {
        // Implementation will vary based on tower type
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 