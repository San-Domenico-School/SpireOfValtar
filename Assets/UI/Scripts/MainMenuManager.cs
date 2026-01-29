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
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument controlsUIDocument;
    [SerializeField] private UIDocument gameUIDocument;
    [SerializeField] private string mainMenuSceneName = "1st Prototype Build Teddy NEW";
    
    private VisualElement mainMenuContainer;
    private Button startButton;
    private Button controlsButton;
    private Button exitButton;
    
    private ControlsManager controlsManager;
    private GameUIManager gameUIManager;
    
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
        
        if (mainMenuContainer != null && ShouldShowMainMenuForScene())
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
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        
        HideMainMenu();

        EnsureGameUI();
        if (gameUIDocument != null)
        {
            gameUIDocument.enabled = true;
            if (gameUIDocument.rootVisualElement != null)
            {
                gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        var spawner = FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
        {
            spawner.StartGame();
        }

        if (gameUIManager != null)
        {
            gameUIManager.StartGame();
        }
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
            HideMainMenu();
            StartGameplayUI();
            return;
        }

        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        BindUI();
        if (uiDocument != null)
        {
            uiDocument.enabled = true;
            if (uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
        DisableDeathUIDocument();
        HideGameUIDocuments();
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

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        BindUI();
        UpdateMainMenuVisibility();
    }

    private void BindUI()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            uiDocument = FindDocumentWithElement("MainMenuContainer");
        }

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

        HideMainMenu();
        EnsureGameUI();
        if (gameUIDocument != null)
        {
            gameUIDocument.enabled = true;
            if (gameUIDocument.rootVisualElement != null)
            {
                gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        var spawner = FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
        {
            spawner.StartGame();
        }

        if (gameUIManager != null)
        {
            gameUIManager.StartGame();
        }

        HideControlsUI();
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
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            return true;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return sceneName == mainMenuSceneName;
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
}
