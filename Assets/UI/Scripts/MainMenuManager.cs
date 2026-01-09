using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

/************************************
 * Handles main menu UI, button clicks, and scene navigation.
 * Manages Start, Controls, and Exit buttons, and coordinates with other UI managers.
 * Gleb 11/4/25
 * Version 1.0
 ************************************/
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument controlsUIDocument;
    [SerializeField] private UIDocument gameUIDocument;
    
    private VisualElement mainMenuContainer;
    private Button startButton;
    private Button controlsButton;
    private Button exitButton;
    
    private ControlsManager controlsManager;
    private GameUIManager gameUIManager;
    
    void Awake()
    {
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        var rootVisualElement = uiDocument.rootVisualElement;
        
        mainMenuContainer = rootVisualElement.Q<VisualElement>("MainMenuContainer");
        startButton = rootVisualElement.Q<Button>("Start");
        controlsButton = rootVisualElement.Q<Button>("Controls");
        exitButton = rootVisualElement.Q<Button>("Exit");
        
        controlsManager = FindObjectOfType<ControlsManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
        
        if (controlsManager == null)
        {
            GameObject controlsManagerObject = new GameObject("ControlsManager");
            controlsManager = controlsManagerObject.AddComponent<ControlsManager>();
        }
        
        if (controlsManager != null && controlsUIDocument != null)
        {
            StartCoroutine(InitializeControlsManagerDelayed());
        }
        
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
        
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
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
        
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
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
        
        if (controlsUIDocument == null)
        {
            Debug.LogWarning("MainMenuManager: controlsUIDocument is null!");
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
            Debug.LogWarning("MainMenuManager: controlsManager is null!");
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
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.Flex;
        }
        
        if (controlsUIDocument != null && controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        if (gameUIDocument != null && gameUIDocument.rootVisualElement != null)
        {
            gameUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
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
}
