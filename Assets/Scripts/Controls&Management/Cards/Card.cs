using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public MinionData minionData;
    public GameObject minionPrefab;
    
    [Header("UI Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Card State")]
    public int currentLevel = 1;
    public bool isInDeck = false;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Canvas canvas;

    protected virtual void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        originalPosition = transform.position;
    }

    public virtual void Initialize(MinionData data)
    {
        minionData = data;
        UpdateCardVisuals();
    }

    protected virtual void UpdateCardVisuals()
    {
        if (minionData == null) return;

        cardImage.sprite = minionData.minionSprite;
        nameText.text = minionData.minionName;
        manaText.text = minionData.manaCost.ToString();
        levelText.text = $"Lvl {currentLevel}";

        // You can add kingdom-specific frame colors here
        switch (minionData.kingdom)
        {
            case KingdomType.GreatZoey:
                cardFrame.color = Color.blue;
                break;
            case KingdomType.SvenImmortal:
                cardFrame.color = Color.red;
                break;
            case KingdomType.AzarakhshMagus:
                cardFrame.color = Color.yellow;
                break;
        }
    }

    // Drag and Drop functionality
    public virtual void OnBeginDrag()
    {
        if (!GameManager.Instance.IsInCombatMode()) return;
        
        isDragging = true;
        originalPosition = transform.position;
    }

    public virtual void OnDrag()
    {
        if (!isDragging) return;

        transform.position = Input.mousePosition;
    }

    public virtual void OnEndDrag()
    {
        if (!isDragging) return;

        isDragging = false;
        
        // Check if card is dropped in a valid position
        if (IsValidDropPosition())
        {
            OnPlay();
        }
        else
        {
            ReturnToHand();
        }
    }

    protected virtual bool IsValidDropPosition()
    {
        // Implement your logic to check if the current position is valid for spawning
        // This could check for:
        // - If we have enough mana
        // - If the position is within our spawn area
        // - If there's no obstacle at the spawn position
        return true; // Placeholder
    }

    protected virtual void OnPlay()
    {
        SpawnMinion();
    }

    protected virtual void SpawnMinion()
    {
        if (minionPrefab == null || minionData == null) return;

        // Convert screen position to world position
        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        spawnPosition.z = 0; // Ensure it's in 2D plane

        // Instantiate the minion
        GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize the minion with data
        Minion minionComponent = minion.GetComponent<Minion>();
        if (minionComponent != null)
        {
            minionComponent.Initialize(minionData, currentLevel, Team.Player);
        }

        // Return card to hand
        ReturnToHand();
    }

    protected virtual void ReturnToHand()
    {
        transform.position = originalPosition;
    }
}

[CreateAssetMenu(fileName = "New Card", menuName = "Kingdom Resist/Card")]
public class Card : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public Sprite cardArt;
    public string description;
    public int manaCost;
    public KingdomType kingdomType;

    [Header("Runtime")]
    protected float currentCooldown;
    protected bool isInCooldown;

    public bool IsInCooldown => isInCooldown;
    public float CurrentCooldown => currentCooldown;

    public virtual void Initialize()
    {
        currentCooldown = 0f;
        isInCooldown = false;
    }

    public virtual bool CanPlay(int currentMana)
    {
        return currentMana >= manaCost && !isInCooldown;
    }

    public virtual void OnPlay()
    {
        // Base implementation does nothing
    }

    public virtual void UpdateCooldown()
    {
        if (isInCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                isInCooldown = false;
                currentCooldown = 0f;
            }
        }
    }
} 