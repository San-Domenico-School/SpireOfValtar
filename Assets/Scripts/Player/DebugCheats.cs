using UnityEngine;
using System.Collections;
using System.Reflection;

public class DebugCheats : MonoBehaviour
{
    [Header("Debug Cheats")]
    [Tooltip("Toggle on to apply cheats, toggle off to restore original values.")]
    [SerializeField] private bool enableCheats = false;

    // Cached components
    private PlayerMovement movement;
    private PlayerHealth ph;

    // Original values
    private float originalWalkSpeed;
    private int originalMaxHealth;
    private int originalHealth;
    private int originalGold;

    // Track previous state to detect toggle changes
    private bool previousEnableCheats;

    private FieldInfo speedField;
    private FieldInfo goldField;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        ph = GetComponent<PlayerHealth>();

        speedField = typeof(PlayerMovement).GetField("walkSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        goldField = typeof(GoldCollector).GetField("goldCollected", BindingFlags.NonPublic | BindingFlags.Instance);

        // Save original walk speed before anything modifies it
        if (movement != null && speedField != null)
            originalWalkSpeed = (float)speedField.GetValue(movement);

        // maxHealth before PlayerHealth.Start() resets it (health not set yet here)
        if (ph != null)
            originalMaxHealth = ph.maxHealth;

        if (enableCheats)
            ph.maxHealth = 999999999;
    }

    private void Start()
    {
        // Save original health after PlayerHealth.Start() has run ResetHealth()
        if (ph != null)
            originalHealth = ph.health;

        previousEnableCheats = enableCheats;

        if (enableCheats)
            StartCoroutine(ApplyCheatsNextFrame());
    }

    private void Update()
    {
        if (enableCheats == previousEnableCheats) return;
        previousEnableCheats = enableCheats;

        if (enableCheats)
            StartCoroutine(ApplyCheatsNextFrame());
        else
            StartCoroutine(RestoreNextFrame());
    }

    private IEnumerator ApplyCheatsNextFrame()
    {
        yield return null;

        if (movement != null)
            speedField?.SetValue(movement, 10f);

        if (ph != null)
            ph.ApplySessionData(999999999, 999999999);

        if (GoldCollector.Instance != null)
        {
            originalGold = (int)goldField.GetValue(GoldCollector.Instance);
            goldField?.SetValue(GoldCollector.Instance, 999999999);
        }
        else
        {
            Debug.LogWarning("DebugCheats: GoldCollector.Instance not found.");
        }
    }

    private IEnumerator RestoreNextFrame()
    {
        yield return null;

        if (movement != null)
            speedField?.SetValue(movement, originalWalkSpeed);

        if (ph != null)
            ph.ApplySessionData(originalHealth, originalMaxHealth);

        if (GoldCollector.Instance != null)
            goldField?.SetValue(GoldCollector.Instance, originalGold);
    }
}
