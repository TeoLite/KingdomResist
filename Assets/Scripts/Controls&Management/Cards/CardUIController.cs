using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Header("Card Display Settings")]
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private float cardWidth = 100f;
    [SerializeField] private Color cooldownTint = new Color(0.5f, 0.5f, 0.5f, 1f);

    private List<CardDisplay> cardDisplays = new List<CardDisplay>();
    private DeckManager deckManager;
    private PlayerHub playerHub;
    private bool isInitialized = false;

    private void OnEnable()
    {
        PlayerHub.OnStatsInitialized += OnPlayerHubInitialized;
    }

    private void OnDisable()
    {
        PlayerHub.OnStatsInitialized -= OnPlayerHubInitialized;
    }

    private void Start()
    {
        deckManager = DeckManager.Instance;
        if (deckManager == null)
        {
            Debug.LogError("DeckManager not found!");
            return;
        }

        playerHub = PlayerHub.Instance;
        if (playerHub == null)
        {
            Debug.LogError("PlayerHub not found!");
            return;
        }

        // Don't initialize UI yet, wait for PlayerHub stats to be initialized
        Debug.Log("[CardUIController] Waiting for PlayerHub stats to be initialized...");
    }

    private void OnPlayerHubInitialized()
    {
        if (!isInitialized && playerHub != null && deckManager != null)
        {
            isInitialized = true;
            Debug.Log("[CardUIController] PlayerHub stats initialized, initializing card UI...");
            InitializeCardUI();
        }
    }

    private void Update()
    {
        if (!isInitialized) return;
        
        UpdateManaDisplay();
        UpdateCardStates();
    }

    private void InitializeCardUI()
    {
        // Clear existing card UIs
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        cardDisplays.Clear();

        // Create UI for each card in deck
        for (int i = 0; i < deckManager.CurrentDeck.Count; i++)
        {
            Card card = deckManager.CurrentDeck[i];
            GameObject cardObj = Instantiate(cardDisplayPrefab, cardContainer);
            CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
            
            if (cardDisplay != null)
            {
                cardDisplay.Initialize(card);
                cardDisplays.Add(cardDisplay);

                // Position the card
                RectTransform rt = cardObj.GetComponent<RectTransform>();
                float xPos = i * (cardWidth + cardSpacing) - (deckManager.CurrentDeck.Count - 1) * (cardWidth + cardSpacing) / 2f;
                rt.anchoredPosition = new Vector2(xPos, 0);
            }
        }
    }

    private void UpdateManaDisplay()
    {
        if (manaText != null && playerHub != null)
        {
            manaText.text = $"Mana: {Mathf.FloorToInt(playerHub.CurrentMana)}/{Mathf.FloorToInt(playerHub.MaxMana)}";
        }
    }

    private void UpdateCardStates()
    {
        foreach (CardDisplay cardDisplay in cardDisplays)
        {
            bool canPlay = deckManager.CanPlayCard(cardDisplay.cardData);
            Image cardImage = cardDisplay.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = canPlay ? Color.white : cooldownTint;
            }
        }
    }

    public void RefreshDeck()
    {
        if (isInitialized)
        {
            InitializeCardUI();
        }
    }
}