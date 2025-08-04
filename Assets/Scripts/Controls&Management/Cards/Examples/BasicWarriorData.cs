using UnityEngine;

[CreateAssetMenu(fileName = "Basic Warrior Data", menuName = "Kingdom Resist/Examples/Basic Warrior Data")]
public class BasicWarriorData : MinionData
{
    private void OnEnable()
    {
        // Set default values for the Basic Warrior
        minionName = "Basic Warrior";
        manaCost = 3;
        kingdom = KingdomType.GreatZoey;
        
        // Stats
        maxHealth = 100f;
        damage = 15f;
        attackSpeed = 1f;
        moveSpeed = 3f;
        attackRange = 1.5f;
        
        // Properties
        isRanged = false;
        canTargetAir = false;
        canTargetBuildings = true;
        isFlying = false;
        specialAbility = MinionSpecialAbility.None;
        
        description = "A basic melee warrior with balanced stats.";
    }
} 