using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KingdomSelectionController : MonoBehaviour
{
    [System.Serializable]
    public class KingdomButton
    {
        public Button button;
        public KingdomType kingdomType;
        public TextMeshProUGUI kingdomNameText;  // Optional: for displaying kingdom name
    }

    [SerializeField] private KingdomButton[] kingdomButtons;
    [SerializeField] private GameObject errorMessage;  // Optional: UI element to show when buttons aren't set up

    private void Start()
    {
        if (kingdomButtons == null || kingdomButtons.Length == 0)
        {
            Debug.LogError("Kingdom buttons array is not set up in the inspector!");
            if (errorMessage != null) errorMessage.SetActive(true);
            return;
        }

        // Initialize each kingdom button
        foreach (var kingdomButton in kingdomButtons)
        {
            if (kingdomButton.button == null)
            {
                Debug.LogError($"Button for kingdom type {kingdomButton.kingdomType} is not assigned!");
                continue;
            }

            // Set up button text if available
            if (kingdomButton.kingdomNameText != null)
            {
                kingdomButton.kingdomNameText.text = kingdomButton.kingdomType.ToString().Replace("_", " ");
            }

            kingdomButton.button.onClick.RemoveAllListeners();  // Clear any existing listeners
            kingdomButton.button.onClick.AddListener(() => OnKingdomSelected(kingdomButton.kingdomType));
        }
    }

    private void OnKingdomSelected(KingdomType kingdomType)
    {
        if (ProfileManager.Instance == null)
        {
            Debug.LogError("ProfileManager instance is null!");
            return;
        }

        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.kingdomType = kingdomType;
            ProfileManager.Instance.currentProfile.isNewProfile = false;
            ProfileManager.Instance.SaveProfiles();
            
            // Make sure GameManager exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReinitializeEnemyTypes();  // Reinitialize enemy types with new kingdom
                GameManager.Instance.LoadScene(GameManager.GameScene.HubDefense);
            }
            else
            {
                Debug.LogError("GameManager instance is null!");
            }
        }
        else
        {
            Debug.LogError("Current profile is null!");
        }
    }

    private void OnValidate()
    {
        // Ensure we have exactly one button for each playable kingdom type
        if (kingdomButtons != null && kingdomButtons.Length != 3) // Three playable kingdoms
        {
            Debug.LogError("Must have exactly three kingdom buttons!");
        }
    }
} 