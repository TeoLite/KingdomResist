using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerHand : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] private int maxHandSize = 4;
    [SerializeField] private Transform handContainer;
    [SerializeField] private GameObject cardDisplayPrefab;
    
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private float cardWidth = 100f;
    [SerializeField] private float cardHeight = 150f;
    [SerializeField] private float cardAngle = 5f; // Angle between cards for curved layout
    [SerializeField] private float curveRadius = 800f; // Radius of the curve for card layout
    
    private List<CardDisplay> cardsInHand = new List<CardDisplay>();
    private DeckManager deckManager;
    private List<Card> availableCards = new List<Card>(); // Track available cards separately

    private void Start()
    {
        deckManager = DeckManager.Instance;
        if (deckManager == null)
        {
            Debug.LogError("DeckManager not found!");
            return;
        }

        // Subscribe to game mode changes using the static event
        GameManager.OnGameModeChanged += HandleGameModeChanged;
        
        // Set initial state based on current game mode
        HandleGameModeChanged(GameManager.Instance.CurrentGameMode);
    }

    private void OnDestroy()
    {
        // Unsubscribe from game mode changes to prevent memory leaks
        GameManager.OnGameModeChanged -= HandleGameModeChanged;
    }

    private void HandleGameModeChanged(GameManager.GameMode newMode)
    {
        if (handContainer != null)
        {
            // Only show hand in Defense mode
            bool shouldShowHand = newMode == GameManager.GameMode.Defense;
            
            handContainer.gameObject.SetActive(shouldShowHand);
            
            if (shouldShowHand)
            {
                RefreshAvailableCards();
                DrawInitialHand();
            }
            else
            {
                // Clear hand when switching to non-combat modes
                ClearHand();
            }
            
            Debug.Log($"Hand container visibility set to {shouldShowHand} for game mode: {newMode}");
        }
        else
        {
            Debug.LogError("Hand container reference is missing!");
        }
    }

    private void ClearHand()
    {
        foreach (var cardDisplay in cardsInHand)
        {
            if (cardDisplay != null && cardDisplay.gameObject != null)
            {
                Destroy(cardDisplay.gameObject);
            }
        }
        cardsInHand.Clear();
    }

    private void RefreshAvailableCards()
    {
        availableCards.Clear();
        foreach (Card card in deckManager.CurrentDeck)
        {
            availableCards.Add(card);
        }
        Debug.Log($"Refreshed available cards. Count: {availableCards.Count}");
    }

    private void DrawInitialHand()
    {
        // Draw up to max hand size
        while (cardsInHand.Count < maxHandSize && availableCards.Count > 0)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("Cannot draw card: Hand is full");
            return;
        }

        if (availableCards.Count == 0)
        {
            Debug.Log("Cannot draw card: No cards available");
            if (deckManager.CurrentDeck.Count > 0)
            {
                RefreshAvailableCards();
            }
            else
            {
                return;
            }
        }

        // Get a random card from available cards
        int randomIndex = Random.Range(0, availableCards.Count);
        Card cardToDraw = availableCards[randomIndex];
        
        // Remove the card from available cards
        availableCards.RemoveAt(randomIndex);
        
        Debug.Log($"Drawing card: {cardToDraw.cardName}. Available cards remaining: {availableCards.Count}");

        // Create card display
        GameObject cardObj = Instantiate(cardDisplayPrefab, handContainer);
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        
        if (cardDisplay != null)
        {
            // Set the PlayerHand reference before initializing
            cardDisplay.SetPlayerHand(this);
            cardDisplay.Initialize(cardToDraw);
            cardsInHand.Add(cardDisplay);
            
            // Update hand layout
            UpdateHandLayout();
        }
        else
        {
            Debug.LogError("Failed to get CardDisplay component from prefab!");
        }
    }

    public void RemoveCard(CardDisplay card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            // Add the card back to available cards
            if (card.cardData != null)
            {
                availableCards.Add(card.cardData);
                Debug.Log($"Card {card.cardData.cardName} returned to available cards. Count: {availableCards.Count}");
            }
            UpdateHandLayout();
        }
    }

    private void UpdateHandLayout()
    {
        if (cardsInHand.Count == 0) return;

        // Special case for single card
        if (cardsInHand.Count == 1)
        {
            CardDisplay card = cardsInHand[0];
            RectTransform rt = card.GetComponent<RectTransform>();
            
            // Center the card with no rotation
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
            return;
        }

        float totalWidth = (cardsInHand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CardDisplay card = cardsInHand[i];
            RectTransform rt = card.GetComponent<RectTransform>();
            
            // Calculate position on curve
            float t = (float)i / (cardsInHand.Count - 1);
            float angle = Mathf.Lerp(-cardAngle * (cardsInHand.Count - 1) / 2, 
                                   cardAngle * (cardsInHand.Count - 1) / 2, 
                                   t);
            
            // Convert angle to radians
            float angleRad = angle * Mathf.Deg2Rad;
            
            // Calculate position on curve
            float xPos = startX + i * cardSpacing;
            float yPos = -Mathf.Cos(angleRad) * curveRadius + curveRadius;
            
            // Set position and rotation
            rt.anchoredPosition = new Vector2(xPos, yPos);
            rt.localRotation = Quaternion.Euler(0, 0, -angle);
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
        }
    }

    public void OnCardPlayed(CardDisplay card)
    {
        if (card == null || card.cardData == null)
        {
            Debug.LogError("Trying to play null card!");
            return;
        }

        // Try to spend mana
        PlayerHub playerHub = PlayerHub.Instance;
        if (playerHub == null)
        {
            Debug.LogError("PlayerHub instance not found!");
            return;
        }

        if (playerHub.TrySpendMana(card.cardData.manaCost))
        {
            // Remove the card from hand
            if (cardsInHand.Contains(card))
            {
                cardsInHand.Remove(card);
                Destroy(card.gameObject);
                
                // Update layout
                UpdateHandLayout();
                
                // Draw a new card
                DrawCard();
            }
        }
        else
        {
            Debug.Log($"Not enough mana to play {card.cardData.cardName}!");
        }
    }

    public bool CanPlayCard(Card card)
    {
        if (card == null) return false;
        
        // Check if we have enough mana
        PlayerHub playerHub = PlayerHub.Instance;
        if (playerHub == null)
        {
            Debug.LogError("PlayerHub instance not found!");
            return false;
        }

        return playerHub.CurrentMana >= card.manaCost;
    }
} 