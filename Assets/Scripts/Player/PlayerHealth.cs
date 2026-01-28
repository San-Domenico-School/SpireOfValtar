using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 100;

    [SerializeField] RestartManager restartManager;

    // --- UI Elements ---
    [Header("UI")]
    [SerializeField] private UIDocument healthUIDocument;  // Assign in Inspector
    private ProgressBar healthBar;  
    
    void Start()
    {
        SetHealthAtStart();
    }                      // ProgressBar instead of VisualElement

    public void SetHealthAtStart()
    {
        health = RestartEvents.maxHealth;
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
        health -= amount;
        Debug.Log($"Player has taken {amount} damage, remaining health = {health}");

        UpdateHealthUI();

        if (health <= 0)
        {
            Debug.Log("Player has died");
            restartManager.RestartLevel();
        }
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
