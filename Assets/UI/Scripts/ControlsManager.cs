using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

/************************************
 * Manages the controls menu UI display and navigation.
 * Handles showing/hiding controls menu and back button functionality.
 * Gleb 11/4/25
 * Version 1.0
 ************************************/
public class ControlsManager : MonoBehaviour
{
    [SerializeField] private Texture2D controlsImage;
    
    private UIDocument controlledUIDocument;
    private VisualElement controlsContainer;
    private VisualElement controlsImageContainer;
    private Button backButton;
    private MainMenuManager mainMenuManager;
    
    void Start()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        HideControls();
    }
    
    public void InitializeFromUIDocument(UIDocument document)
    {
        if (document == null) 
        {
            return;
        }
        
        controlledUIDocument = document;
        var rootVisualElement = document.rootVisualElement;
        
        if (rootVisualElement == null)
        {
            return;
        }
        
        controlsContainer = rootVisualElement.Q<VisualElement>("ControlsContainer");
        
        var allElements = rootVisualElement.Query<VisualElement>().ToList();
        controlsImageContainer = allElements.FirstOrDefault(e => e.style.backgroundImage.value.texture != null);
        
        backButton = rootVisualElement.Q<Button>("Back");
        
        if (backButton != null)
        {
            backButton.clicked += OnBackButtonClicked;
        }
    }
    
    public void ShowControls()
    {
        if (controlledUIDocument != null)
        {
            controlledUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            return;
        }
        
        if (controlsContainer == null && controlledUIDocument != null)
        {
            controlsContainer = controlledUIDocument.rootVisualElement.Q<VisualElement>("ControlsContainer");
        }
        
        if (controlsContainer != null)
        {
            controlsContainer.style.display = DisplayStyle.Flex;
        }
        
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
        // Keep game frozen (same pattern as Start button but with timeScale = 0f)
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        HideControls();
        
        // Hide controls UI document (same pattern as HideMainMenu)
        if (controlledUIDocument != null && controlledUIDocument.rootVisualElement != null)
        {
            controlledUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        // Show main menu (same pattern as showing game UI)
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
    }
    
    public void SetControlsImage(Texture2D image)
    {
        controlsImage = image;
        if (controlsImageContainer != null && image != null)
        {
            controlsImageContainer.style.backgroundImage = new StyleBackground(image);
        }
    }
}
