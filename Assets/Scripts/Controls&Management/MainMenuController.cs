using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pressAnyKeyPanel;
    [SerializeField] private GameObject profileSelectionPanel;
    [SerializeField] private GameObject[] profileButtons;
    [SerializeField] private Text[] profileNames;
    [SerializeField] private Text[] kingdomTypes;
    [SerializeField] private Text[] playTimes;
    [SerializeField] private Text[] loadNewTexts;
    [SerializeField] private Button[] deleteProfileButtons;
    [SerializeField] private Button clearAllProfilesButton;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Text confirmationText;

    private bool isWaitingForInput = true;
    private int profileToDelete = -1;
    private bool isConfirmingClearAll = false;

    private void Start()
    {
        profileSelectionPanel.SetActive(false);
        pressAnyKeyPanel.SetActive(true);
        confirmationDialog.SetActive(false);
        UpdateProfileUI();
    }

    private void Update()
    {
        if (isWaitingForInput && Input.anyKeyDown)
        {
            ShowProfileSelection();
        }
    }

    private void ShowProfileSelection()
    {
        isWaitingForInput = false;
        pressAnyKeyPanel.SetActive(false);
        profileSelectionPanel.SetActive(true);
    }

    private void UpdateProfileUI()
    {
        var profilesList = ProfileManager.Instance.ProfilesList;

        // Update profile buttons and info
        for (int i = 0; i < profileButtons.Length; i++)
        {
            if (i < profilesList.Count)
            {
                var profile = profilesList[i];
                profileNames[i].text = profile.profileName;
                kingdomTypes[i].text = profile.kingdomType.ToString();
                playTimes[i].text = FormatPlayTime(profile.playTime);
                loadNewTexts[i].text = profile.isNewProfile ? "New" : "Load";
                deleteProfileButtons[i].gameObject.SetActive(true);
            }
            else
            {
                profileNames[i].text = "Empty Slot";
                kingdomTypes[i].text = "-";
                playTimes[i].text = "-";
                loadNewTexts[i].text = "New";
                deleteProfileButtons[i].gameObject.SetActive(false);
            }
        }

        // Update clear all button
        clearAllProfilesButton.gameObject.SetActive(profilesList.Count > 0);
    }

    public void OnProfileSelected(int index)
    {
        var profilesList = ProfileManager.Instance.ProfilesList;
        
        if (index >= profilesList.Count)
        {
            // Create new profile
            string profileName = $"Profile {index + 1}";
            ProfileManager.Instance.CreateProfile(profileName);
            GameManager.Instance.LoadScene(GameManager.GameScene.KingdomSelection);
        }
        else
        {
            // Load existing profile
            var profile = profilesList[index];
            ProfileManager.Instance.SelectProfile(profile.profileName);
            if (profile.isNewProfile)
            {
                GameManager.Instance.LoadScene(GameManager.GameScene.KingdomSelection);
            }
            else
            {
                GameManager.Instance.LoadScene(GameManager.GameScene.HubDefense);
            }
        }
    }

    public void OnDeleteProfileClicked(int index)
    {
        profileToDelete = index;
        isConfirmingClearAll = false;
        confirmationText.text = $"Are you sure you want to delete Profile {index + 1}?";
        confirmationDialog.SetActive(true);
    }

    public void OnClearAllProfilesClicked()
    {
        isConfirmingClearAll = true;
        profileToDelete = -1;
        confirmationText.text = "Are you sure you want to delete all profiles?";
        confirmationDialog.SetActive(true);
    }

    public void OnConfirmationYes()
    {
        if (isConfirmingClearAll)
        {
            ProfileManager.Instance.ClearAllProfiles();
        }
        else if (profileToDelete >= 0)
        {
            var profilesList = ProfileManager.Instance.ProfilesList;
            if (profileToDelete < profilesList.Count)
            {
                ProfileManager.Instance.DeleteProfile(profilesList[profileToDelete].profileName);
            }
        }

        confirmationDialog.SetActive(false);
        UpdateProfileUI();
    }

    public void OnConfirmationNo()
    {
        confirmationDialog.SetActive(false);
        profileToDelete = -1;
        isConfirmingClearAll = false;
    }

    private string FormatPlayTime(float playTimeInSeconds)
    {
        int hours = Mathf.FloorToInt(playTimeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((playTimeInSeconds % 3600) / 60);
        return $"{hours}h {minutes}m";
    }
} 