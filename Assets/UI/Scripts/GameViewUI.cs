using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

/************************************
 * Manages health, stamina, and gold display in the game view HUD.
 * Gleb 01/09/26 — v1.0
 * Gold: stale-panel fix + debug logging — v1.3
 ************************************/
public class GameViewUI : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument; // drag Game_View UIDocument here in Inspector

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAbilityController playerAbilityController;

    // Cached UI elements
    private ProgressBar healthBar;
    private ProgressBar staminaBar;
    private Label       healthValueLabel;
    private Label       goldLabel;
    private Label       killCounterLabel;

    // Kill counter state
    private Stairs stairsRef;
    private int    lastKillsRemaining = -1;

    // Runtime state
    private float        currentHealth  = 100f;
    private float        maxHealth      = 100f;
    private float        currentStamina = 100f;
    private float        maxStamina     = 100f;
    private PlayerHealth subscribedHealth;

    // Debug throttle — logs gold state once per second so console isn't spammed
    private float debugTimer = 0f;

    // ─── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // Fallback if Inspector slot is empty: try same GameObject, then search all
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
            TryFindUIDocument();
    }

    private void Start()
    {
        // Start() is the safe earliest point — rootVisualElement is guaranteed ready
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindUIDocument();
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    private void Update()
    {
        // ── Health ────────────────────────────────────────────────────────────
        if (playerHealth == null || playerHealth != subscribedHealth)
        {
            CachePlayerReferences();
            SubscribeToPlayer();
            SyncFromPlayer();
        }

        if (playerAbilityController == null)
            CachePlayerReferences();

        if (playerHealth != null)
        {
            if (!Mathf.Approximately(currentHealth, playerHealth.CurrentHealth) ||
                !Mathf.Approximately(maxHealth,     playerHealth.maxHealth))
            {
                currentHealth = playerHealth.CurrentHealth;
                maxHealth     = playerHealth.maxHealth;
                UpdateHealthBar();
            }
        }

        // ── Stamina ───────────────────────────────────────────────────────────
        if (playerAbilityController != null)
        {
            if (!Mathf.Approximately(currentStamina, playerAbilityController.CurrentStamina) ||
                !Mathf.Approximately(maxStamina,     playerAbilityController.MaxStamina))
            {
                currentStamina = playerAbilityController.CurrentStamina;
                maxStamina     = playerAbilityController.MaxStamina;
                UpdateStaminaBar();
            }
        }

        // ── Gold ──────────────────────────────────────────────────────────────
        UpdateGoldLabel();

        // ── Kill Counter ──────────────────────────────────────────────────────
        UpdateKillCounterLabel();
    }

    // ─── Gold update ───────────────────────────────────────────────────────────

    private void UpdateGoldLabel()
    {
        // If UIDocument still not found, keep trying
        if (uiDocument == null)
            TryFindUIDocument();

        // KEY FIX: goldLabel.panel == null means the visual tree was rebuilt
        // (happens every time a UIDocument is disabled then re-enabled — pause menu, shop, etc.)
        // When stale, throw away the reference and re-query
        if (goldLabel != null && goldLabel.panel == null)
        {
            Debug.Log("[GameViewUI] goldLabel was stale (panel detached) — re-querying.");
            goldLabel = null;
        }

        // Re-query if we don't have a live reference
        if (goldLabel == null && uiDocument != null && uiDocument.rootVisualElement != null)
            goldLabel = uiDocument.rootVisualElement.Q<Label>("GoldLabel");

        // Debug log once per second so you can see what's actually happening
        debugTimer += Time.deltaTime;
        if (debugTimer >= 1f)
        {
            debugTimer = 0f;
            int  actualGold      = GoldCollector.Instance != null ? GoldCollector.Instance.GetGold() : -999;
            bool labelAlive      = goldLabel != null;
            bool docFound        = uiDocument != null;
            bool panelAttached   = goldLabel != null && goldLabel.panel != null;
            Debug.Log($"[GameViewUI Gold] GoldCollector.GetGold()={actualGold} | " +
                      $"uiDocument={docFound} | goldLabel found={labelAlive} | panel attached={panelAttached}");
        }

        // Write the value — same direct read as SpellShopUI
        if (goldLabel != null && GoldCollector.Instance != null)
            goldLabel.text = GoldCollector.Instance.GetGold().ToString();
    }

    // ─── Kill Counter update ───────────────────────────────────────────────────

    private void UpdateKillCounterLabel()
    {
        // Lazy-find killCounterLabel the same way goldLabel is handled
        if (killCounterLabel != null && killCounterLabel.panel == null)
            killCounterLabel = null;

        if (killCounterLabel == null && uiDocument != null && uiDocument.rootVisualElement != null)
            killCounterLabel = uiDocument.rootVisualElement.Q<Label>("KillCounterLabel");

        if (killCounterLabel == null) return;

        // Lazy-find Stairs — only present in scenes that have the staircase
        if (stairsRef == null)
            stairsRef = FindFirstObjectByType<Stairs>(FindObjectsInactive.Include);

        // No Stairs in this scene — keep label hidden
        if (stairsRef == null)
        {
            killCounterLabel.style.display = DisplayStyle.None;
            return;
        }

        // Stairs done — hide the label
        if (stairsRef.IsComplete)
        {
            killCounterLabel.style.display = DisplayStyle.None;
            return;
        }

        // Show and update only when the value actually changes
        int remaining = stairsRef.KillsRemaining;
        if (remaining != lastKillsRemaining)
        {
            lastKillsRemaining  = remaining;
            killCounterLabel.text = $"Kill {remaining} more {(remaining == 1 ? "enemy" : "enemies")}";
        }

        killCounterLabel.style.display = DisplayStyle.Flex;
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        UnsubscribeFromPlayer();
        BindUI();
        CachePlayerReferences();
        SubscribeToPlayer();
        SyncFromPlayer();
    }

    public void SetHealth(float health)   { currentHealth  = Mathf.Clamp(health,  0, maxHealth);  UpdateHealthBar();  }
    public void SetMaxHealth(float max)   { maxHealth      = max;                                   UpdateHealthBar();  }
    public void SetStamina(float stamina) { currentStamina = Mathf.Clamp(stamina, 0, maxStamina); UpdateStaminaBar(); }
    public void SetMaxStamina(float max)  { maxStamina     = max;                                   UpdateStaminaBar(); }

    public float GetCurrentHealth()  => currentHealth;
    public float GetMaxHealth()      => maxHealth;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina()     => maxStamina;

    // ─── UI binding ────────────────────────────────────────────────────────────

    private void BindUI()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root         = uiDocument.rootVisualElement;
        healthBar        = root.Q<ProgressBar>("HealthBar");
        staminaBar       = root.Q<ProgressBar>("StaminaProgressBar");
        healthValueLabel = root.Q<Label>("HealthValueLabel");
        goldLabel        = root.Q<Label>("GoldLabel");
        killCounterLabel = root.Q<Label>("KillCounterLabel");

        // Re-find Stairs whenever UI is rebound (scene may have changed)
        stairsRef            = FindFirstObjectByType<Stairs>(FindObjectsInactive.Include);
        lastKillsRemaining   = -1;

        Debug.Log($"[GameViewUI] BindUI complete — goldLabel found: {goldLabel != null} | killLabel found: {killCounterLabel != null} | stairs found: {stairsRef != null}");
    }

    private void TryFindUIDocument()
    {
        if (uiDocument != null) return;

        foreach (var doc in FindObjectsOfType<UIDocument>(true))
        {
            if (doc == null || doc.visualTreeAsset == null) continue;
            if (doc.visualTreeAsset.name.Equals("Game_View", StringComparison.OrdinalIgnoreCase))
            {
                uiDocument = doc;
                Debug.Log("[GameViewUI] UIDocument found via search.");
                return;
            }
        }
    }

    // ─── Player references ─────────────────────────────────────────────────────

    private void CachePlayerReferences()
    {
        var player = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        if (player != null)
        {
            playerHealth            = player.GetComponent<PlayerHealth>();
            playerAbilityController = player.GetComponent<PlayerAbilityController>();
            return;
        }

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);

        if (playerAbilityController == null)
            playerAbilityController = FindFirstObjectByType<PlayerAbilityController>(FindObjectsInactive.Include);
    }

    private void SubscribeToPlayer()
    {
        if (playerHealth == null) return;

        if (subscribedHealth != null && subscribedHealth != playerHealth)
            subscribedHealth.OnHealthChanged -= HandleHealthChanged;

        playerHealth.OnHealthChanged -= HandleHealthChanged;
        playerHealth.OnHealthChanged += HandleHealthChanged;
        subscribedHealth = playerHealth;
    }

    private void UnsubscribeFromPlayer()
    {
        if (subscribedHealth == null) return;
        subscribedHealth.OnHealthChanged -= HandleHealthChanged;
        subscribedHealth = null;
    }

    // ─── Event handlers ────────────────────────────────────────────────────────

    private void HandleHealthChanged(int current, int max)
    {
        currentHealth = current;
        maxHealth     = max;
        UpdateHealthBar();
    }

    // ─── Sync & bar updates ────────────────────────────────────────────────────

    private void SyncFromPlayer()
    {
        if (playerHealth != null)
        {
            currentHealth = playerHealth.CurrentHealth;
            maxHealth     = playerHealth.maxHealth;
        }

        if (playerAbilityController != null)
        {
            currentStamina = playerAbilityController.CurrentStamina;
            maxStamina     = playerAbilityController.MaxStamina;
        }

        UpdateHealthBar();
        UpdateStaminaBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.highValue = maxHealth;
            healthBar.value     = currentHealth;
        }

        if (healthValueLabel != null)
            healthValueLabel.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.highValue = maxStamina;
            staminaBar.value     = currentStamina;
        }
    }
}
