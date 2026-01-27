using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 100;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private bool isDead;
    private bool suppressDeathEvents;

    public int CurrentHealth => health;
    public bool IsDead => isDead;

    // --- UI Elements ---
    [Header("UI")]
    [SerializeField] private UIDocument healthUIDocument;  // Assign in Inspector
    private ProgressBar healthBar;                        // ProgressBar instead of VisualElement

    void Start()
    {
        ResetHealth();
        Debug.Log("Health = 100");

        // --- Initialize UI ---
        if (healthUIDocument != null)
        {
            var root = healthUIDocument.rootVisualElement;

            // CHANGE "HealthBar" to whatever the Progress Bar name is in UI Builder
            healthBar = root.Q<ProgressBar>("HealthBar");

            if (healthBar == null)
                Debug.LogWarning("PlayerHealth: Could not find ProgressBar named 'HealthBar'.");

            UpdateHealthUI();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: No UIDocument assigned for health UI.");
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (suppressDeathEvents) return;

        int previousHealth = health;
        int newHealth = health - amount;
        SetHealth(newHealth);
        Debug.Log($"Player has taken {amount} damage, remaining health = {health}");

        // #region agent log
        RuntimeDebugLogger.Log(
            "PlayerHealth.cs:TakeDamage",
            "Damage applied",
            "H2",
            "{\"amount\":" + amount +
            ",\"previous\":" + previousHealth +
            ",\"current\":" + health +
            ",\"isDead\":" + (isDead ? "true" : "false") +
            ",\"suppress\":" + (suppressDeathEvents ? "true" : "false") + "}"
        );
        // #endregion

        if (previousHealth > 0 && newHealth <= 0)
        {
            isDead = true;
            Debug.Log("Player has died");
            OnDeath?.Invoke();
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        SetHealth(maxHealth);
    }

    public void ResetForRespawn()
    {
        suppressDeathEvents = true;
        isDead = false;
        SetHealth(maxHealth);
        suppressDeathEvents = false;
    }

    public void SuppressDeathEvents(bool suppress)
    {
        suppressDeathEvents = suppress;
    }

    public void SetSuppressEvents(bool value)
    {
        suppressDeathEvents = value;
    }

    private void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    // --- UI Update Method ---
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = health;     // ProgressBar automatically handles max value
            healthBar.highValue = maxHealth;
        }
    }
}
