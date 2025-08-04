using UnityEngine;

[CreateAssetMenu(fileName = "New Minion", menuName = "Kingdom Resist/Minion Data")]
public class MinionData : ScriptableObject
{
    [Header("Basic Info")]
    public string minionName = "Unnamed Minion";
    [SerializeField] private Sprite _minionSprite;
    public int manaCost = 10;
    public KingdomType kingdom;
    [SerializeField] private GameObject _minionPrefab;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float moveSpeed = 3f;
    public float attackRange = 1f;
    public float detectionRange = 10f;

    [Header("Combat Properties")]
    public bool isRanged;
    public bool canTargetAir;
    public bool canTargetBuildings = true;
    public bool isFlying;
    public MinionSpecialAbility specialAbility;
    public float specialAbilityRange = 5f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    
    [Header("Visual Effects")]
    public GameObject spawnVFX;
    public GameObject deathVFX;
    public GameObject attackVFX;
    public GameObject hitVFX;

    [TextArea(3, 5)]
    public string description;

    // Properties with validation
    public Sprite minionSprite 
    {
        get => _minionSprite;
        set
        {
            if (value == null)
            {
                Debug.LogError($"[{name}] Cannot set null sprite for minion!");
                return;
            }
            _minionSprite = value;
        }
    }

    public GameObject minionPrefab
    {
        get => _minionPrefab;
        set
        {
            if (value == null)
            {
                Debug.LogError($"[{name}] Cannot set null prefab for minion!");
                return;
            }
            
            // Validate that the prefab has required components
            var prefabMinion = value.GetComponent<Minion>();
            if (prefabMinion == null)
            {
                Debug.LogError($"[{name}] Minion prefab must have Minion component!");
                return;
            }

            var prefabRenderer = value.GetComponent<SpriteRenderer>();
            if (prefabRenderer == null)
            {
                Debug.LogError($"[{name}] Minion prefab must have SpriteRenderer component!");
                return;
            }

            var prefabRigidbody = value.GetComponent<Rigidbody2D>();
            if (prefabRigidbody == null)
            {
                Debug.LogError($"[{name}] Minion prefab must have Rigidbody2D component!");
                return;
            }

            _minionPrefab = value;
        }
    }

    private void OnValidate()
    {
        // Ensure name is not empty
        if (string.IsNullOrEmpty(minionName))
        {
            minionName = "Unnamed Minion";
        }

        // Ensure positive values for stats
        maxHealth = Mathf.Max(1f, maxHealth);
        damage = Mathf.Max(0f, damage);
        attackSpeed = Mathf.Max(0.1f, attackSpeed);
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        attackRange = Mathf.Max(0.1f, attackRange);
        manaCost = Mathf.Max(0, manaCost);

        // Validate sprite and prefab
        if (_minionSprite == null)
        {
            Debug.LogError($"[{name}] Minion sprite is required!");
        }

        if (_minionPrefab == null)
        {
            Debug.LogError($"[{name}] Minion prefab is required!");
        }
    }
}


public enum MinionSpecialAbility
{
    None,
    Heal,
    Boost,
    Stun,
    AreaDamage,
    Summon
} 