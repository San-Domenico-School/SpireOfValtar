using UnityEngine;
using UnityEngine.SceneManagement;

/***************************************************
 * PlayerSpawner.cs
 * 
 * Scene-based spawner for the Player prefab.
 * Creates fresh Player instances and applies PlayerSession state.
 * Handles Play and Restart flows without persistence on the Player.
 * Gleb
 * 01.27.2026
 ***************************************************/
public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PlayerSpawnPoint spawnPoint;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private int startSceneIndex = -1;

    private PlayerSession session;
    private Player currentPlayer;

    void Awake()
    {
        EnsureSession();
        CacheSpawnPoint();

        if (startSceneIndex < 0)
        {
            startSceneIndex = SceneManager.GetActiveScene().buildIndex;
        }
    }

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnIfMissing();
            ApplySessionToPlayer();
            session.SetRestartFlag(false);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSession();
        
        // Clear cached spawn point to find the new scene's spawn point
        spawnPoint = null;
        CacheSpawnPoint();
        
        SpawnIfMissing();
        
        // Always place existing player at the new scene's spawn point
        if (currentPlayer != null && spawnPoint != null)
        {
            PlaceAtSpawnPoint(currentPlayer.transform);
        }
    }

    public void StartGame()
    {
        EnsureSession();
        SpawnIfMissing();
        ApplySessionToPlayer();
        session.SetRestartFlag(false);
    }

    public void RestartGame()
    {
        EnsureSession();
        session.SetRestartFlag(true);
        session.ResetToDefaults();

        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneIndex);
    }

    private void SpawnIfMissing()
    {
        if (currentPlayer == null)
        {
            var existing = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
            if (existing != null)
            {
                currentPlayer = existing;
                return;
            }
            SpawnNewPlayer();
        }
    }

    private void SpawnNewPlayer()
    {
        var existing = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        if (existing != null)
        {
            currentPlayer = existing;
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogWarning("PlayerSpawner: Player prefab is not assigned.");
            return;
        }

        var instanceObject = Instantiate((Object)playerPrefab);
        var instance = instanceObject as GameObject;
        if (instance == null)
        {
            Debug.LogWarning("PlayerSpawner: Assigned prefab is not a GameObject.");
            return;
        }

        currentPlayer = instance.GetComponent<Player>();
        if (currentPlayer == null)
        {
            currentPlayer = instance.AddComponent<Player>();
        }

        var health = instance.GetComponentInChildren<PlayerHealth>(true);
        if (health != null)
        {
            var deathHandler = health.GetComponent<PlayerDeathHandler>();
            if (deathHandler == null)
            {
                deathHandler = health.gameObject.AddComponent<PlayerDeathHandler>();
            }
        }

        PlaceAtSpawn(currentPlayer.transform);
    }

    private void PlaceAtSpawn(Transform target)
    {
        if (target == null) return;

        CacheSpawnPoint();
        if (session != null && session.HasSavedTransform && !session.RestartFlag)
        {
            target.SetPositionAndRotation(session.SavedPosition, session.SavedRotation);
            return;
        }

        if (spawnPoint != null)
        {
            target.SetPositionAndRotation(spawnPoint.Position, spawnPoint.Rotation);
        }
    }

    /
    private void PlaceAtSpawnPoint(Transform target)
    {
        if (target == null || spawnPoint == null) return;

        // Disable CharacterController to allow position change
        var controller = target.GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled)
        {
            controller.enabled = false;
        }

        target.SetPositionAndRotation(spawnPoint.Position, spawnPoint.Rotation);
        Debug.Log($"PlayerSpawner: Placed player at spawn point {spawnPoint.Position}");

        if (wasEnabled)
        {
            controller.enabled = true;
        }
    }

    private void ApplySessionToPlayer()
    {
        if (currentPlayer == null || session == null) return;
        session.ApplyToPlayer(currentPlayer);
    }

    private void EnsureSession()
    {
        if (session != null) return;
        session = FindFirstObjectByType<PlayerSession>(FindObjectsInactive.Include);
        if (session == null)
        {
            var go = new GameObject("PlayerSession");
            session = go.AddComponent<PlayerSession>();
        }
    }

    private void CacheSpawnPoint()
    {
        if (spawnPoint == null)
        {
            spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>(FindObjectsInactive.Include);
        }
    }
}
