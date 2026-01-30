using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

/************************************
 * Handles main menu UI, button clicks, and scene navigation.
 * Manages Start, Controls, and Exit buttons, and coordinates with other UI managers.
 * Gleb 01/09/26
 * Version 1.0
 ************************************/
public class MainMenuManager : MonoBehaviour
{
    private const string MainMenuUxmlName = "MainMenu";
    private const string GameViewUxmlName = "Game_View";
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument controlsUIDocument;
    [SerializeField] private UIDocument gameUIDocument;
    [SerializeField] private int mainMenuSceneIndex = 0;
    
    private VisualElement mainMenuContainer;
    private Button startButton;
    private Button controlsButton;
    private Button exitButton;
    
    private ControlsManager controlsManager;
    private GameUIManager gameUIManager;
    private bool pendingStartFromMenu;
    
    void Awake()
    {
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Start()
    {
        BindUI();
        
        if (controlsManager == null)
        {
            GameObject controlsManagerObject = new GameObject("ControlsManager");
            controlsManager = controlsManagerObject.AddComponent<ControlsManager>();
        }
        
        if (controlsManager != null && controlsUIDocument != null)
        {
            StartCoroutine(InitializeControlsManagerDelayed());
        }
        
        if (ShouldShowMainMenuForScene())
        {
            ShowMainMenu();
        }
        else
        {
            StartGameplayUI();
        }
    }
    
    void Update()
    {
        // Only handle ESC if we're in the main menu context (not in game)
        // Check if game UI is visible - if so, let GameUIManager handle ESC
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null && 
            gameUIDocument.rootVisualElement.style.display == DisplayStyle.Flex)
        {
            return; // Game is running, let GameUIManager handle ESC
        }
        
        // Handle ESC key when in settings menu from main menu
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Check if rebinding is in progress - if so, let ControlsManager handle it
            if (controlsManager != null && controlsManager.IsRebindingInProgress())
            {
                return; // Don't close settings, let rebinding handle ESC
            }
            
            // Check if settings menu is open
            if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null && 
                controlsUIDocument.rootVisualElement.style.display == DisplayStyle.Flex)
            {
                // Check if main menu is hidden (meaning we're in settings)
                if (mainMenuContainer != null && mainMenuContainer.style.display == DisplayStyle.None)
                {
                    // Go back to main menu
                    OnBackFromSettings();
                }
            }
        }
    }
    
    public void OnBackFromSettings()
    {
        if (controlsManager != null)
        {
            controlsManager.HideControls();
        }
        
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        ShowMainMenu();
    }
    
    private void OnStartButtonClicked()
    {
        pendingStartFromMenu = true;
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != mainMenuSceneIndex)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneIndex);
            return;
        }

        StartGameplayFromMenuScene();
    }
    
    private void OnControlsButtonClicked()
    {
        // Ensure game is paused when in menu
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        HideMainMenu();

        EnsureControlsUI();
        if (controlsUIDocument == null)
        {
            return;
        }
        
        // Ensure UIDocument is enabled
        controlsUIDocument.enabled = true;
        
        if (controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        
        // Make sure game UI is hidden
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        if (controlsManager == null)
        {
            return;
        }
        
        // Ensure initialization happens before showing
        if (controlsUIDocument != null)
        {
            controlsManager.InitializeFromUIDocument(controlsUIDocument);
        }
        
        // Small delay to ensure everything is ready
        StartCoroutine(ShowControlsDelayed());
    }
    
    private System.Collections.IEnumerator ShowControlsDelayed()
    {
        yield return null; // Wait one frame
        
        if (controlsManager != null && controlsUIDocument != null)
        {
            // Double-check the document is still enabled and visible
            controlsUIDocument.enabled = true;
            if (controlsUIDocument.rootVisualElement != null)
            {
                controlsUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            
            controlsManager.ShowControls();
        }
    }
    
    private void OnExitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void ShowMainMenu()
    {
        if (!ShouldShowMainMenuForScene())
        {
            ForceShowMainMenu();
            return;
        }

        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        BindUI();
        SetMainMenuDocumentActive(true);
        StartCoroutine(RebindMainMenuNextFrame());
        DisableDeathUIDocument();
        HideGameUIDocuments();
        if (gameUIManager == null)
        {
            gameUIManager = FindObjectOfType<GameUIManager>();
        }
        if (gameUIManager != null)
        {
            gameUIManager.ResetForMainMenu();
        }
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.Flex;
        }
        
        HideSecondaryUI();
    }

    public void ForceShowMainMenu()
    {
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        BindUI();
        SetMainMenuDocumentActive(true);
        StartCoroutine(RebindMainMenuNextFrame());
        DisableDeathUIDocument();
        HideGameUIDocuments();
        if (gameUIManager == null)
        {
            gameUIManager = FindObjectOfType<GameUIManager>();
        }
        if (gameUIManager != null)
        {
            gameUIManager.ResetForMainMenu();
        }
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.Flex;
        }

        HideSecondaryUI();
    }
    
    public void HideMainMenu()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.None;
        }
    }
    
    private System.Collections.IEnumerator InitializeControlsManagerDelayed()
    {
        yield return null;
        
        if (controlsManager != null && controlsUIDocument != null)
        {
            controlsManager.InitializeFromUIDocument(controlsUIDocument);
        }
    }

    private System.Collections.IEnumerator RebindMainMenuNextFrame()
    {
        yield return null;
        BindUI();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        BindUI();
        UpdateMainMenuVisibility();

        if (pendingStartFromMenu && scene.buildIndex == mainMenuSceneIndex)
        {
            StartGameplayFromMenuScene();
        }
    }

    private void BindUI()
    {
        EnsureMainMenuDocument();

        if (uiDocument != null)
        {
            var rootVisualElement = uiDocument.rootVisualElement;
            if (rootVisualElement != null)
            {
                mainMenuContainer = rootVisualElement.Q<VisualElement>("MainMenuContainer");
                startButton = rootVisualElement.Q<Button>("Start");
                controlsButton = rootVisualElement.Q<Button>("Controls");
                exitButton = rootVisualElement.Q<Button>("Exit");
            }
        }

        if (startButton != null)
        {
            startButton.clicked -= OnStartButtonClicked;
            startButton.clicked += OnStartButtonClicked;
        }

        if (controlsButton != null)
        {
            controlsButton.clicked -= OnControlsButtonClicked;
            controlsButton.clicked += OnControlsButtonClicked;
        }

        if (exitButton != null)
        {
            exitButton.clicked -= OnExitButtonClicked;
            exitButton.clicked += OnExitButtonClicked;
        }

        controlsManager = FindObjectOfType<ControlsManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();

        if (controlsManager == null)
        {
            GameObject controlsManagerObject = new GameObject("ControlsManager");
            controlsManager = controlsManagerObject.AddComponent<ControlsManager>();
        }

        EnsureControlsUI();
        EnsureGameUI();
    }

    private void DisableDeathUIDocument()
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
                document.enabled = false;
            }
        }
    }

    private void EnsureControlsUI()
    {
        if (controlsUIDocument == null || controlsUIDocument.rootVisualElement == null)
        {
            controlsUIDocument = FindDocumentWithElement("ControlsContainer");
        }
    }

    private void EnsureGameUI()
    {
        if (gameUIDocument == null || gameUIDocument.rootVisualElement == null)
        {
            gameUIDocument = FindDocumentWithElement("GameContentArea");
        }
    }

    private void EnsureGameUIDocument()
    {
        if (gameUIDocument != null)
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
                gameUIDocument = document;
                return;
            }
        }
    }

    private void HideSecondaryUI()
    {
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void HideControlsUI()
    {
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void SetMainMenuDocumentActive(bool active)
    {
        EnsureMainMenuDocument();
        if (uiDocument != null)
        {
            uiDocument.enabled = active;
            if (uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        if (!active)
        {
            HideMainMenu();
        }
    }

    private void HideGameUIDocuments()
    {
        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals("Game_View", System.StringComparison.OrdinalIgnoreCase))
            {
                document.enabled = false;
                if (document.rootVisualElement != null)
                {
                    document.rootVisualElement.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private void StartGameplayUI()
    {
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        SetMainMenuDocumentActive(false);
        EnableAllGameViewDocuments();

        var spawner = FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
        {
            spawner.StartGame();
        }

        if (gameUIManager != null)
        {
            gameUIManager.RebindGameUI();
            gameUIManager.StartGame();
        }

        HideControlsUI();
    }

    private void StartGameplayFromMenuScene()
    {
        pendingStartFromMenu = false;
        HideMainMenu();
        StartGameplayUI();
        EnableAllGameViewDocuments();

        var spawner = FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
        {
            spawner.StartGame();
        }

        if (gameUIManager != null)
        {
            gameUIManager.RebindGameUI();
            gameUIManager.StartGame();
        }

        ResetPlayerToSpawnPoint();
        StartCoroutine(ShowGameUIAfterStart());
    }

    private void ResetPlayerToSpawnPoint()
    {
        var player = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        if (player == null)
        {
            return;
        }

        var spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>(FindObjectsInactive.Include);
        if (spawnPoint != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            player.transform.SetPositionAndRotation(spawnPoint.Position, spawnPoint.Rotation);
            if (controller != null) controller.enabled = true;
        }

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ResetForRespawn();
        }
    }

    private void EnableGameViewDocument()
    {
        EnsureGameUIDocument();
        EnsureGameUI();
        if (gameUIDocument != null)
        {
            gameUIDocument.enabled = true;
            if (gameUIDocument.rootVisualElement != null)
            {
                gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
    }

    private IEnumerator ShowGameUIAfterStart()
    {
        yield return null;
        EnableAllGameViewDocuments();

        if (gameUIManager != null)
        {
            gameUIManager.RebindGameUI();
            gameUIManager.StartGame();
        }
    }

    private void EnableAllGameViewDocuments()
    {
        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals(GameViewUxmlName, System.StringComparison.OrdinalIgnoreCase))
            {
                document.enabled = true;
                if (document.rootVisualElement != null)
                {
                    document.rootVisualElement.style.display = DisplayStyle.Flex;
                }
            }
        }

        EnableGameViewDocument();
    }

    private void UpdateMainMenuVisibility()
    {
        if (ShouldShowMainMenuForScene())
        {
            ShowMainMenu();
        }
        else
        {
            StartGameplayUI();
        }
    }

    private bool ShouldShowMainMenuForScene()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        return sceneIndex == mainMenuSceneIndex;
    }

    private UIDocument FindDocumentWithElement(string elementName)
    {
        var documents = FindObjectsOfType<UIDocument>(true);
        foreach (var document in documents)
        {
            if (document == null || document.rootVisualElement == null) continue;
            if (document.rootVisualElement.Q<VisualElement>(elementName) != null)
            {
                return document;
            }
        }
        return null;
    }

    private void EnsureMainMenuDocument()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            return;
        }

        uiDocument = FindDocumentWithElement("MainMenuContainer");
        if (uiDocument != null && uiDocument.rootVisualElement != null)
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

            if (document.visualTreeAsset.name.Equals(MainMenuUxmlName, System.StringComparison.OrdinalIgnoreCase))
            {
                uiDocument = document;
                return;
            }
        }
    }
}
