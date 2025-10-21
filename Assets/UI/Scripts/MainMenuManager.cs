using UnityEngine;
using UnityEngine.UIElements;

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
        
        mainMenuContainer = rootVisualElement.Q<VisualElement>("MainMenuContainer");
        startButton = rootVisualElement.Q<Button>("Start");
        controlsButton = rootVisualElement.Q<Button>("Controls");
        exitButton = rootVisualElement.Q<Button>("Exit");
        
        // Get references to other UI managers
        controlsManager = FindObjectOfType<ControlsManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
        
        Debug.Log("MainMenuManager Start: controlsManager found: " + (controlsManager != null));
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
            controlsManager.InitializeFromUIDocument(controlsUIDocument);
        }
        else
        {
            if (controlsUIDocument == null)
                Debug.LogError("MainMenuManager Start: controlsUIDocument is null - assign it in inspector!");
        }
        
        // Add click event handlers
        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
        }
        
        if (controlsButton != null)
        {
            controlsButton.clicked += OnControlsButtonClicked;
        }
        
        if (exitButton != null)
        {
            exitButton.clicked += OnExitButtonClicked;
        }
        
        // Initially hide other UIs
        if (controlsUIDocument != null)
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        if (gameUIDocument != null)
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
    }
    
    private void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked - Starting game");
        
        // Hide main menu
        HideMainMenu();
        
        // Show game UI
        if (gameUIDocument != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        
        // Initialize game UI manager
        if (gameUIManager != null)
        {
            gameUIManager.StartGame();
        }
    }
    
    private void OnControlsButtonClicked()
    {
        Debug.Log("Controls button clicked - Showing controls");
        
        // Hide main menu
        HideMainMenu();
        
        // Debug: Check if controlsUIDocument exists
        if (controlsUIDocument == null)
        {
            Debug.LogError("MainMenuManager: controlsUIDocument is null! Make sure it's assigned in the inspector.");
            return;
        }
        
        Debug.Log("MainMenuManager: controlsUIDocument found: " + controlsUIDocument.name);
        
        // Show controls UI document first
        controlsUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        Debug.Log("MainMenuManager: Set controlsUIDocument to DisplayStyle.Flex");
        
        // Debug: Check if controlsManager exists
        if (controlsManager == null)
        {
            Debug.LogError("MainMenuManager: controlsManager is null! Make sure ControlsManager script is in the scene.");
            return;
        }
        
        Debug.Log("MainMenuManager: controlsManager found, calling ShowControls()");
        
        // Then initialize and show controls content
        controlsManager.ShowControls();
    }
    
    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked - Closing game");
        
        // Exit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Method to show the main menu again (called from other managers)
    public void ShowMainMenu()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.Flex;
        }
        
        // Hide other UIs
        if (controlsUIDocument != null)
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        if (gameUIDocument != null)
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
    }
    
    // Method to hide the main menu
    public void HideMainMenu()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.None;
        }
    }
}
