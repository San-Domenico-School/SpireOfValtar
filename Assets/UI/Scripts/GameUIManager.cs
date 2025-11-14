using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

/************************************
 * Handles game UI, pause menu, and ESC key controls.
 * Manages pause/unpause, controls menu navigation, and menu button functionality.
 * Gleb 11/4/25
 * Version 1.0
 ************************************/
public class GameUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument pauseMenuDocument;
    [SerializeField] private UIDocument controlsDocument;
    [SerializeField] private UIDocument controlsPauseDocument;
    
    private VisualElement gameUIContainer;
    private Button menuButton;
    private MainMenuManager mainMenuManager;
    
    private Button pauseControlsButton;
    private Button pauseMainMenuButton;
    private Button pauseExitButton;
    
    private bool isPaused = false;
    
    public bool IsPaused => isPaused;
    
    void Start()
    {
        var rootVisualElement = uiDocument.rootVisualElement;
        
        gameUIContainer = rootVisualElement.Q<VisualElement>("GameUIContainer");
        menuButton = rootVisualElement.Q<Button>("MenuButton");
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        
        if (menuButton != null)
        {
            menuButton.clicked += OnMenuButtonClicked;
        }
        
        HideGameUI();
        InitializePauseMenu();
        InitializeControlsMenu();
        InitializeControlsPauseMenu();
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                if (controlsPauseDocument != null && controlsPauseDocument.rootVisualElement != null && 
                    controlsPauseDocument.rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    HideControlsPauseMenu();
                    ShowPauseMenu();
                }
                else if (controlsDocument != null && controlsDocument.rootVisualElement != null && 
                    controlsDocument.rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    HideControlsMenu();
                    ShowPauseMenu();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    private void InitializePauseMenu()
    {
        if (pauseMenuDocument != null)
        {
            var pauseRoot = pauseMenuDocument.rootVisualElement;
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
            if (controlsDocument.rootVisualElement != null)
            {
                controlsDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
    
    private void InitializeControlsPauseMenu()
    {
        if (controlsPauseDocument != null)
        {
            if (controlsPauseDocument.rootVisualElement != null)
            {
                controlsPauseDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
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
        Time.timeScale = 1f;
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
        
        if (controlsPauseDocument != null && controlsPauseDocument.rootVisualElement != null)
        {
            controlsPauseDocument.rootVisualElement.style.display = DisplayStyle.None;
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
    
    private void HideControlsPauseMenu()
    {
        if (controlsPauseDocument != null && controlsPauseDocument.rootVisualElement != null)
        {
            controlsPauseDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void CleanupGame()
    {
        if (isPaused)
        {
            ResumeGame();
        }
    }
    
    private void OnPauseControlsClicked()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        if (controlsPauseDocument != null)
        {
            controlsPauseDocument.enabled = true;
            
            if (controlsPauseDocument.rootVisualElement != null)
            {
                controlsPauseDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void OnPauseMainMenuClicked()
    {
        ResumeGame();
        HideGameUI();
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        
        CleanupGame();
    }
    
    private void OnPauseExitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void StartGame()
    {
        if (gameUIContainer != null)
        {
            gameUIContainer.style.display = DisplayStyle.Flex;
        }
        
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
        HideGameUI();
        
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        
        CleanupGame();
    }
    
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
