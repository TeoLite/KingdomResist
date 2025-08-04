using UnityEngine;
using UnityEngine.UI;

public class EnemyHubUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText;
    [SerializeField] private Image manaBarFill;
    [SerializeField] private Text manaText;

    private EnemyHub enemyHub;
    private bool isInitialized = false;
    private float lastHealthValue = -1f;
    private float lastManaValue = -1f;

    private void Start()
    {
        StartCoroutine(InitializeWithDelay());
    }

    private System.Collections.IEnumerator InitializeWithDelay()
    {
        // Wait until we find EnemyHub
        float timeWaited = 0f;
        float maxWaitTime = 5f;

        while (enemyHub == null && timeWaited < maxWaitTime)
        {
            enemyHub = FindObjectOfType<EnemyHub>();
            if (enemyHub == null)
            {
                yield return new WaitForSeconds(0.1f);
                timeWaited += 0.1f;
            }
        }

        if (enemyHub == null)
        {
            Debug.LogError("[EnemyHubUI] Failed to find EnemyHub!");
            enabled = false;
            yield break;
        }

        isInitialized = true;
        Debug.Log($"[EnemyHubUI] Successfully found EnemyHub. Initial values: Health={enemyHub.CurrentHealth}/{enemyHub.MaxHealth}, Mana={enemyHub.CurrentMana}/{enemyHub.MaxMana}");
        
        // Force first update
        UpdateUI();
    }

    private void Update()
    {
        if (!isInitialized || enemyHub == null)
            return;

        // Only update UI if values have changed
        if (enemyHub.CurrentHealth != lastHealthValue || enemyHub.CurrentMana != lastManaValue)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (enemyHub == null) return;

        // Update health
        float healthPercent = enemyHub.CurrentHealth / enemyHub.MaxHealth;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
        }
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(enemyHub.CurrentHealth)}/{Mathf.CeilToInt(enemyHub.MaxHealth)}";
        }
        lastHealthValue = enemyHub.CurrentHealth;

        // Update mana
        float manaPercent = enemyHub.CurrentMana / enemyHub.MaxMana;
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = manaPercent;
        }
        if (manaText != null)
        {
            manaText.text = $"{Mathf.CeilToInt(enemyHub.CurrentMana)}/{Mathf.CeilToInt(enemyHub.MaxMana)}";
        }
        lastManaValue = enemyHub.CurrentMana;
    }

    // Call this method when the enemy hub is destroyed
    public void OnEnemyHubDestroyed()
    {
        enemyHub = null;
        isInitialized = false;
        
        // Reset UI
        if (healthBarFill != null) healthBarFill.fillAmount = 0;
        if (manaBarFill != null) manaBarFill.fillAmount = 0;
        if (healthText != null) healthText.text = "0/0";
        if (manaText != null) manaText.text = "0/0";
    }
} 