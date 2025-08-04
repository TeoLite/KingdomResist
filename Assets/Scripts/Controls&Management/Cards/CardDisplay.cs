using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("Card Data")]
    public Card cardData;
    
    
    [Header("UI Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Text nameText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text levelText;
    
    [Header("Card State")]
    public int currentLevel = 1;
    public bool isInDeck = false;
    protected PlayerHand playerHand;

    protected virtual void Awake()
    {
        // Ensure UI components are properly referenced
        if (cardImage == null) cardImage = transform.Find("CardImage")?.GetComponent<Image>();
        if (cardFrame == null) cardFrame = transform.Find("CardFrame")?.GetComponent<Image>();
        if (nameText == null) nameText = transform.Find("NameText")?.GetComponent<Text>();
        if (manaText == null) manaText = transform.Find("ManaText")?.GetComponent<Text>();
        if (levelText == null) levelText = transform.Find("LevelText")?.GetComponent<Text>();
    }

    public void SetPlayerHand(PlayerHand hand)
    {
        playerHand = hand;
        if (playerHand == null)
        {
            Debug.LogError($"[{gameObject.name}] Setting null PlayerHand reference!");
        }
    }

    public virtual void Initialize(Card data)
    {
        cardData = data;
        if (cardData == null)
        {
            Debug.LogError($"[{gameObject.name}] Trying to initialize with null card data!");
            return;
        }
        Debug.Log($"Initializing card: {cardData.cardName} with art: {cardData.cardArt != null}");
        UpdateCardVisuals();
    }

    protected virtual void UpdateCardVisuals()
    {
        if (cardData == null)
        {
            Debug.LogError($"[{gameObject.name}] No card data assigned!");
            return;
        }

        if (cardImage == null)
        {
            Debug.LogError($"[{gameObject.name}] No card image component found!");
            return;
        }

        if (cardData.cardArt == null)
        {
            Debug.LogError($"[{gameObject.name}] No card art assigned for card {cardData.cardName}!");
            return;
        }

        cardImage.sprite = cardData.cardArt;
        cardImage.preserveAspect = true;
        
        if (nameText != null) nameText.text = cardData.cardName;
        if (manaText != null) manaText.text = cardData.manaCost.ToString();
        if (levelText != null) levelText.text = $"Lvl {currentLevel}";
        
        Debug.Log($"Updated card visuals for {cardData.cardName}. Image assigned: {cardImage.sprite != null}");
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // Check for null references
        if (GameManager.Instance == null)
        {
            Debug.LogError($"[{gameObject.name}] GameManager.Instance is null!");
            return;
        }

        if (playerHand == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerHand reference is null!");
            return;
        }

        if (cardData == null)
        {
            Debug.LogError($"[{gameObject.name}] CardData is null!");
            return;
        }

        // Check game state
        if (!GameManager.Instance.IsInCombatMode())
        {
            Debug.Log($"[{gameObject.name}] Cannot play cards in non-combat mode!");
            return;
        }

        // Check if we can play the card
        if (!playerHand.CanPlayCard(cardData))
        {
            Debug.Log($"[{gameObject.name}] Cannot play card: {cardData.cardName} (insufficient mana or on cooldown)");
            return;
        }

        Debug.Log($"[{gameObject.name}] Playing card: {cardData.cardName} (Mana Cost: {cardData.manaCost})");
        
        // Play the card
        OnPlay();
        playerHand.OnCardPlayed(this);
    }

    protected virtual void OnPlay()
    {
        if (cardData != null)
        {
            Debug.Log($"[{gameObject.name}] Executing card effect for {cardData.cardName}");
            cardData.OnPlay();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Trying to play card but cardData is null!");
        }
    }
} 