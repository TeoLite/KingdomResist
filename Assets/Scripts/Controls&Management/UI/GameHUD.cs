using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText;
    
    [Header("Mana Bar")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private Text manaText;

    [Header("Enemy Stats")]
    [SerializeField] private Image enemyHealthBarFill;
    [SerializeField] private Text enemyHealthText;
    [SerializeField] private Image enemyManaBarFill;
    [SerializeField] private Text enemyManaText;

    private PlayerHub playerHub;
    private bool isInitialized = false;
    private float lastHealthValue = -1f;
    private float lastManaValue = -1f;

    private void Start()
    {
        Debug.Log("[GameHUD] Start called");
        StartCoroutine(InitializeWithDelay());
    }

    private void OnEnable()
    {
        PlayerHub.OnStatsInitialized += OnPlayerHubInitialized;
        if (GameManager.Instance != null)
        {
            GameManager.OnGameModeChanged += OnGameModeChanged;
        }
    }

    private void OnDisable()
    {
        PlayerHub.OnStatsInitialized -= OnPlayerHubInitialized;
        if (GameManager.Instance != null)
        {
            GameManager.OnGameModeChanged -= OnGameModeChanged;
        }
    }

    private void OnGameModeChanged(GameManager.GameMode newMode)
    {
        // Force UI update when game mode changes
        lastHealthValue = -1f;
        lastManaValue = -1f;
        UpdateUI();
    }

    private void OnPlayerHubInitialized()
    {
        Debug.Log("[GameHUD] OnPlayerHubInitialized called");
        StartCoroutine(WaitForValidStats());
    }

    private IEnumerator WaitForValidStats()
    {
        float timeWaited = 0f;
        float maxWaitTime = 5f;

        while (timeWaited < maxWaitTime)
        {
            if (playerHub != null && playerHub.MaxHealth > 0 && playerHub.MaxMana > 0)
            {
                isInitialized = true;
                Debug.Log($"[GameHUD] Stats are valid: Health={playerHub.CurrentHealth}/{playerHub.MaxHealth}, Mana={playerHub.CurrentMana}/{playerHub.MaxMana}");
                lastHealthValue = -1f;
                lastManaValue = -1f;
                UpdateUI();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
            timeWaited += 0.1f;
        }

        Debug.LogWarning("[GameHUD] Timed out waiting for valid stats!");
    }

    private IEnumerator InitializeWithDelay()
    {
        // Wait until we find PlayerHub
        float timeWaited = 0f;
        float maxWaitTime = 5f; // Maximum time to wait for PlayerHub

        while (playerHub == null && timeWaited < maxWaitTime)
        {
            playerHub = PlayerHub.Instance;
            if (playerHub == null)
            {
                playerHub = FindObjectOfType<PlayerHub>();
            }

            if (playerHub == null)
            {
                Debug.Log("[GameHUD] Waiting for PlayerHub to initialize...");
                yield return new WaitForSeconds(0.1f);
                timeWaited += 0.1f;
            }
        }

        if (playerHub == null)
        {
            Debug.LogError("[GameHUD] Failed to find PlayerHub after waiting!");
            enabled = false;
            yield break;
        }

        Debug.Log($"[GameHUD] Successfully found PlayerHub. Initial values: Health={playerHub.CurrentHealth}/{playerHub.MaxHealth}, Mana={playerHub.CurrentMana}/{playerHub.MaxMana}");
        StartCoroutine(WaitForValidStats());
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        if (playerHub == null)
        {
            Debug.LogWarning("[GameHUD] PlayerHub reference lost! Trying to recover...");
            playerHub = PlayerHub.Instance ?? FindObjectOfType<PlayerHub>();
            if (playerHub == null)
            {
                enabled = false;
                return;
            }
        }

        // Only update UI if values have changed
        if (playerHub.CurrentHealth != lastHealthValue || playerHub.CurrentMana != lastManaValue)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (playerHub == null) return;

        // Update health
        float healthPercent = playerHub.GetHealthPercentage();
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
        }
        if (healthText != null)
        {
            string newHealthText = $"{Mathf.CeilToInt(playerHub.CurrentHealth)}/{Mathf.CeilToInt(playerHub.MaxHealth)}";
            if (healthText.text != newHealthText)
            {
                healthText.text = newHealthText;
                Debug.Log($"[GameHUD] Updated health text: {newHealthText}");
            }
        }
        lastHealthValue = playerHub.CurrentHealth;

        // Update mana
        float manaPercent = playerHub.CurrentMana / playerHub.MaxMana;
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = manaPercent;
        }
        if (manaText != null)
        {
            string newManaText = $"{Mathf.CeilToInt(playerHub.CurrentMana)}/{Mathf.CeilToInt(playerHub.MaxMana)}";
            if (manaText.text != newManaText)
            {
                manaText.text = newManaText;
                Debug.Log($"[GameHUD] Updated mana text: {newManaText}");
            }
        }
        lastManaValue = playerHub.CurrentMana;

        // Update enemy stats if available
        // TODO: Implement enemy stats updates once we have the enemy reference
    }
} 