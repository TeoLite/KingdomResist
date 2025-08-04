using UnityEngine;

[CreateAssetMenu(fileName = "Basic Warrior Card", menuName = "Kingdom Resist/Examples/Basic Warrior Card")]
public class BasicWarriorCard : MinionCard
{
    private void OnEnable()
    {
        // Set default values for the Basic Warrior Card
        cardName = "Basic Warrior";
        description = "Summons a basic warrior to fight for you.";
        manaCost = 3;
        kingdomType = KingdomType.GreatZoey;
        
        // Set spawn offset (relative to player hub)
        spawnOffset = new Vector2(2f, 0f);
    }
} 