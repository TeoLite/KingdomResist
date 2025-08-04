using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyCamp : Building, IDamageable
{
    [System.Serializable]
    public class MinionSpawnInfo
    {
        public MinionData minionData;
        [HideInInspector] public float currentCooldown;
        public int maxCount = 2; // Maximum number of this type of minion
        [HideInInspector] public int currentCount; // Current count of this type
    }

    // IDamageable implementation
    public bool IsDead => base.currentHealth <= 0;
    public Transform Transform => transform;
    public Team Team => Team.Enemy;

    [Header("Camp Stats")]
    [SerializeField] protected float manaCapacity = 100f;
    [SerializeField] protected float currentMana;
    [SerializeField] protected float manaRegenRate = 1f;
    [SerializeField] protected float spawnRadius = 3f;
    [SerializeField] protected KingdomType campType;

    [Header("Minion Management")]
    [SerializeField] protected MinionSpawnInfo[] availableMinions;
    [SerializeField] protected int maxMinionCount = 10;
    [SerializeField] protected Transform playerBase;
    [SerializeField] protected Transform[] spawnPoints; // Array of spawn points

    protected List<GameObject> activeMinions = new List<GameObject>();
    protected bool isActive;
    protected int currentSpawnPointIndex = 0; // Track which spawn point to use next

    protected override void Start()
    {
        base.Start(); // This will initialize health
        currentMana = manaCapacity;
        
        // Find player base
        if (playerBase == null)
        {
            playerBase = FindObjectOfType<PlayerHub>()?.transform;
            if (playerBase == null)
            {
                Debug.LogError("[EnemyCamp] Could not find PlayerHub!");
                enabled = false;
                return;
            }
        }

        // Find spawn points if not set
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Look for child objects with "SpawnPoint" in their name
            var foundSpawnPoints = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name.Contains("SpawnPoint"))
                {
                    foundSpawnPoints.Add(child);
                }
            }

            if (foundSpawnPoints.Count > 0)
            {
                spawnPoints = foundSpawnPoints.ToArray();
                Debug.Log($"[EnemyCamp] Found {spawnPoints.Length} spawn points");
            }
            else
            {
                Debug.LogWarning("[EnemyCamp] No spawn points found, will use random positions");
            }
        }

        // Initialize minion counts
        foreach (var minionInfo in availableMinions)
        {
            minionInfo.currentCount = 0;
            // Validate minion data
            if (minionInfo.minionData == null)
            {
                Debug.LogError("[EnemyCamp] MinionData is null in availableMinions!");
                continue;
            }
            // Validate minion prefab
            if (minionInfo.minionData.minionPrefab == null)
            {
                Debug.LogError($"[EnemyCamp] Minion prefab is missing for {minionInfo.minionData.name}!");
                continue;
            }
            // Check if prefab has Minion component
            if (minionInfo.minionData.minionPrefab.GetComponent<Minion>() == null)
            {
                Debug.LogError($"[EnemyCamp] Minion prefab {minionInfo.minionData.minionPrefab.name} is missing Minion component!");
                continue;
            }
        }

        // Validate minion data matches kingdom type
        ValidateAndFilterMinions();
    }

    protected virtual void ValidateAndFilterMinions()
    {
        List<MinionSpawnInfo> validMinions = new List<MinionSpawnInfo>();
        foreach (var minionInfo in availableMinions)
        {
            if (minionInfo.minionData != null && minionInfo.minionData.kingdom == campType)
            {
                validMinions.Add(minionInfo);
            }
            else
            {
                Debug.LogWarning($"[EnemyCamp] Removing minion {minionInfo.minionData?.name} as it doesn't match camp type {campType}");
            }
        }
        availableMinions = validMinions.ToArray();
    }

    protected virtual void Update()
    {
        if (!isActive) return;

        RegenerateMana();
        UpdateSpawnCooldowns();
        UpdateMinionCounts();
    }

    protected virtual void RegenerateMana()
    {
        if (currentMana < manaCapacity)
        {
            currentMana = Mathf.Min(manaCapacity, currentMana + (manaRegenRate * Time.deltaTime));
        }
    }

    protected virtual void UpdateSpawnCooldowns()
    {
        foreach (var minionInfo in availableMinions)
        {
            if (minionInfo.currentCooldown > 0)
            {
                minionInfo.currentCooldown -= Time.deltaTime;
            }
        }
    }

    protected virtual void UpdateMinionCounts()
    {
        // Remove null references and update counts
        activeMinions.RemoveAll(m => m == null);
        
        // Reset all counts
        foreach (var minionInfo in availableMinions)
        {
            minionInfo.currentCount = 0;
        }
        
        // Count current minions
        foreach (var minion in activeMinions)
        {
            if (minion != null)
            {
                string minionType = minion.name.Replace("(Clone)", "").Trim();
                foreach (var minionInfo in availableMinions)
                {
                    if (minionInfo.minionData.minionPrefab.name == minionType)
                    {
                        minionInfo.currentCount++;
                        break;
                    }
                }
            }
        }
    }

    protected virtual IEnumerator AIRoutine()
    {
        Debug.Log($"[EnemyCamp] Starting AI routine");
        
        while (isActive)
        {
            if (activeMinions.Count < maxMinionCount)
            {
                // Clean up destroyed minions
                activeMinions.RemoveAll(m => m == null);
                
                // Update minion counts
                foreach (var minionInfo in availableMinions)
                {
                    minionInfo.currentCount = activeMinions.Where(m => 
                        m != null && m.name.StartsWith(minionInfo.minionData.minionName)).Count();
                }
                
                // Try to spawn a minion
                TrySpawnMinion();
            }
            
            yield return new WaitForSeconds(1f);
        }
    }

    protected virtual void TrySpawnMinion()
    {
        // Find available minions to spawn
        var availableForSpawn = new List<MinionSpawnInfo>();
        foreach (var minionInfo in availableMinions)
        {
            if (minionInfo.currentCooldown <= 0 && 
                currentMana >= minionInfo.minionData.manaCost && 
                minionInfo.currentCount < minionInfo.maxCount)
            {
                availableForSpawn.Add(minionInfo);
            }
        }

        if (availableForSpawn.Count > 0)
        {
            // Sort by mana cost (higher cost = higher priority)
            availableForSpawn.Sort((a, b) => b.minionData.manaCost.CompareTo(a.minionData.manaCost));
            
            // Randomly select from top half of available minions
            int index = Random.Range(0, Mathf.Max(1, availableForSpawn.Count / 2));
            var selectedMinion = availableForSpawn[index];
            
            // Spawn the minion
            SpawnMinion(selectedMinion);
            
            // Set cooldown
            selectedMinion.currentCooldown = selectedMinion.minionData.attackSpeed * 2f;
            
            Debug.Log($"[EnemyCamp] Selected minion {selectedMinion.minionData.minionName} for spawning");
        }
    }

    protected virtual void SpawnMinion(MinionSpawnInfo minionInfo)
    {
        if (minionInfo == null || minionInfo.minionData == null || minionInfo.minionData.minionPrefab == null)
        {
            Debug.LogError("[EnemyCamp] Invalid minion info or missing prefab!");
            return;
        }

        // Get spawn position
        Vector3 spawnPosition;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Use spawn points in sequence
            spawnPosition = spawnPoints[currentSpawnPointIndex].position;
            currentSpawnPointIndex = (currentSpawnPointIndex + 1) % spawnPoints.Length;
        }
        else
        {
            // Random position within spawn radius
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        // Instantiate and initialize minion
        GameObject minionObj = Instantiate(minionInfo.minionData.minionPrefab, spawnPosition, Quaternion.identity);
        if (minionObj.TryGetComponent<Minion>(out var minion))
        {
            minion.Initialize(minionInfo.minionData, 1, Team.Enemy);
            activeMinions.Add(minionObj);
            minionInfo.currentCount++;
        }
        else
        {
            Debug.LogError("[EnemyCamp] Spawned minion prefab missing Minion component!");
            Destroy(minionObj);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // Visual feedback
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.SetTrigger("Hit");
        }

        if (base.currentHealth <= 0)
        {
            OnCampDestroyed();
        }
    }

    protected virtual void OnCampDestroyed()
    {
        isActive = false;
        
        // Destroy all active minions
        foreach (var minion in activeMinions.ToList())
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }
        activeMinions.Clear();

        // Notify GameManager or other systems
        GameManager.Instance?.OnEnemyCampDestroyed(this);

        // Visual feedback
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.SetTrigger("Destroy");
        }

        // Disable the camp
        enabled = false;
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }
    }

    public void ActivateCamp()
    {
        isActive = true;
        currentMana = manaCapacity;
        StartCoroutine(AIRoutine());
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Draw spawn radius if no spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                }
            }
        }
    }
}