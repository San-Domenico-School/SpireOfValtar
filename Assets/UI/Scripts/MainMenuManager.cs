using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument controlsUIDocument; // Should reference "UI GLEB/Controls.uxml"
    [SerializeField] private UIDocument gameUIDocument;
    
    private VisualElement mainMenuContainer;
    private Button startButton;
    private Button controlsButton;
    private Button exitButton;
    
    private ControlsManager controlsManager;
    private GameUIManager gameUIManager;
    
    void Start()
    {
        // Get references to UI elements
        var rootVisualElement = uiDocument.rootVisualElement;
        
        Debug.Log("=== MAIN MENU MANAGER START ===");
        Debug.Log("UI Document: " + (uiDocument != null ? uiDocument.name : "NULL"));
        Debug.Log("Root Visual Element: " + (rootVisualElement != null ? rootVisualElement.name : "NULL"));
        
        mainMenuContainer = rootVisualElement.Q<VisualElement>("MainMenuContainer");
        Debug.Log("MainMenuContainer found: " + (mainMenuContainer != null));
        
        startButton = rootVisualElement.Q<Button>("Start");
        Debug.Log("Start button found: " + (startButton != null));
        
        controlsButton = rootVisualElement.Q<Button>("Controls");
        Debug.Log("Controls button found: " + (controlsButton != null));
        
        exitButton = rootVisualElement.Q<Button>("Exit");
        Debug.Log("Exit button found: " + (exitButton != null));
        
        // Get references to other UI managers
        controlsManager = FindObjectOfType<ControlsManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
        
        Debug.Log("MainMenuManager Start: controlsUIDocument found: " + (controlsUIDocument != null));
        
        // Create ControlsManager if it doesn't exist
        if (controlsManager == null)
        {
            Debug.Log("MainMenuManager Start: Creating ControlsManager GameObject automatically");
            GameObject controlsManagerObject = new GameObject("ControlsManager");
            controlsManager = controlsManagerObject.AddComponent<ControlsManager>();
        }
        
        // Initialize controls manager with the controls UIDocument
        if (controlsManager != null && controlsUIDocument != null)
        {
            Debug.Log("MainMenuManager Start: Initializing ControlsManager with UIDocument");
            // Delay initialization to ensure UIDocument is ready
            StartCoroutine(InitializeControlsManagerDelayed());
        }
        else
        {
            if (controlsUIDocument == null)
                Debug.LogError("MainMenuManager Start: controlsUIDocument is null - assign it in inspector!");
        }
        
        // Add click event handlers
        Debug.Log("=== ADDING BUTTON EVENT HANDLERS ===");
        
        if (startButton != null)
        {
            Debug.Log("Adding event handler to Start button...");
            startButton.clicked += OnStartButtonClicked;
            Debug.Log("Start button event handler added successfully");
            Debug.Log("Start button clickable: " + (startButton.clickable != null));
        }
        else
        {
            Debug.LogError("Start button is NULL - cannot add event handler!");
        }
        
        if (controlsButton != null)
        {
            Debug.Log("Adding event handler to Controls button...");
            controlsButton.clicked += OnControlsButtonClicked;
            Debug.Log("Controls button event handler added successfully");
            Debug.Log("Controls button clickable: " + (controlsButton.clickable != null));
        }
        else
        {
            Debug.LogError("Controls button is NULL - cannot add event handler!");
        }
        
        if (exitButton != null)
        {
            Debug.Log("Adding event handler to Exit button...");
            exitButton.clicked += OnExitButtonClicked;
            Debug.Log("Exit button event handler added successfully");
            Debug.Log("Exit button clickable: " + (exitButton.clickable != null));
        }
        else
        {
            Debug.LogError("Exit button is NULL - cannot add event handler!");
        }
        
        Debug.Log("=== MAIN MENU MANAGER START COMPLETE ===");
        
        // Initially hide other UIs
        Debug.Log("Initially hiding other UIs...");
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            Debug.Log("Controls UI initially hidden");
        }
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            Debug.Log("Game UI initially hidden");
        }
    }
    
    private void OnStartButtonClicked()
    {
        Debug.Log("=== START BUTTON CLICKED ===");
        Debug.Log("Start button clicked - Starting game");
        
        // Hide main menu
        Debug.Log("Hiding main menu...");
        HideMainMenu();
        
        // Show game UI
        if (gameUIDocument != null)
        {
            Debug.Log("Showing game UI document...");
            if (gameUIDocument.rootVisualElement != null)
            {
                gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                Debug.Log("Game UI document display set to: " + gameUIDocument.rootVisualElement.style.display);
            }
            else
            {
                Debug.LogError("gameUIDocument.rootVisualElement is NULL! GameUI GameObject may be inactive.");
            }
        }
        else
        {
            Debug.LogError("gameUIDocument is NULL!");
        }
        
        // Initialize game UI manager
        if (gameUIManager != null)
        {
            Debug.Log("Calling gameUIManager.StartGame()...");
            gameUIManager.StartGame();
        }
        else
        {
            Debug.LogError("gameUIManager is NULL!");
        }
        
        Debug.Log("=== START BUTTON COMPLETE ===");
    }
    
    private void OnControlsButtonClicked()
    {
        Debug.Log("=== CONTROLS BUTTON CLICKED ===");
        Debug.Log("Controls button clicked - Showing controls");
        
        // Hide main menu
        Debug.Log("Hiding main menu...");
        HideMainMenu();
        
        // Debug: Check if controlsUIDocument exists
        if (controlsUIDocument == null)
        {
            Debug.LogError("MainMenuManager: controlsUIDocument is null! Make sure it's assigned in the inspector.");
            return;
        }
        
        Debug.Log("MainMenuManager: controlsUIDocument found: " + controlsUIDocument.name);
        
        // Show controls UI document first
        Debug.Log("Showing controls UI document...");
        if (controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            Debug.Log("Controls UI document display set to: " + controlsUIDocument.rootVisualElement.style.display);
        }
        else
        {
            Debug.LogError("controlsUIDocument.rootVisualElement is NULL! ControlsUI GameObject may be inactive.");
        }
        
        // Debug: Check if controlsManager exists
        if (controlsManager == null)
        {
            Debug.LogError("MainMenuManager: controlsManager is null! Make sure ControlsManager script is in the scene.");
            return;
        }
        
        Debug.Log("MainMenuManager: controlsManager found, calling ShowControls()");
        
        // Then initialize and show controls content
        controlsManager.ShowControls();
        
        Debug.Log("=== CONTROLS BUTTON COMPLETE ===");
    }
    
    private void OnExitButtonClicked()
    {
        Debug.Log("=== EXIT BUTTON CLICKED ===");
        Debug.Log("Exit button clicked - Closing game");
        
        // Exit the application
        Debug.Log("Attempting to stop play mode...");
        #if UNITY_EDITOR
            Debug.Log("Running in Unity Editor - stopping play mode");
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Debug.Log("Running in build - quitting application");
            Application.Quit();
        #endif
        
        Debug.Log("=== EXIT BUTTON COMPLETE ===");
    }
    
    // Method to show the main menu again (called from other managers)
    public void ShowMainMenu()
    {
        Debug.Log("=== SHOW MAIN MENU CALLED ===");
        
        if (mainMenuContainer != null)
        {
            Debug.Log("Showing main menu container...");
            mainMenuContainer.style.display = DisplayStyle.Flex;
            Debug.Log("Main menu container display set to: " + mainMenuContainer.style.display);
        }
        else
        {
            Debug.LogError("mainMenuContainer is NULL!");
        }
        
        // Hide other UIs
        Debug.Log("Hiding other UIs...");
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            Debug.Log("Controls UI hidden");
        }
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            Debug.Log("Game UI hidden");
        }
        
        Debug.Log("=== SHOW MAIN MENU COMPLETE ===");
    }
    
    // Method to hide the main menu
    public void HideMainMenu()
    {
        Debug.Log("=== HIDE MAIN MENU CALLED ===");
        
        if (mainMenuContainer != null)
        {
            Debug.Log("Hiding main menu container...");
            mainMenuContainer.style.display = DisplayStyle.None;
            Debug.Log("Main menu container display set to: " + mainMenuContainer.style.display);
        }
        else
        {
            Debug.LogError("mainMenuContainer is NULL!");
        }
        
        Debug.Log("=== HIDE MAIN MENU COMPLETE ===");
    }
    
    // Coroutine to initialize ControlsManager after a delay
    private System.Collections.IEnumerator InitializeControlsManagerDelayed()
    {
        // Wait one frame to ensure UIDocument is ready
        yield return null;
        
        if (controlsManager != null && controlsUIDocument != null)
        {
            controlsManager.InitializeFromUIDocument(controlsUIDocument);
        }
    }
}
