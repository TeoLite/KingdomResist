using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerHub : Building
{
    public static PlayerHub Instance { get; private set; }
    public static event Action<KingdomType> OnKingdomTypeChanged;
    public static event Action OnStatsInitialized;

    [Header("Hub Stats")]
    [SerializeField] private int level = 1;
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float currentMana;
    [SerializeField] private float manaRegenRate = 5f;
    [SerializeField] private int maxDeckSize = 4;
    [SerializeField] private int gold;

    [Header("Kingdom Specific")]
    [SerializeField] private SpriteRenderer kingdomSprite;
    [SerializeField] private Sprite[] kingdomSprites; // One for each kingdom type

    private List<Card> deck = new List<Card>();
    private KingdomType currentKingdomType;
    private bool isInitialized = false;

    // Public properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;

    private void Awake()
    {
        Debug.Log("[PlayerHub] Awake called");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerProfile();
        }
        else
        {
            Debug.LogWarning("[PlayerHub] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected override void Start()
    {
        Debug.Log("[PlayerHub] Start called");
        
        // Don't call base.Start() since we handle health initialization in LoadPlayerProfile
        
        // Initialize values if not already set
        if (currentMana <= 0)
        {
            currentMana = maxMana;
        }
        
        UpdateKingdomAppearance();
        isInitialized = true;
        NotifyKingdomTypeChanged();

        // Log initial values
        Debug.Log($"[PlayerHub] Initialized with Health={currentHealth}/{maxHealth}, Mana={currentMana}/{maxMana}");
        
        // Notify other systems that stats are initialized
        OnStatsInitialized?.Invoke();
    }

    private void LoadPlayerProfile()
    {
        Debug.Log("[PlayerHub] Loading player profile...");
        var profile = ProfileManager.Instance?.currentProfile;
        if (profile != null)
        {
            level = profile.level;
            maxHealth = profile.maxHealth;
            currentHealth = profile.maxHealth; // Set current health to max when loading profile
            maxMana = profile.maxMana;
            currentMana = profile.maxMana; // Set current mana to max when loading profile
            manaRegenRate = profile.manaRegen;
            defense = profile.defense;
            maxDeckSize = profile.maxDeckSize;
            gold = profile.gold;
            
            // Set kingdom type
            SetKingdomType((KingdomType)profile.kingdomType);
            
            Debug.Log($"[PlayerHub] Profile loaded: Health={currentHealth}/{maxHealth}, Mana={currentMana}/{maxMana}");
        }
        else
        {
            Debug.LogWarning("[PlayerHub] No profile found, using default values");
            // Set default values
            maxHealth = 100f;
            currentHealth = maxHealth;
            maxMana = 50f;
            currentMana = maxMana;
            manaRegenRate = 1f;
            defense = 10f;
            maxDeckSize = 4;
            currentKingdomType = KingdomType.GreatZoey;
            
            // Create a new profile with these values
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.CreateProfile("Profile 1");
                var newProfile = ProfileManager.Instance.currentProfile;
                if (newProfile != null)
                {
                    newProfile.maxHealth = maxHealth;
                    newProfile.maxMana = maxMana;
                    newProfile.manaRegen = manaRegenRate;
                    newProfile.defense = defense;
                    newProfile.maxDeckSize = maxDeckSize;
                    newProfile.kingdomType = KingdomType.GreatZoey;
                    ProfileManager.Instance.SaveProfiles();
                }
            }
        }
    }

    private void SetKingdomType(KingdomType newType)
    {
        if (currentKingdomType != newType)
        {
            currentKingdomType = newType;
            NotifyKingdomTypeChanged();
        }
    }

    private void NotifyKingdomTypeChanged()
    {
        Debug.Log($"[PlayerHub] Notifying kingdom type changed to: {currentKingdomType}");
        OnKingdomTypeChanged?.Invoke(currentKingdomType);
    }

    private void UpdateKingdomAppearance()
    {
        Debug.Log($"[PlayerHub] Updating kingdom appearance for type: {currentKingdomType}");
        if (kingdomSprite == null)
        {
            Debug.LogError("[PlayerHub] Kingdom sprite renderer is null!");
            return;
        }

        int spriteIndex = (int)currentKingdomType;
        if (kingdomSprites == null || kingdomSprites.Length == 0)
        {
            Debug.LogError("[PlayerHub] Kingdom sprites array is null or empty!");
            return;
        }

        if (spriteIndex < kingdomSprites.Length && kingdomSprites[spriteIndex] != null)
        {
            kingdomSprite.sprite = kingdomSprites[spriteIndex];
            Debug.Log("[PlayerHub] Successfully updated kingdom appearance");
        }
        else
        {
            Debug.LogError($"[PlayerHub] Missing sprite for kingdom type {currentKingdomType}");
        }
    }

    public KingdomType GetKingdomType()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerHub] GetKingdomType called before initialization! Current type: " + currentKingdomType);
        }
        return currentKingdomType;
    }

    private void Update()
    {
        // Only regenerate mana in Defense mode
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameManager.GameMode.Defense)
        {
            RegenerateMana();
        }
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            float oldMana = currentMana;
            currentMana = Mathf.Min(maxMana, currentMana + (manaRegenRate * Time.deltaTime));
            
            // Log mana changes for debugging
            if (Mathf.FloorToInt(oldMana) != Mathf.FloorToInt(currentMana))
            {
                Debug.Log($"[PlayerHub] Mana regenerated: {oldMana:F1} -> {currentMana:F1}");
            }
        }
    }

    public bool TrySpendMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            Debug.Log($"[PlayerHub] Spent {amount} mana. Remaining: {currentMana}");
            return true;
        }
        Debug.Log($"[PlayerHub] Not enough mana! Required: {amount}, Current: {currentMana}");
        return false;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        var profile = ProfileManager.Instance.currentProfile;
        if (profile != null)
        {
            profile.gold = gold;
            ProfileManager.Instance.SaveProfiles();
        }
    }

    public bool TrySpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            var profile = ProfileManager.Instance.currentProfile;
            if (profile != null)
            {
                profile.gold = gold;
                ProfileManager.Instance.SaveProfiles();
            }
            return true;
        }
        return false;
    }

    public void AddExperience(int amount)
    {
        // TODO: Implement leveling system
        level++;
        UpdateStats();
    }

    private void UpdateStats()
    {
        // Update stats based on level
        maxHealth = 100f + (level - 1) * 20f;
        maxMana = 100f + (level - 1) * 10f;
        manaRegenRate = 1f + (level - 1) * 0.1f;
        defense = 10f + (level - 1) * 2f;
        maxDeckSize = 4 + Mathf.FloorToInt((level - 1) / 5);

        // Update profile
        var profile = ProfileManager.Instance.currentProfile;
        if (profile != null)
        {
            profile.level = level;
            profile.maxHealth = maxHealth;
            profile.maxMana = maxMana;
            profile.manaRegen = manaRegenRate;
            profile.defense = defense;
            profile.maxDeckSize = maxDeckSize;
            ProfileManager.Instance.SaveProfiles();
        }
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        // Handle game over or respawn logic
    }
} 