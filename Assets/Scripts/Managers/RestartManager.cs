using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RestartManager : MonoBehaviour
{
    public static RestartManager Instance { get; private set; }

    private enum GameState
    {
        MainMenu,
        Playing,
        Dead,
        Restarting
    }

    [Header("Restart Mode")]
    [SerializeField] private bool reloadSceneOnRestart = true;
    [SerializeField] private bool destroyAndRespawnPlayer = false;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerTag = "Player";

    [Header("Death Flow")]
    [SerializeField] private bool pauseGameplayOnDeath = true;
    [SerializeField] private float restartDelaySeconds = 0.2f;

    [Header("Exit")]
    [SerializeField] private string mainMenuSceneName = string.Empty;

    private GameObject playerInstance;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerAbilityController playerAbilityController;

    private DeathUIController deathUIController;
    private GameUIManager gameUIManager;
    private MainMenuManager mainMenuManager;

    private Vector3 cachedSpawnPosition;
    private Quaternion cachedSpawnRotation = Quaternion.identity;
    private bool hasCachedSpawn;
    private bool isDeathSequenceActive;
    private bool isRestarting;
    private bool pendingPlayerRespawn;
    private GameState state = GameState.MainMenu;
    private PlayerHealth subscribedHealth;
    private int deathSubscriptionCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        CacheSceneReferences();
        EnsurePlayerReference();
        CacheSpawnPoint();
        SetInitialStateIfNeeded();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        UnsubscribeFromDeath();
    }

    private void OnPlayerDeath()
    {
        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:OnPlayerDeath",
            "OnDeath event received",
            "H11",
            "{\"state\":\"" + state + "\",\"isRestarting\":" + (isRestarting ? "true" : "false") + "}"
        );
        // #endregion

        if (state == GameState.Restarting || state == GameState.Dead)
        {
            return;
        }

        HandlePlayerDeath();
    }

    public void HandlePlayerDeath()
    {
        if (isDeathSequenceActive || state == GameState.Restarting || state == GameState.Dead) return;
        isDeathSequenceActive = true;
        state = GameState.Dead;

        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:HandlePlayerDeath",
            "HandlePlayerDeath invoked",
            "H4",
            "{\"isRestarting\":" + (isRestarting ? "true" : "false") +
            ",\"isDeathSequenceActive\":" + (isDeathSequenceActive ? "true" : "false") + "}"
        );
        // #endregion

        EnsurePlayerReference();
        SetPlayerControlEnabled(false);

        if (pauseGameplayOnDeath)
        {
            Time.timeScale = 0f;
        }

        if (gameUIManager != null)
        {
            gameUIManager.HideGameUI();
        }

        if (deathUIController != null)
        {
            deathUIController.PlayDeathSequence();
        }
        else
        {
            Debug.LogWarning("RestartManager: No DeathUIController found in scene.");
        }
    }

    public void RequestRestartFromDeath()
    {
        if (isRestarting || state != GameState.Dead) return;
        state = GameState.Restarting;
        StartCoroutine(RestartRoutine());
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        if (deathUIController != null)
        {
            deathUIController.HideImmediate();
        }

        if (gameUIManager != null)
        {
            gameUIManager.HideGameUI();
        }

        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
            return;
        }

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private IEnumerator RestartRoutine()
    {
        isRestarting = true;
        Time.timeScale = 1f;

        if (deathUIController != null)
        {
            deathUIController.HideImmediate();
        }

        if (restartDelaySeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(restartDelaySeconds);
        }

        if (destroyAndRespawnPlayer && playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
            pendingPlayerRespawn = true;
        }

        if (reloadSceneOnRestart)
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            yield break;
        }

        CacheSpawnPoint();
        PositionPlayerAtSpawn();
        ResetPlayerState();
        CompleteRestart();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:OnSceneLoaded",
            "Scene loaded",
            "H6",
            "{\"scene\":\"" + RuntimeDebugLogger.Escape(scene.name) +
            "\",\"mode\":\"" + mode +
            "\",\"pendingRespawn\":" + (pendingPlayerRespawn ? "true" : "false") +
            ",\"isRestarting\":" + (isRestarting ? "true" : "false") + "}"
        );
        // #endregion

        CacheSceneReferences();

        if (pendingPlayerRespawn)
        {
            RespawnPlayerFromPrefab();
            pendingPlayerRespawn = false;
        }

        EnsurePlayerReference();
        CacheSpawnPoint();
        PositionPlayerAtSpawn();
        ResetPlayerState();
        CompleteRestart();
    }

    private void CompleteRestart()
    {
        isDeathSequenceActive = false;
        SetPlayerControlEnabled(true);
        if (deathUIController != null)
        {
            deathUIController.HideImmediate();
        }
        if (gameUIManager != null)
        {
            gameUIManager.StartGame();
        }
        if (mainMenuManager != null)
        {
            mainMenuManager.HideMainMenu();
        }

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        if (playerHealth != null)
        {
            Debug.Assert(playerHealth.CurrentHealth > 0, "Player health invalid after restart");
            Debug.Assert(!playerHealth.IsDead, "Player isDead still true after restart");
        }

        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:CompleteRestart",
            "CompleteRestart finished",
            "H5",
            "{\"health\":" + (playerHealth != null ? playerHealth.CurrentHealth.ToString() : "null") +
            ",\"isDead\":" + (playerHealth != null && playerHealth.IsDead ? "true" : "false") + "}"
        );
        // #endregion

        state = GameState.Playing;
        isRestarting = false;
    }

    private void CacheSceneReferences()
    {
        deathUIController = FindFirstObjectByType<DeathUIController>(FindObjectsInactive.Include);
        gameUIManager = FindFirstObjectByType<GameUIManager>(FindObjectsInactive.Include);
        mainMenuManager = FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);

        DisableDeathUIOnLoad();
    }

    private void DisableDeathUIOnLoad()
    {
        if (deathUIController == null)
        {
            var deathDocument = FindDeathUIDocument();
            if (deathDocument != null)
            {
                deathUIController = deathDocument.GetComponent<DeathUIController>();
                if (deathUIController == null)
                {
                    deathUIController = deathDocument.gameObject.AddComponent<DeathUIController>();
                }
            }
        }

        if (deathUIController != null)
        {
            deathUIController.HideImmediate();
            return;
        }

        // Fallback: disable DeathScreen UIDocument even if controller is missing
        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals("DeathScreen", System.StringComparison.OrdinalIgnoreCase))
            {
                document.enabled = false;
            }
        }
    }

    private UIDocument FindDeathUIDocument()
    {
        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals("DeathScreen", System.StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }
        }

        return null;
    }

    private void EnsurePlayerReference()
    {
        if (playerInstance == null)
        {
            if (!string.IsNullOrEmpty(playerTag))
            {
                playerInstance = GameObject.FindGameObjectWithTag(playerTag);
            }

            if (playerInstance == null)
            {
                var playerComponent = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
                if (playerComponent != null)
                {
                    playerInstance = playerComponent.gameObject;
                }
            }
        }

        if (playerInstance != null)
        {
            playerHealth = playerInstance.GetComponent<PlayerHealth>();
            playerMovement = playerInstance.GetComponent<PlayerMovement>();
            playerAbilityController = playerInstance.GetComponent<PlayerAbilityController>();
        }

        SubscribeToDeath();
    }

    private void CacheSpawnPoint()
    {
        var spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>(FindObjectsInactive.Include);
        if (spawnPoint != null)
        {
            cachedSpawnPosition = spawnPoint.Position;
            cachedSpawnRotation = spawnPoint.Rotation;
            hasCachedSpawn = true;
            return;
        }

        if (!hasCachedSpawn && playerInstance != null)
        {
            cachedSpawnPosition = playerInstance.transform.position;
            cachedSpawnRotation = playerInstance.transform.rotation;
            hasCachedSpawn = true;
        }
    }

    private void PositionPlayerAtSpawn()
    {
        if (!hasCachedSpawn || playerInstance == null) return;

        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:PositionPlayerAtSpawn",
            "Positioning player at spawn",
            "H7",
            "{\"x\":" + cachedSpawnPosition.x.ToString("F2") +
            ",\"y\":" + cachedSpawnPosition.y.ToString("F2") +
            ",\"z\":" + cachedSpawnPosition.z.ToString("F2") + "}"
        );
        // #endregion

        var controller = playerInstance.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        playerInstance.transform.SetPositionAndRotation(cachedSpawnPosition, cachedSpawnRotation);
        if (controller != null) controller.enabled = true;
    }

    private void ResetPlayerState()
    {
        if (playerHealth != null)
        {
            // #region agent log
            RuntimeDebugLogger.Log(
                "RestartManager.cs:ResetPlayerState",
                "Resetting player state",
                "H8",
                "{\"healthBefore\":" + playerHealth.CurrentHealth +
                ",\"isDeadBefore\":" + (playerHealth.IsDead ? "true" : "false") + "}"
            );
            // #endregion

            playerHealth.SuppressDeathEvents(true);
            playerHealth.ResetForRespawn();
            playerHealth.SuppressDeathEvents(false);
        }

        if (playerMovement != null)
        {
            playerMovement.ResetMovementState();
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.ResetState();
        }
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.enabled = enabled;
        }
    }

    private void RespawnPlayerFromPrefab()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("RestartManager: destroyAndRespawnPlayer is enabled, but no playerPrefab is set.");
            return;
        }

        var instance = Instantiate(playerPrefab);
        playerInstance = instance;
        EnsurePlayerReference();
    }

    private void SubscribeToDeath()
    {
        if (playerHealth == null)
        {
            return;
        }

        if (subscribedHealth != null && subscribedHealth != playerHealth)
        {
            UnsubscribeFromDeath();
        }

        if (subscribedHealth == playerHealth)
        {
            return;
        }

        subscribedHealth = playerHealth;
        subscribedHealth.OnDeath += OnPlayerDeath;
        deathSubscriptionCount++;

        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:SubscribeToDeath",
            "Subscribed to OnDeath",
            "H12",
            "{\"playerId\":" + subscribedHealth.GetInstanceID() +
            ",\"count\":" + deathSubscriptionCount + "}"
        );
        // #endregion
    }

    private void UnsubscribeFromDeath()
    {
        if (subscribedHealth == null)
        {
            return;
        }

        subscribedHealth.OnDeath -= OnPlayerDeath;
        subscribedHealth = null;

        // #region agent log
        RuntimeDebugLogger.Log(
            "RestartManager.cs:UnsubscribeFromDeath",
            "Unsubscribed from OnDeath",
            "H13",
            "{}"
        );
        // #endregion
    }

    private void SetInitialStateIfNeeded()
    {
        if (state == GameState.Restarting)
        {
            return;
        }

        state = playerHealth != null ? GameState.Playing : GameState.MainMenu;
    }
}
