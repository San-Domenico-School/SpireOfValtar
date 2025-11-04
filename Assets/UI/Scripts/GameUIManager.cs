using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument pauseMenuDocument;
    [SerializeField] private UIDocument controlsDocument;
    
    private VisualElement gameUIContainer;
    private VisualElement topBar;
    private VisualElement gameContentArea;
    private Button menuButton;
    private Button actionButton1;
    private Button actionButton2;
    private Button settingsButton;
    private MainMenuManager mainMenuManager;
    private PlayerMovement playerMovement;
    
    // Pause menu elements
    private VisualElement pauseMenuContainer;
    private Button pauseControlsButton;
    private Button pauseMainMenuButton;
    private Button pauseExitButton;
    private VisualElement controlsContainer;
    private Button controlsBackButton;
    
    private bool isPaused = false;
    
    void Start()
    {
        // Get references to UI elements
        var rootVisualElement = uiDocument.rootVisualElement;
        
        gameUIContainer = rootVisualElement.Q<VisualElement>("GameUIContainer");
        topBar = rootVisualElement.Q<VisualElement>("TopBar");
        gameContentArea = rootVisualElement.Q<VisualElement>("GameContentArea");
        menuButton = rootVisualElement.Q<Button>("MenuButton");
        Debug.Log("MenuButton found: " + (menuButton != null));
        
        actionButton1 = rootVisualElement.Q<Button>("ActionButton1");
        Debug.Log("ActionButton1 found: " + (actionButton1 != null));
        
        actionButton2 = rootVisualElement.Q<Button>("ActionButton2");
        Debug.Log("ActionButton2 found: " + (actionButton2 != null));
        
        settingsButton = rootVisualElement.Q<Button>("SettingsButton");
        Debug.Log("SettingsButton found: " + (settingsButton != null));
        
        // Get reference to main menu manager
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        Debug.Log("MainMenuManager found: " + (mainMenuManager != null));
        
        // Get reference to player movement
        playerMovement = FindObjectOfType<PlayerMovement>();
        Debug.Log("PlayerMovement found: " + (playerMovement != null));
        
        // Add click event handlers
        if (menuButton != null)
        {
            menuButton.clicked += OnMenuButtonClicked;
        }
        
        if (actionButton1 != null)
        {
            actionButton1.clicked += OnActionButton1Clicked;
        }
        
        if (actionButton2 != null)
        {
            actionButton2.clicked += OnActionButton2Clicked;
        }
        
        if (settingsButton != null)
        {
            settingsButton.clicked += OnSettingsButtonClicked;
        }
        
        // Initially hide this UI
        HideGameUI();
        
        // Initialize pause menu
        InitializePauseMenu();
        
        // Initialize controls menu
        InitializeControlsMenu();
    }
    
    void Update()
    {
        // Handle ESC key to toggle pause menu
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                // If controls menu is open, go back to pause menu
                if (controlsDocument != null && controlsDocument.rootVisualElement != null && 
                    controlsDocument.rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    HideControlsMenu();
                    ShowPauseMenu();
                }
                else
                {
                    // Close pause menu
                    ResumeGame();
                }
            }
            else
            {
                // Open pause menu
                PauseGame();
            }
        }
    }
    
    private void InitializePauseMenu()
    {
        if (pauseMenuDocument != null)
        {
            var pauseRoot = pauseMenuDocument.rootVisualElement;
            pauseMenuContainer = pauseRoot.Q<VisualElement>("PauseMenuContainer");
            pauseControlsButton = pauseRoot.Q<Button>("ControlsButton");
            pauseMainMenuButton = pauseRoot.Q<Button>("MainMenuButton");
            pauseExitButton = pauseRoot.Q<Button>("ExitButton");
            
            if (pauseControlsButton != null)
            {
                pauseControlsButton.clicked += OnPauseControlsClicked;
            }
            
            if (pauseMainMenuButton != null)
            {
                pauseMainMenuButton.clicked += OnPauseMainMenuClicked;
            }
            
            if (pauseExitButton != null)
            {
                pauseExitButton.clicked += OnPauseExitClicked;
            }
            
            // Initially hide pause menu - hide entire UIDocument
            if (pauseMenuDocument.rootVisualElement != null)
            {
                pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
    
    private void InitializeControlsMenu()
    {
        if (controlsDocument != null)
        {
            var controlsRoot = controlsDocument.rootVisualElement;
            controlsContainer = controlsRoot.Q<VisualElement>("ControlsContainer");
            controlsBackButton = controlsRoot.Q<Button>("Back");
            
            if (controlsBackButton != null)
            {
                controlsBackButton.clicked += OnControlsBackClicked;
            }
            
            // Initially hide controls menu - hide entire UIDocument
            if (controlsDocument.rootVisualElement != null)
            {
                controlsDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        
        // Ensure cursor is unlocked for menu interaction
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        
        // Lock cursor back for game
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        if (controlsDocument != null && controlsDocument.rootVisualElement != null)
        {
            controlsDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void ShowPauseMenu()
    {
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }
    
    private void HideControlsMenu()
    {
        if (controlsDocument != null && controlsDocument.rootVisualElement != null)
        {
            controlsDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void CleanupGame()
    {
        // Resume game if paused
        if (isPaused)
        {
            ResumeGame();
        }
        
        // Add your game cleanup logic here
        // For example: Stop timers, save game state, etc.
        Debug.Log("Game cleaned up");
    }
    
    private void OnPauseControlsClicked()
    {
        Debug.Log("Controls button clicked");
        
        // Ensure cursor stays unlocked for menu interaction
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        // Ensure controls UIDocument is enabled
        if (controlsDocument != null)
        {
            controlsDocument.enabled = true;
            
            // Show controls menu first
            if (controlsDocument.rootVisualElement != null)
            {
                Debug.Log("Showing controls menu");
                controlsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                Debug.LogWarning("Controls document root element is null!");
            }
        }
        else
        {
            Debug.LogError("Controls document is null!");
        }
        
        // Hide pause menu
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            Debug.Log("Hiding pause menu");
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void OnControlsBackClicked()
    {
        // Hide controls menu and show pause menu
        HideControlsMenu();
        ShowPauseMenu();
    }
    
    private void OnPauseMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked from pause menu - Returning to main menu");
        
        // Resume game first (unpause)
        ResumeGame();
        
        // Hide game UI
        HideGameUI();
        
        // Hide pause menu
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        // Show main menu
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        
        // Cleanup game
        CleanupGame();
    }
    
    private void OnPauseExitClicked()
    {
        // Exit the game
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void StartGame()
    {
        Debug.Log("Starting game - Showing game UI");
        
        if (gameUIContainer != null)
        {
            gameUIContainer.style.display = DisplayStyle.Flex;
        }
        
        // Ensure controls and pause menus are hidden when starting game
        if (controlsDocument != null && controlsDocument.rootVisualElement != null)
        {
            controlsDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    public void HideGameUI()
    {
        if (gameUIContainer != null)
        {
            gameUIContainer.style.display = DisplayStyle.None;
        }
    }
    
    private void OnMenuButtonClicked()
    {
        Debug.Log("Menu button clicked - Returning to main menu");
        
        // Hide game UI
        HideGameUI();
        
        // Show main menu
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        
        // Add any game cleanup logic here
        CleanupGame();
    }
    
    private void OnActionButton1Clicked()
    {
        Debug.Log("Action Button 1 clicked");
        // Add your game action logic here
        UpdateGameContent("Action 1 performed!");
    }
    
    private void OnActionButton2Clicked()
    {
        Debug.Log("Action Button 2 clicked");
        // Add your game action logic here
        UpdateGameContent("Action 2 performed!");
    }
    
    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked");
        // Add your settings logic here
        UpdateGameContent("Settings opened!");
    }
    
    private void InitializeGame()
    {
        // Add your game initialization logic here
        // For example: Start game timers, initialize game state, etc.
        Debug.Log("Game initialized");
    }
    
    
    void OnDestroy()
    {
        // Ensure time scale is reset when destroyed
        Time.timeScale = 1f;
    }
    
    // Method to update game content
    public void UpdateGameContent(string content)
    {
        if (gameContentArea != null)
        {
            var label = gameContentArea.Q<Label>();
            if (label != null)
            {
                label.text = content;
            }
        }
    }
    
    // Method to show/hide specific game elements
    public void SetGameElementVisible(string elementName, bool visible)
    {
        var element = gameUIContainer?.Q<VisualElement>(elementName);
        if (element != null)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
