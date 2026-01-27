using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RestartManager restartManager;

    void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    void OnEnable()
    {
        // Disabled: RestartManager is the single death authority.
    }

    void OnDisable()
    {
        // Disabled: RestartManager is the single death authority.
    }

    private void HandleDeath()
    {
        // #region agent log
        RuntimeDebugLogger.Log(
            "PlayerDeathHandler.cs:HandleDeath",
            "OnDeath received",
            "H3",
            "{}"
        );
        // #endregion

        if (restartManager == null)
        {
            restartManager = RestartManager.Instance;
        }

        if (restartManager != null)
        {
            restartManager.HandlePlayerDeath();
        }
        else
        {
            Debug.LogWarning("PlayerDeathHandler: RestartManager not found.");
        }
    }
}
