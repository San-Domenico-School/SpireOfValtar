using UnityEngine;
using UnityEngine.UIElements;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement gameUIContainer;
    private VisualElement topBar;
    private VisualElement gameContentArea;
    private Button menuButton;
    private MainMenuManager mainMenuManager;
    
    void Start()
    {
        // Get references to UI elements
        var rootVisualElement = uiDocument.rootVisualElement;
        
        gameUIContainer = rootVisualElement.Q<VisualElement>("GameUIContainer");
        topBar = rootVisualElement.Q<VisualElement>("TopBar");
        gameContentArea = rootVisualElement.Q<VisualElement>("GameContentArea");
        menuButton = rootVisualElement.Q<Button>("MenuButton");
        
        // Get reference to main menu manager
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        
        // Add click event handler for menu button
        if (menuButton != null)
        {
            menuButton.clicked += OnMenuButtonClicked;
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
