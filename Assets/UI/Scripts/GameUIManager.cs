using UnityEngine;
using UnityEngine.UIElements;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement gameUIContainer;
    private VisualElement topBar;
    private VisualElement gameContentArea;
    private Button menuButton;
    private Button actionButton1;
    private Button actionButton2;
    private Button settingsButton;
    private MainMenuManager mainMenuManager;
    private PlayerMovement playerMovement;
    
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
    }
    
    public void StartGame()
    {
        Debug.Log("Starting game - Showing game UI");
        
        if (gameUIContainer != null)
        {
            gameUIContainer.style.display = DisplayStyle.Flex;
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
    
    private void CleanupGame()
    {
        // Add your game cleanup logic here
        // For example: Stop timers, save game state, etc.
        Debug.Log("Game cleaned up");
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
