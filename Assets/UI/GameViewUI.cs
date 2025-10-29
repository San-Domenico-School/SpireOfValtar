using UnityEngine;
using UnityEngine.UIElements;

public class GameViewUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private ProgressBar healthBar;
    private ProgressBar staminaBar;
    private Label healthValueLabel;

    private float currentHealth = 100f;
    private float maxHealth = 100f;
    private float currentStamina = 100f;
    private float maxStamina = 100f;

    void Start()
    {
        // Get the UIDocument component
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // Get references to the UI elements
        healthBar = root.Q<ProgressBar>("HealthProgressBar");
        staminaBar = root.Q<ProgressBar>("StaminaProgressBar");
        healthValueLabel = root.Q<Label>("HealthValueLabel");

        // Set the max values
        healthBar.highValue = maxHealth;
        staminaBar.highValue = maxStamina;
        
        // Set initial values
        UpdateHealthBar();
        UpdateStaminaBar();
    }

    // Method to update health (call this when health changes)
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    // Method to set max health
    public void SetMaxHealth(float max)
    {
        maxHealth = max;
        healthBar.highValue = maxHealth;
        UpdateHealthBar();
    }

    // Method to update stamina (call this when stamina changes)
    public void SetStamina(float stamina)
    {
        currentStamina = Mathf.Clamp(stamina, 0, maxStamina);
        UpdateStaminaBar();
    }

    // Method to set max stamina
    public void SetMaxStamina(float max)
    {
        maxStamina = max;
        staminaBar.highValue = maxStamina;
        UpdateStaminaBar();
    }

    // Internal method to update the health bar UI
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        
        if (healthValueLabel != null)
        {
            healthValueLabel.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    // Internal method to update the stamina bar UI
    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    // Example: Test the bars (call from Update() or via button)
    void Update()
    {
        // Example: Press H to reduce health by 10
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetHealth(currentHealth - 10);
        }
        
        // Example: Press S to reduce stamina by 10
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetStamina(currentStamina - 10);
        }

        // Example: Press R to regenerate stamina
        if (Input.GetKey(KeyCode.R))
        {
            SetStamina(currentStamina + 5 * Time.deltaTime);
        }
    }

    // Get current values (useful for other scripts)
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
}

