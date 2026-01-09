using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

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
    [SerializeField] private InputActionAsset inputActions;
    private Dictionary<string, Button> pauseKeybindButtons = new Dictionary<string, Button>();
    private VisualElement pauseRebindPrompt;
    private InputActionRebindingExtensions.RebindingOperation pauseRebindOperation;
    
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
        
        // Find input actions if not assigned
        if (inputActions == null)
        {
            inputActions = Resources.FindObjectsOfTypeAll<InputActionAsset>().FirstOrDefault();
        }
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
        
        // Initialize keybinds for pause menu
        InitializePauseKeybinds(rootVisualElement);
    }
    
    private void InitializePauseKeybinds(VisualElement root)
    {
        if (inputActions == null) return;
        
        var keybindMap = new Dictionary<string, (string actionName, string partName)>
        {
            { "Keybind_MoveForward", ("Move", "up") },
            { "Keybind_MoveBackward", ("Move", "down") },
            { "Keybind_MoveLeft", ("Move", "left") },
            { "Keybind_MoveRight", ("Move", "right") },
            { "Keybind_CastSpell", ("Attack", null) },
            { "Keybind_NextSpell", ("Next", null) },
            { "Keybind_PreviousSpell", ("Previous", null) }
        };
        
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) return;
        
        foreach (var kvp in keybindMap)
        {
            Button button = root.Q<Button>(kvp.Key);
            if (button != null)
            {
                pauseKeybindButtons[kvp.Key] = button;
                string keyName = GetCurrentKeyName(playerMap, kvp.Value.actionName, kvp.Value.partName);
                button.text = keyName;
                
                string actionName = kvp.Value.actionName;
                string partName = kvp.Value.partName;
                button.clicked += () => StartPauseRebinding(kvp.Key, actionName, partName, button);
            }
        }
        
        pauseRebindPrompt = root.Q<VisualElement>("RebindPrompt");
    }
    
    private string GetCurrentKeyName(InputActionMap playerMap, string actionName, string partName)
    {
        InputAction action = playerMap.FindAction(actionName);
        if (action == null) return "?";
        
        if (partName != null)
        {
            foreach (var binding in action.bindings)
            {
                if (binding.isPartOfComposite && binding.name == partName)
                {
                    // Use overridePath if available, otherwise use path
                    string bindingPath = !string.IsNullOrEmpty(binding.overridePath) ? binding.overridePath : binding.path;
                    return FormatKeyName(bindingPath);
                }
            }
        }
        else
        {
            foreach (var binding in action.bindings)
            {
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    // Use overridePath if available, otherwise use path
                    string bindingPath = !string.IsNullOrEmpty(binding.overridePath) ? binding.overridePath : binding.path;
                    return FormatKeyName(bindingPath);
                }
            }
        }
        
        return "?";
    }
    
    private string FormatKeyName(string path)
    {
        if (string.IsNullOrEmpty(path)) return "?";
        
        // Handle both <Keyboard>/key and /Keyboard/key formats
        if (path.Contains("<Keyboard>/") || path.Contains("/Keyboard/"))
        {
            string key = path.Replace("<Keyboard>/", "").Replace("/Keyboard/", "");
            if (string.IsNullOrEmpty(key)) return "?";
            
            // Remove leading slash if present
            if (key.StartsWith("/")) key = key.Substring(1);
            
            if (key.Length == 1) return key.ToUpper();
            
            switch (key.ToLower())
            {
                case "uparrow": return "↑";
                case "downarrow": return "↓";
                case "leftarrow": return "←";
                case "rightarrow": return "→";
                case "space": return "Space";
                case "enter": return "Enter";
                case "tab": return "Tab";
                case "shift": return "Shift";
                case "ctrl": return "Ctrl";
                case "alt": return "Alt";
                default: 
                    // Capitalize first letter
                    return char.ToUpper(key[0]) + key.Substring(1).ToLower();
            }
        }
        else if (path.Contains("<Mouse>/") || path.Contains("/Mouse/"))
        {
            string button = path.Replace("<Mouse>/", "").Replace("/Mouse/", "");
            if (string.IsNullOrEmpty(button)) return "?";
            
            // Remove leading slash if present
            if (button.StartsWith("/")) button = button.Substring(1);
            
            if (button == "leftButton") return "Left Mouse";
            if (button == "rightButton") return "Right Mouse";
            if (button == "middleButton") return "Middle Mouse";
            return "Mouse " + button;
        }
        
        // If path doesn't match expected formats, try to extract just the key name
        if (path.Contains("/"))
        {
            string[] parts = path.Split('/');
            if (parts.Length > 0)
            {
                string lastPart = parts[parts.Length - 1];
                if (lastPart.Length == 1) return lastPart.ToUpper();
                return lastPart;
            }
        }
        
        return path;
    }
    
    private void StartPauseRebinding(string buttonName, string actionName, string partName, Button button)
    {
        if (pauseRebindOperation != null) return;
        
        if (inputActions == null) return;
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) return;
        
        InputAction action = playerMap.FindAction(actionName);
        if (action == null) return;
        
        int bindingIndex = -1;
        if (partName != null)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isPartOfComposite && action.bindings[i].name == partName)
                {
                    bindingIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                {
                    bindingIndex = i;
                    break;
                }
            }
        }
        
        if (bindingIndex == -1) return;
        
        if (pauseRebindPrompt != null)
        {
            pauseRebindPrompt.style.display = DisplayStyle.Flex;
        }
        
        action.Disable();
        
        pauseRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                string newKey = FormatKeyName(operation.selectedControl.path);
                button.text = newKey;
                
                string key = $"Keybind_{buttonName}_{actionName}_{partName}";
                PlayerPrefs.SetString(key, operation.selectedControl.path);
                PlayerPrefs.Save();
                
                operation.Dispose();
                pauseRebindOperation = null;
                action.Enable();
                
                if (pauseRebindPrompt != null)
                {
                    pauseRebindPrompt.style.display = DisplayStyle.None;
                }
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                pauseRebindOperation = null;
                action.Enable();
                
                if (pauseRebindPrompt != null)
                {
                    pauseRebindPrompt.style.display = DisplayStyle.None;
                }
            });
        
        pauseRebindOperation.Start();
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
        
        if (pauseRebindOperation != null)
        {
            pauseRebindOperation.Cancel();
            pauseRebindOperation = null;
        }
    }
}
