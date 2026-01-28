/*******************************************************************
* This is not attached to any GameObject
*
* This script defines a ScriptableObject Data Container for Power-Ups. 
* It serves as a modular template that stores configuration data for 
* various gameplay modifiers, including combat physics, movement boosts, 
* scoring mechanics, and defensive/offensive abilities. 
*
* By creating assets from this script, designers can define unique 
* power-up behaviors (e.g., Speed Boost, Shield, Point Multiplier) 
* that can be referenced and applied to players at runtime.
* 
* Bruce Gustin
* Jan 2, 2026
*******************************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "RestartData", menuName = "Scriptable Objects/RestartData")]
public class RestartData : ScriptableObject
{
    [Header("General Information")]
        public string powerUpName;                // Name of powerup
        public Color colorIndicator;              // Color of light around player indicating powerup
        public float duration;                    // Duration of effect

    [Header("Core Combat/Push Mechanics")]
        public float pushForceMultiplier = 1f;    // Push enemies/players harder
        public float massIncrease = 0f;           // Harder to be pushed yourself
        public float knockbackResistance = 0f;    // Reduce incoming push force (0-1)
        public float pushRange = 0f;              // Increase push distance/radius
        public bool areaOfEffectPush = false;     // Push multiple targets at once

    [Header("Movement")]
        public float speedMultiplier = 1f;        // Move faster
        public float accelerationBoost = 0f;      // Reach top speed faster
        public float jumpPowerBoost = 0f;         // Jump higher/farther
        public bool grantDash = false;            // Quick dash ability
        public int dashCharges = 0;               // Number of dashes
        public float gravityMultiplier = 1f;      // Control fall speed

    [Header("Scoring")]
        public int scoreValue = 0;                // Instant points on pickup
        public float scoreMultiplier = 1f;        // Multiply points from scoreables
        public float magnetRadius = 0f;           // Auto-collect scoreables from distance
        public float collectionRangeBoost = 0f;   // Larger collection radius

    [Header("Defense")]
        public bool shieldActive = false;         // Absorb one push/knockoff
        public int shieldHitPoints = 0;           // Multi-hit shield
        public bool invincibility = false;        // Can't be pushed off (short duration)
        public float respawnTime = 0f;            // Faster respawn if you fall off
        public bool anchorMode = false;           // Can't be moved (but can't move either)

    [Header("Offense")]
        public bool freezeNearbyPlayers = false;  // Slow/freeze nearby enemies
        public float stunDuration = 0f;           // Stun hit targets
        public bool reverseControls = false;      // Reverse enemy controls nearby
        public float confusionRadius = 0f;        // Confuse enemies in area
        public bool stealPoints = false;          // Steal points on collision
        public int pointsStealAmount = 0;         // How many points to steal
}
