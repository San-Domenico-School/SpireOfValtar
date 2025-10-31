using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class ControlsManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Texture2D controlsImage;
    
    // Reference to the UIDocument that will be controlled by MainMenuManager
    private UIDocument controlledUIDocument;
    
    private VisualElement controlsContainer;
    private VisualElement controlsImageContainer;
    private Button backButton;
    private MainMenuManager mainMenuManager;
    
    void Start()
    {
        // Get reference to main menu manager
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        
        // Initially hide this UI
        HideControls();
    }
    
    // Method to initialize UI elements from a specific UIDocument
    public void InitializeFromUIDocument(UIDocument document)
    {
        if (document == null) 
        {
            Debug.LogWarning("ControlsManager: UIDocument is null!");
            return;
        }
        
        controlledUIDocument = document;
        var rootVisualElement = document.rootVisualElement;
        
        if (rootVisualElement == null)
        {
            Debug.LogWarning("ControlsManager: rootVisualElement is null! UIDocument may not be initialized yet.");
            return;
        }
        
        Debug.Log("ControlsManager: Initializing from UIDocument with root element: " + rootVisualElement.name);
        
        controlsContainer = rootVisualElement.Q<VisualElement>("ControlsContainer");
        Debug.Log("ControlsManager: ControlsContainer found: " + (controlsContainer != null));
        
        // Find the visual element with background image (the one with Floor Tiles texture)
        var allElements = rootVisualElement.Query<VisualElement>().ToList();
        controlsImageContainer = allElements.FirstOrDefault(e => e.style.backgroundImage.value.texture != null);
        
        backButton = rootVisualElement.Q<Button>("Back");
        Debug.Log("ControlsManager: Back button found: " + (backButton != null));
        if (backButton != null)
        {
            Debug.Log("ControlsManager: Back button clickable: " + (backButton.clickable != null));
        }
        
        // Add click event handler for back button
        if (backButton != null)
        {
            Debug.Log("ControlsManager: Adding event handler to Back button...");
            backButton.clicked += OnBackButtonClicked;
            Debug.Log("ControlsManager: Back button event handler added successfully");
        }
        else
        {
            Debug.LogError("ControlsManager: Back button is NULL - cannot add event handler!");
        }
    }
    
    public void ShowControls()
    {
        Debug.Log("ControlsManager: ShowControls() called");
        
        // Ensure the entire controlled UIDocument is visible first
        if (controlledUIDocument != null)
        {
            controlledUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            Debug.Log("ControlsManager: Set controlledUIDocument root to DisplayStyle.Flex");
        }
        else
        {
            Debug.LogError("ControlsManager: controlledUIDocument is null!");
            return;
        }
        
        // Try to find controlsContainer again if it wasn't found during initialization
        if (controlsContainer == null && controlledUIDocument != null)
        {
            Debug.Log("ControlsManager: Attempting to find ControlsContainer again...");
            controlsContainer = controlledUIDocument.rootVisualElement.Q<VisualElement>("ControlsContainer");
            Debug.Log("ControlsManager: ControlsContainer found on retry: " + (controlsContainer != null));
        }
        
        // Make sure the controls container is visible
        if (controlsContainer != null)
        {
            controlsContainer.style.display = DisplayStyle.Flex;
            Debug.Log("ControlsManager: Controls container displayed");
        }
        else
        {
            Debug.LogWarning("ControlsManager: Controls container still not found! Check if the element name is 'ControlsContainer' in your UXML.");
            
            // Try to find any visual element that might be the container
            var allElements = controlledUIDocument.rootVisualElement.Query<VisualElement>().ToList();
            Debug.Log("ControlsManager: Found " + allElements.Count + " visual elements in the document");
            foreach (var element in allElements)
            {
                Debug.Log("ControlsManager: Element name: " + element.name + ", classes: " + string.Join(", ", element.GetClasses()));
            }
        }
        
        // Set the controls image if available
        if (controlsImage != null && controlsImageContainer != null)
        {
            controlsImageContainer.style.backgroundImage = new StyleBackground(controlsImage);
        }
    }
    
    public void HideControls()
    {
        if (controlsContainer != null)
        {
            controlsContainer.style.display = DisplayStyle.None;
        }
    }
    
    private void OnBackButtonClicked()
    {
        Debug.Log("=== BACK BUTTON CLICKED ===");
        Debug.Log("Back button clicked - Returning to main menu");
        
        // Hide controls UI
        Debug.Log("Hiding controls UI...");
        HideControls();
        
        // Show main menu
        if (mainMenuManager != null)
        {
            Debug.Log("Calling mainMenuManager.ShowMainMenu()...");
            mainMenuManager.ShowMainMenu();
        }
        else
        {
            Debug.LogError("mainMenuManager is NULL!");
        }
        
        Debug.Log("=== BACK BUTTON COMPLETE ===");
    }
    
    // Method to set controls image programmatically
    public void SetControlsImage(Texture2D image)
    {
        controlsImage = image;
        if (controlsImageContainer != null && image != null)
        {
            controlsImageContainer.style.backgroundImage = new StyleBackground(image);
        }
    }
}
