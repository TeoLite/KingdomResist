using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class PlayerProfile
{
    public string profileName;
    public bool isNewProfile = true;
    public KingdomType kingdomType;
    public int level = 1;
    public float maxHealth = 100f;
    public float maxMana = 50f;
    public float manaRegen = 1f;
    public float defense = 10f;
    public int maxDeckSize = 4;
    public int gold = 0;
    public List<string> unlockedCards = new List<string>();
    public List<string> currentDeck = new List<string>();
    public float playTime = 0f;
}

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }
    public PlayerProfile currentProfile { get; private set; }

    private const string PROFILES_KEY = "PlayerProfiles";
    private const string CURRENT_PROFILE_KEY = "CurrentProfile";
    private Dictionary<string, PlayerProfile> profiles = new Dictionary<string, PlayerProfile>();
    
    // Public property to access profiles as a list
    public List<PlayerProfile> ProfilesList
    {
        get { return new List<PlayerProfile>(profiles.Values); }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProfiles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateProfile(string profileName)
    {
        if (!profiles.ContainsKey(profileName))
        {
            var newProfile = new PlayerProfile
            {
                profileName = profileName,
                isNewProfile = true,
                kingdomType = KingdomType.GreatZoey, // Default kingdom
                level = 1,
                maxHealth = 100f,
                maxMana = 50f,
                manaRegen = 1f,
                defense = 10f,
                maxDeckSize = 4,
                gold = 0,
                playTime = 0f
            };

            profiles.Add(profileName, newProfile);
            currentProfile = newProfile;
            SaveProfiles();
        }
    }

    public void LoadProfiles()
    {
        string json = PlayerPrefs.GetString(PROFILES_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            var loadedProfiles = JsonUtility.FromJson<Dictionary<string, PlayerProfile>>(json);
            if (loadedProfiles != null)
            {
                profiles = loadedProfiles;
                string currentProfileName = PlayerPrefs.GetString(CURRENT_PROFILE_KEY, "");
                if (!string.IsNullOrEmpty(currentProfileName) && profiles.ContainsKey(currentProfileName))
                {
                    currentProfile = profiles[currentProfileName];
                }
            }
        }

        // If no profiles exist, create a default one
        if (profiles.Count == 0)
        {
            CreateProfile("Profile 1");
        }
    }

    public void SaveProfiles()
    {
        string json = JsonUtility.ToJson(profiles);
        PlayerPrefs.SetString(PROFILES_KEY, json);
        if (currentProfile != null)
        {
            PlayerPrefs.SetString(CURRENT_PROFILE_KEY, currentProfile.profileName);
        }
        PlayerPrefs.Save();
    }

    public void SelectProfile(string profileName)
    {
        if (profiles.ContainsKey(profileName))
        {
            currentProfile = profiles[profileName];
            SaveProfiles();
        }
    }

    public void DeleteProfile(string profileName)
    {
        if (profiles.ContainsKey(profileName))
        {
            if (currentProfile != null && currentProfile.profileName == profileName)
            {
                currentProfile = null;
            }
            profiles.Remove(profileName);
            SaveProfiles();
        }
    }

    public void ClearAllProfiles()
    {
        profiles.Clear();
        currentProfile = null;
        SaveProfiles();
    }
} 