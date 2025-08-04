using UnityEngine;
using UnityEngine.EventSystems;

public class MinionCardDisplay : CardDisplay
{
    [Header("Spawn Settings")]
    public animatioPlayer_SpalshLight_Script animationPlay;
    [SerializeField] private float spawnDistanceFromHub = 2f;
    

    protected override void OnPlay()
    {
        base.OnPlay();
        
        // Check if this is a minion card
        if (cardData is MinionCard minionCard && minionCard.minionData != null)
        {
            SpawnMinion(minionCard.minionData);
        }
    }

    private void SpawnMinion(MinionData minionData)
    {
        if (minionData.minionPrefab == null)
        {
            Debug.LogError($"No minion prefab assigned for {minionData.minionName}!");
            return;
        }

        // Get the player's hub position as reference
        PlayerHub playerHub = Object.FindObjectOfType<PlayerHub>();
        if (playerHub == null)
        {
            Debug.LogError("Could not find PlayerHub in the scene!");
            return;
        }

        // Calculate spawn position to the right of the hub
        Vector3 spawnPosition = playerHub.transform.position + Vector3.right * spawnDistanceFromHub;
        
        // Instantiate the minion
        GameObject minion = Instantiate(minionData.minionPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize the minion with data
        if (minion.TryGetComponent<Minion>(out var minionComponent))
        {
            minionComponent.Initialize(minionData, currentLevel, Team.Player);
            Debug.Log($"Spawned minion {minionData.minionName} at {spawnPosition}");
        }
    }
} 