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
            
            // Initialize settings UI for pause controls
            InitializePauseControlsSettings();
        }
    }
    
    private void InitializePauseControlsSettings()
    {
        if (controlsPauseDocument == null) return;
        
        var rootVisualElement = controlsPauseDocument.rootVisualElement;
        if (rootVisualElement == null) return;
        
        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null) return;
        
        Slider sensitivitySlider = rootVisualElement.Q<Slider>("SensitivitySlider");
        Slider volumeSlider = rootVisualElement.Q<Slider>("VolumeSlider");
        Label sensitivityValueLabel = rootVisualElement.Q<Label>("SensitivityValueLabel");
        Label volumeValueLabel = rootVisualElement.Q<Label>("VolumeValueLabel");
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = settingsManager.GetMouseSensitivity();
            sensitivitySlider.RegisterValueChangedCallback(evt => {
                settingsManager.SetMouseSensitivity(evt.newValue);
            });
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.value = settingsManager.GetMasterVolume();
            volumeSlider.RegisterValueChangedCallback(evt => {
                settingsManager.SetMasterVolume(evt.newValue);
            });
        }
        
        // Update labels when settings change
        settingsManager.OnSensitivityChanged += (value) => {
            if (sensitivityValueLabel != null)
            {
                sensitivityValueLabel.text = value.ToString("F2");
            }
        };
        
        settingsManager.OnVolumeChanged += (value) => {
            if (volumeValueLabel != null)
            {
                volumeValueLabel.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
            }
        };
        
        // Initialize labels
        if (sensitivityValueLabel != null)
        {
            sensitivityValueLabel.text = settingsManager.GetMouseSensitivity().ToString("F2");
        }
        
        if (volumeValueLabel != null)
        {
            volumeValueLabel.text = Mathf.RoundToInt(settingsManager.GetMasterVolume() * 100f).ToString() + "%";
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
                
                // Refresh settings values when opening
                RefreshPauseControlsSettings();
            }
        }
        
        if (pauseMenuDocument != null && pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
    
    private void RefreshPauseControlsSettings()
    {
        if (controlsPauseDocument == null) return;
        
        var rootVisualElement = controlsPauseDocument.rootVisualElement;
        if (rootVisualElement == null) return;
        
        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null) return;
        
        Slider sensitivitySlider = rootVisualElement.Q<Slider>("SensitivitySlider");
        Slider volumeSlider = rootVisualElement.Q<Slider>("VolumeSlider");
        Label sensitivityValueLabel = rootVisualElement.Q<Label>("SensitivityValueLabel");
        Label volumeValueLabel = rootVisualElement.Q<Label>("VolumeValueLabel");
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = settingsManager.GetMouseSensitivity();
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.value = settingsManager.GetMasterVolume();
        }
        
        if (sensitivityValueLabel != null)
        {
            sensitivityValueLabel.text = settingsManager.GetMouseSensitivity().ToString("F2");
        }
        
        if (volumeValueLabel != null)
        {
            volumeValueLabel.text = Mathf.RoundToInt(settingsManager.GetMasterVolume() * 100f).ToString() + "%";
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
