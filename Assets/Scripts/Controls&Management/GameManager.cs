using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CameraController cameraController;
    
    public enum GameScene
    {
        MainMenu,
        KingdomSelection,
        HubDefense
    }

    public enum GameMode
    {
        Hub,
        Defense
    }

    [Header("Defense Mode Settings")]
    [SerializeField] private float defenseCooldown = 10f; // 5 minutes
    [SerializeField] private float currentDefenseCooldown;
    [SerializeField] private GameObject[] enemyCampPrefabs;
    [SerializeField] private Vector2 campSpawnOffset = new Vector2(20f, 0f);

    private EnemyCamp currentEnemyCamp;
    private List<KingdomType> availableEnemyTypes = new List<KingdomType>();
    private bool isDefenseActive;
    private GameMode currentGameMode = GameMode.Hub;

    public GameMode CurrentGameMode => currentGameMode;
    public bool IsInCombatMode() => currentGameMode == GameMode.Defense;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemyTypes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (currentGameMode == GameMode.Defense && !isDefenseActive && currentDefenseCooldown > 0)
    {
        currentDefenseCooldown -= Time.deltaTime;
        if (currentDefenseCooldown <= 0)
        {
            SpawnNewEnemyCamp();
        }
    }
    }

    private void InitializeEnemyTypes()
    {
        availableEnemyTypes.Clear();
        // Add all kingdom types except the player's current kingdom
        foreach (KingdomType type in System.Enum.GetValues(typeof(KingdomType)))
        {
            if (ProfileManager.Instance == null || 
                ProfileManager.Instance.currentProfile == null || 
                type != ProfileManager.Instance.currentProfile.kingdomType)
            {
                availableEnemyTypes.Add(type);
            }
        }
    }

    public void ReinitializeEnemyTypes()
    {
        InitializeEnemyTypes();
    }

    public void LoadScene(GameScene scene)
    {
        StartCoroutine(LoadSceneRoutine(scene));
    }

    private IEnumerator LoadSceneRoutine(GameScene scene)
    {
        // Clean up current scene if needed
        if (scene == GameScene.HubDefense)
        {
            currentDefenseCooldown = defenseCooldown;
            isDefenseActive = false;
        }

        SceneManager.LoadScene(scene.ToString());
        yield return new WaitForSeconds(0.1f); // Wait for scene to load

        if (scene == GameScene.HubDefense)
        {
            InitializeDefenseMode();
        }
    }

    private void InitializeDefenseMode()
    {
        if (currentEnemyCamp == null && currentDefenseCooldown <= 0)
        {
            SpawnNewEnemyCamp();
        }
    }

    private void SpawnNewEnemyCamp()
    {
        if (availableEnemyTypes.Count == 0) return;

        // Select random enemy type
        int randomIndex = Random.Range(0, availableEnemyTypes.Count);
        KingdomType enemyType = availableEnemyTypes[randomIndex];

        // Find the corresponding prefab
        GameObject campPrefab = enemyCampPrefabs[(int)enemyType];
        if (campPrefab == null) return;

        // Find player hub position
        PlayerHub playerHub = FindObjectOfType<PlayerHub>();
        if (playerHub == null) return;

        // Spawn camp at offset from player
        Vector3 spawnPosition = playerHub.transform.position + new Vector3(campSpawnOffset.x, campSpawnOffset.y, 0);
        GameObject campObject = Instantiate(campPrefab, spawnPosition, Quaternion.identity);
        
        currentEnemyCamp = campObject.GetComponent<EnemyCamp>();
        if (currentEnemyCamp != null)
        {
            currentEnemyCamp.ActivateCamp();
            isDefenseActive = true;
            if (cameraController != null) { 
            cameraController.UpdateBounds();
            }
        }
    }

    public void OnEnemyCampDestroyed(EnemyCamp camp)
    {
        if (camp == currentEnemyCamp)
        {
            currentEnemyCamp = null;
            isDefenseActive = false;
            currentDefenseCooldown = defenseCooldown;

            // Award gold to player
            PlayerHub playerHub = FindObjectOfType<PlayerHub>();
            if (playerHub != null)
            {
                playerHub.AddGold(100); // Base reward, could be modified based on camp type/difficulty
            }
        }
    }

    public bool IsDefenseActive()
    {
        return isDefenseActive;
    }

    public float GetDefenseCooldown()
    {
        return currentDefenseCooldown;
    }

    public void ForceStartDefense()
    {
        if (currentGameMode != GameMode.Defense) return;

        currentDefenseCooldown = 0f;
        if (!isDefenseActive)
        {
            SpawnNewEnemyCamp();
        }
    }

    public void SwitchGameMode(GameMode newMode)
    {
        if (currentGameMode == newMode) return;

        currentGameMode = newMode;
        
        if (newMode == GameMode.Defense)
        {
            // Initialize defense mode
            if (currentEnemyCamp == null && currentDefenseCooldown <= 0)
            {
                SpawnNewEnemyCamp();
            }
        }
        else
        {
            // Clean up if switching to Hub mode
            if (currentEnemyCamp != null)
            {
                Destroy(currentEnemyCamp.gameObject);
                currentEnemyCamp = null;
            }
            isDefenseActive = false;
        }

        // Notify any listeners about the mode change
        OnGameModeChanged?.Invoke(newMode);
    }

    // Event for game mode changes
    public delegate void GameModeChangeHandler(GameMode newMode);
    public static event GameModeChangeHandler OnGameModeChanged;
} 