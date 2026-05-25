using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private DeathUIController deathUIController;
    [SerializeField] private GameUIManager gameUIManager;

    void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandleDeath;
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandleDeath;
        }
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

        if (deathUIController == null)
        {
            deathUIController = FindFirstObjectByType<DeathUIController>(FindObjectsInactive.Include);
        }

        if (gameUIManager == null)
        {
            gameUIManager = FindFirstObjectByType<GameUIManager>(FindObjectsInactive.Include);
        }

        if (gameUIManager != null)
        {
            gameUIManager.HideGameUI();
        }

        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        if (deathUIController != null)
        {
            deathUIController.PlayDeathSequence();
        }
        else
        {
            Debug.LogWarning("PlayerDeathHandler: DeathUIController not found.");
        }
    }
}
