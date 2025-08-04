using UnityEngine;

[CreateAssetMenu(fileName = "New Minion Card", menuName = "Kingdom Resist/Cards/Minion")]
public class MinionCard : Card
{
    [Header("Minion Data")]
    public MinionData minionData;
    
    [SerializeField] public Vector2 spawnOffset = new Vector2(1f, 0f);

    public override void OnPlay()
    {
        base.OnPlay();
        
        // Get the player's hub position as reference
        PlayerHub playerHub = Object.FindObjectOfType<PlayerHub>();
        if (playerHub == null) return;

        // Calculate spawn position relative to the hub
        Vector3 spawnPosition = playerHub.transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);
        
        // Instantiate the minion
        if (minionData != null && minionData.minionPrefab != null)
        {
            GameObject minion = Object.Instantiate(minionData.minionPrefab, spawnPosition, Quaternion.identity);
            
            // Initialize minion stats
            if (minion.TryGetComponent<Minion>(out var minionComponent))
            {
                minionComponent.Initialize(minionData, 1, Team.Player); // Player's minions
            }
        }
    }
} 