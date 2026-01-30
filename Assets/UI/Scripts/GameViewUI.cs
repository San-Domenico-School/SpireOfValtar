using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/************************************
 * Manages health and stamina bars display in the game view.
 * Updates UI elements based on current health and stamina values.
 * Gleb 01/09/26
 * Version 1.0
 ************************************/
public class GameViewUI : MonoBehaviour
{
    private const string GameViewUxmlName = "Game_View";
    private UIDocument uiDocument;
    private ProgressBar healthBar;
    private ProgressBar staminaBar;
    private Label healthValueLabel;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAbilityController playerAbilityController;

    private float currentHealth = 100f;
    private float maxHealth = 100f;
    private float currentStamina = 100f;
    private float maxStamina = 100f;
    private PlayerHealth subscribedHealth;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    public void RefreshUI()
    {
        UnsubscribeFromPlayer();
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    private void BindUI()
    {
        ResolveUIDocument();

        if (uiDocument == null)
        {
            return;
        }

        var root = uiDocument.rootVisualElement;
        healthBar = root.Q<ProgressBar>("HealthBar");
        staminaBar = root.Q<ProgressBar>("StaminaProgressBar");
        healthValueLabel = root.Q<Label>("HealthValueLabel");
    }

    private void ResolveUIDocument()
    {
        if (uiDocument != null)
        {
            return;
        }

        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            return;
        }

        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals(GameViewUxmlName, System.StringComparison.OrdinalIgnoreCase))
            {
                uiDocument = document;
                return;
            }
        }
    }

    private void CachePlayerReferences()
    {
        var player = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerAbilityController = player.GetComponent<PlayerAbilityController>();
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        }

        if (playerAbilityController == null)
        {
            playerAbilityController = FindFirstObjectByType<PlayerAbilityController>(FindObjectsInactive.Include);
        }
    }

    private void SubscribeToPlayer()
    {
        if (playerHealth == null)
        {
            return;
        }

        if (subscribedHealth != null && subscribedHealth != playerHealth)
        {
            subscribedHealth.OnHealthChanged -= HandleHealthChanged;
        }

        playerHealth.OnHealthChanged -= HandleHealthChanged;
        playerHealth.OnHealthChanged += HandleHealthChanged;
        subscribedHealth = playerHealth;
    }

    private void UnsubscribeFromPlayer()
    {
        if (subscribedHealth != null)
        {
            subscribedHealth.OnHealthChanged -= HandleHealthChanged;
            subscribedHealth = null;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        maxHealth = max;
        currentHealth = current;
        UpdateHealthBar();
    }

    private void SyncFromPlayer()
    {
        if (playerHealth != null)
        {
            maxHealth = playerHealth.maxHealth;
            currentHealth = playerHealth.CurrentHealth;
        }

        if (playerAbilityController != null)
        {
            maxStamina = playerAbilityController.MaxStamina;
            currentStamina = playerAbilityController.CurrentStamina;
        }

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
        UpdateStaminaBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.highValue = maxHealth;
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
            staminaBar.highValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    private void Update()
    {
        if (playerHealth == null || playerHealth != subscribedHealth)
        {
            CachePlayerReferences();
            SubscribeToPlayer();
            SyncFromPlayer();
        }

        if (playerAbilityController == null)
        {
            CachePlayerReferences();
        }

        if (playerHealth != null)
        {
            if (!Mathf.Approximately(currentHealth, playerHealth.CurrentHealth) ||
                !Mathf.Approximately(maxHealth, playerHealth.maxHealth))
            {
                currentHealth = playerHealth.CurrentHealth;
                maxHealth = playerHealth.maxHealth;
                UpdateHealthBar();
            }
        }

        if (playerAbilityController != null)
        {
            if (!Mathf.Approximately(currentStamina, playerAbilityController.CurrentStamina) ||
                !Mathf.Approximately(maxStamina, playerAbilityController.MaxStamina))
            {
                currentStamina = playerAbilityController.CurrentStamina;
                maxStamina = playerAbilityController.MaxStamina;
                UpdateStaminaBar();
            }
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
}

