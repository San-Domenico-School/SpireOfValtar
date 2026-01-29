using UnityEngine;

/***************************************************
 * PlayerSession.cs
 * 
 * Persistent session container for player data.
 * Holds default values and the latest saved state.
 * Single source of truth for player state across scenes.
 * Gleb
 * 01.27.2026
 ***************************************************/
public class PlayerSession : MonoBehaviour
{
    private static PlayerSession instance;

    [Header("Default Values")]
    [SerializeField] private int defaultMaxHealth = 100;
    [SerializeField] private int defaultHealth = 100;
    [SerializeField] private float defaultMaxStamina = 100f;
    [SerializeField] private float defaultStamina = 100f;

    private int currentHealth;
    private int maxHealth;
    private float currentStamina;
    private float maxStamina;
    private Vector3 position;
    private Quaternion rotation = Quaternion.identity;
    private bool hasSavedTransform;
    private bool restartFlag;

    public bool HasSavedTransform => hasSavedTransform;
    public Vector3 SavedPosition => position;
    public Quaternion SavedRotation => rotation;
    public bool RestartFlag => restartFlag;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ResetToDefaults();
    }

    public void SaveFromPlayer(Player player)
    {
        if (player == null) return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            currentHealth = health.CurrentHealth;
            maxHealth = health.maxHealth;
        }

        var abilities = player.GetComponent<PlayerAbilityController>();
        if (abilities != null)
        {
            currentStamina = abilities.CurrentStamina;
            maxStamina = abilities.MaxStamina;
        }

        position = player.transform.position;
        rotation = player.transform.rotation;
        hasSavedTransform = true;
    }

    public void ApplyToPlayer(Player player)
    {
        if (player == null) return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ApplySessionData(currentHealth, maxHealth);
        }

        var abilities = player.GetComponent<PlayerAbilityController>();
        if (abilities != null)
        {
            abilities.ApplySessionData(currentStamina, maxStamina);
        }
    }

    public void ResetToDefaults()
    {
        maxHealth = defaultMaxHealth;
        currentHealth = Mathf.Clamp(defaultHealth, 0, defaultMaxHealth);
        maxStamina = defaultMaxStamina;
        currentStamina = Mathf.Clamp(defaultStamina, 0f, defaultMaxStamina);
        position = Vector3.zero;
        rotation = Quaternion.identity;
        hasSavedTransform = false;
    }

    public void SetRestartFlag(bool value)
    {
        restartFlag = value;
    }
}
