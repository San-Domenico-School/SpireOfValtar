using UnityEngine;
using UnityEngine.UIElements;

/************************************
 * Manages health and stamina bars display in the game view.
 * Updates UI elements based on current health and stamina values.
 * Gleb 01/09/26
 * Version 1.0
 ************************************/
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
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            return;
        }

        var root = uiDocument.rootVisualElement;

        healthBar = root.Q<ProgressBar>("HealthProgressBar");
        staminaBar = root.Q<ProgressBar>("StaminaProgressBar");
        healthValueLabel = root.Q<Label>("HealthValueLabel");

        healthBar.highValue = maxHealth;
        staminaBar.highValue = maxStamina;
        
        UpdateHealthBar();
        UpdateStaminaBar();
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    public void SetMaxHealth(float max)
    {
        maxHealth = max;
        healthBar.highValue = maxHealth;
        UpdateHealthBar();
    }

    public void SetStamina(float stamina)
    {
        currentStamina = Mathf.Clamp(stamina, 0, maxStamina);
        UpdateStaminaBar();
    }

    public void SetMaxStamina(float max)
    {
        maxStamina = max;
        staminaBar.highValue = maxStamina;
        UpdateStaminaBar();
    }

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

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
}

