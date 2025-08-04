using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Deck Settings")]
    [SerializeField] private int maxDeckSize = 8;
    [SerializeField] private List<Card> availableCards = new List<Card>();
    [SerializeField] private List<Card> currentDeck = new List<Card>();

    private bool isInitialized = false;
    private PlayerHub playerHub;

    public List<Card> CurrentDeck => currentDeck;
    public List<Card> AvailableCards => availableCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDeck();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // If we don't have a current deck but have available cards, create an initial deck
        if (!isInitialized && currentDeck.Count == 0 && availableCards.Count > 0)
        {
            CreateInitialDeck();
        }
    }

    private void Update()
    {
        // Only update in combat modes
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Hub)
        {
            // Update card cooldowns
            foreach (Card card in currentDeck.ToList()) // Use ToList to avoid modification during enumeration
            {
                card.UpdateCooldown();
            }
        }
    }

    private void InitializeDeck()
    {
        if (isInitialized) return;

        // Get PlayerHub reference
        playerHub = PlayerHub.Instance;
        if (playerHub == null)
        {
            Debug.LogError("[DeckManager] PlayerHub not found!");
            return;
        }

        // Initialize all cards
        foreach (Card card in availableCards)
        {
            card.Initialize();
        }

        // If we have cards in current deck, initialize them
        if (currentDeck.Count > 0)
        {
            foreach (Card card in currentDeck)
            {
                card.Initialize();
            }
        }
        else
        {
            // If no current deck, create initial deck
            CreateInitialDeck();
        }
        
        isInitialized = true;
        Debug.Log($"DeckManager initialized with {currentDeck.Count} cards in current deck");
    }

    private void CreateInitialDeck()
    {
        // Clear current deck first
        currentDeck.Clear();

        // Add cards from available cards up to maxDeckSize
        foreach (Card card in availableCards)
        {
            if (currentDeck.Count >= maxDeckSize) break;
            
            // Create a copy of the card to avoid reference issues
            Card cardCopy = Instantiate(card);
            cardCopy.Initialize();
            currentDeck.Add(cardCopy);
        }

        Debug.Log($"Created initial deck with {currentDeck.Count} cards");
    }

    public bool AddCardToDeck(Card card)
    {
        if (currentDeck.Count >= maxDeckSize)
            return false;

        if (!availableCards.Contains(card))
            return false;

        // Create a copy of the card
        Card cardCopy = Instantiate(card);
        cardCopy.Initialize();
        currentDeck.Add(cardCopy);
        return true;
    }

    public bool RemoveCardFromDeck(Card card)
    {
        if (currentDeck.Contains(card))
        {
            bool removed = currentDeck.Remove(card);
            if (removed)
            {
                Destroy(card); // Clean up the card instance
            }
            return removed;
        }
        return false;
    }

    public bool CanPlayCard(Card card)
    {
        if (!currentDeck.Contains(card) || playerHub == null)
            return false;

        return card.CanPlay(Mathf.FloorToInt(playerHub.CurrentMana));
    }

    public bool PlayCard(Card card)
    {
        if (!CanPlayCard(card))
            return false;

        if (playerHub.TrySpendMana(card.manaCost))
        {
            card.OnPlay();
            return true;
        }
        return false;
    }

    public void ClearDeck()
    {
        // Clean up card instances
        foreach (Card card in currentDeck)
        {
            Destroy(card);
        }
        currentDeck.Clear();
    }

    public void LoadDeck(List<Card> deck)
    {
        if (deck.Count <= maxDeckSize)
        {
            // Clean up existing cards
            ClearDeck();

            // Create copies of the new cards
            foreach (Card card in deck)
            {
                Card cardCopy = Instantiate(card);
                cardCopy.Initialize();
                currentDeck.Add(cardCopy);
            }
        }
    }

    public List<Card> GetAvailableCardsByKingdom(KingdomType kingdomType)
    {
        return availableCards.Where(card => card.kingdomType == kingdomType).ToList();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
} 