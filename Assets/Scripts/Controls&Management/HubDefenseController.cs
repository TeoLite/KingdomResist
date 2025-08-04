using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubDefenseController : MonoBehaviour
{
    [Header("Mode UI")]
    [SerializeField] private Button switchModeButton;
    [SerializeField] private Text modeText;
    [SerializeField] private GameObject defenseUI;
    [SerializeField] private GameObject hubUI;

    [Header("Defense Mode UI")]
    [SerializeField] private Text cooldownText;
    [SerializeField] private Button startDefenseButton;

    private void Start()
    {
        // Initialize UI based on current mode
        UpdateUI(GameManager.Instance.CurrentGameMode);
        
        // Subscribe to mode change events
        GameManager.OnGameModeChanged += UpdateUI;

        // Add listener to start defense button
        if (startDefenseButton != null)
        {
            startDefenseButton.onClick.AddListener(OnStartDefenseClicked);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameManager.OnGameModeChanged -= UpdateUI;

        // Remove button listener
        if (startDefenseButton != null)
        {
            startDefenseButton.onClick.RemoveListener(OnStartDefenseClicked);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Defense)
        {
            UpdateCooldownUI();
        }
    }

    public void OnSwitchModeClicked()
    {
        GameManager.GameMode newMode = GameManager.Instance.CurrentGameMode == GameManager.GameMode.Hub
            ? GameManager.GameMode.Defense
            : GameManager.GameMode.Hub;

        GameManager.Instance.SwitchGameMode(newMode);
    }

    public void OnStartDefenseClicked()
    {
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Defense)
        {
            // Switch to defense mode first if we're not in it
            GameManager.Instance.SwitchGameMode(GameManager.GameMode.Defense);
        }

        // Force the cooldown to 0 to trigger immediate spawn
        GameManager.Instance.ForceStartDefense();
        
        // Hide the start button
        startDefenseButton.gameObject.SetActive(false);
    }

    private void UpdateUI(GameManager.GameMode mode)
    {
        // Update mode text
        modeText.text = mode == GameManager.GameMode.Hub ? "Hub Mode" : "Defense Mode";

        // Show/hide appropriate UI elements
        defenseUI.SetActive(mode == GameManager.GameMode.Defense);
        hubUI.SetActive(mode == GameManager.GameMode.Hub);

        // Update defense button state
        if (mode == GameManager.GameMode.Defense)
        {
            UpdateCooldownUI();
        }
    }

    private void UpdateCooldownUI()
    {
        float cooldown = GameManager.Instance.GetDefenseCooldown();
        bool isDefenseActive = GameManager.Instance.IsDefenseActive();

        if (isDefenseActive)
        {
            cooldownText.text = "Defense in Progress";
            startDefenseButton.gameObject.SetActive(false);
        }
        else if (cooldown > 0)
        {
            int minutes = Mathf.FloorToInt(cooldown / 60);
            int seconds = Mathf.FloorToInt(cooldown % 60);
            cooldownText.text = $"Next Defense: {minutes:00}:{seconds:00}";
            startDefenseButton.gameObject.SetActive(false);
        }
        else
        {
            cooldownText.text = "Defense Ready!";
            startDefenseButton.gameObject.SetActive(true);
        }
    }
} 