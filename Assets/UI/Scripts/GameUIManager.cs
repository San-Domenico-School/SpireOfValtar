using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

/************************************
 * Handles game UI, pause menu, and ESC key controls.
 * Manages pause/unpause, controls menu navigation, and menu button functionality.
 * Gleb 01/09/26
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
    
    private Button pauseResumeButton;
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
        
        // Use root element as gameUIContainer since Game_View.uxml doesn't have a GameUIContainer element
        gameUIContainer = rootVisualElement;
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
        
        Debug.Log($"GameUIManager: Start complete. pauseMenuDocument: {(pauseMenuDocument != null ? "assigned" : "NULL - ASSIGN IN INSPECTOR!")}, gameUIContainer: {(gameUIContainer != null ? "found" : "not found")}, uiDocument: {(uiDocument != null ? "exists" : "null")}");
    }
    
    void Update()
    {
        // Only handle ESC if game UI is visible (game is actually running)
        bool gameUIVisible = gameUIContainer != null && gameUIContainer.style.display == DisplayStyle.Flex;
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log($"GameUIManager: ESC pressed. gameUIVisible: {gameUIVisible}, isPaused: {isPaused}, gameUIContainer: {(gameUIContainer != null ? "exists" : "null")}");
            
            if (!gameUIVisible)
            {
                Debug.Log("GameUIManager: Game UI not visible, ignoring ESC");
                return;
            }
            
            // Check for ESC key press during rebinding to cancel it
            if (pauseRebindOperation != null)
            {
                Debug.Log("GameUIManager: Canceling rebind operation");
                pauseRebindOperation.Cancel();
                return;
            }
            
            if (isPaused)
            {
                Debug.Log("GameUIManager: Game is paused, handling ESC for navigation between menus");
                // ESC can navigate between pause menu and settings, but cannot close pause menu
                if (controlsPauseDocument != null && controlsPauseDocument.rootVisualElement != null && 
                    controlsPauseDocument.rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    // Close settings and return to pause menu
                    HideControlsPauseMenu();
                    ShowPauseMenu();
                }
                else if (controlsDocument != null && controlsDocument.rootVisualElement != null && 
                    controlsDocument.rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    // Close controls and return to pause menu
                    HideControlsMenu();
                    ShowPauseMenu();
                }
                // If pause menu is open, ESC does nothing - only Resume button can close it
            }
            else
            {
                // Pause the game - ESC can only open pause menu
                Debug.Log("GameUIManager: Pausing game");
                PauseGame();
            }
        }
    }
    
    private void InitializePauseMenu()
    {
        if (pauseMenuDocument != null)
        {
            pauseMenuDocument.enabled = true;
            var pauseRoot = pauseMenuDocument.rootVisualElement;
            
            if (pauseRoot == null)
            {
                Debug.LogWarning("GameUIManager: pauseMenuDocument.rootVisualElement is null during initialization!");
                return;
            }
            
            pauseResumeButton = pauseRoot.Q<Button>("ResumeButton");
            pauseControlsButton = pauseRoot.Q<Button>("ControlsButton");
            pauseMainMenuButton = pauseRoot.Q<Button>("MainMenuButton");
            pauseExitButton = pauseRoot.Q<Button>("ExitButton");
            
            if (pauseResumeButton != null)
            {
                pauseResumeButton.clicked += OnPauseResumeClicked;
            }
            
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
            
            // Hide pause menu initially
            pauseRoot.style.display = DisplayStyle.None;
        }
        else
            {
            Debug.LogWarning("GameUIManager: pauseMenuDocument is null!");
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
        if (inputActions == null)
        {
            Debug.LogWarning("GameUIManager: inputActions is null, cannot initialize pause keybinds");
            return;
        }
        
        var keybindMap = new Dictionary<string, (string actionName, string partName)>
        {
            { "Keybind_MoveForward", ("Move", "up") },
            { "Keybind_MoveBackward", ("Move", "down") },
            { "Keybind_MoveLeft", ("Move", "left") },
            { "Keybind_MoveRight", ("Move", "right") },
            { "Keybind_Jump", ("Jump", null) },
            { "Keybind_CastSpell", ("Attack", null) },
            { "Keybind_NextSpell", ("Next", null) },
            { "Keybind_PreviousSpell", ("Previous", null) }
        };
        
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogWarning("GameUIManager: Player action map not found");
            return;
        }
        
        // Only initialize if buttons haven't been set up yet, or re-initialize if needed
        bool needsInitialization = pauseKeybindButtons.Count == 0;
        
        foreach (var kvp in keybindMap)
        {
            Button button = root.Q<Button>(kvp.Key);
            if (button != null)
            {
                if (needsInitialization || !pauseKeybindButtons.ContainsKey(kvp.Key))
                {
                    pauseKeybindButtons[kvp.Key] = button;
                    
                    string actionName = kvp.Value.actionName;
                    string partName = kvp.Value.partName;
                    string buttonName = kvp.Key; // Capture for closure
                    
                    // Add click handler
                    button.clicked += () => StartPauseRebinding(buttonName, actionName, partName, button);
                }
                
                // Always update the display text
                string keyName = GetCurrentKeyName(playerMap, kvp.Value.actionName, kvp.Value.partName);
                button.text = keyName;
            }
            else
            {
                Debug.LogWarning($"GameUIManager: Could not find button {kvp.Key} in pause controls menu");
            }
        }
        
        pauseRebindPrompt = root.Q<VisualElement>("RebindPrompt");
        if (pauseRebindPrompt == null)
        {
            Debug.LogWarning("GameUIManager: Could not find RebindPrompt in pause controls menu");
        }
        
        // Style the scrollbar to match the design
        StylePauseScrollbar(root);
    }
    
    private void RefreshPauseKeybindDisplay()
    {
        if (inputActions == null) return;
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) return;
        
        var keybindMap = new Dictionary<string, (string actionName, string partName)>
        {
            { "Keybind_MoveForward", ("Move", "up") },
            { "Keybind_MoveBackward", ("Move", "down") },
            { "Keybind_MoveLeft", ("Move", "left") },
            { "Keybind_MoveRight", ("Move", "right") },
            { "Keybind_Jump", ("Jump", null) },
            { "Keybind_CastSpell", ("Attack", null) },
            { "Keybind_NextSpell", ("Next", null) },
            { "Keybind_PreviousSpell", ("Previous", null) }
        };
        
        foreach (var kvp in keybindMap)
        {
            if (pauseKeybindButtons.ContainsKey(kvp.Key) && pauseKeybindButtons[kvp.Key] != null)
            {
                string keyName = GetCurrentKeyName(playerMap, kvp.Value.actionName, kvp.Value.partName);
                pauseKeybindButtons[kvp.Key].text = keyName;
            }
        }
    }
    
    private void StylePauseScrollbar(VisualElement root)
    {
        ScrollView scrollView = root.Q<ScrollView>("ContentScrollView");
        if (scrollView != null)
        {
            // Use ScrollView's verticalScroller and horizontalScroller properties
            var verticalScroller = scrollView.verticalScroller;
            if (verticalScroller != null)
            {
                // Match UI background color exactly - no borders
                Color darkBg = new Color(20f / 255f, 20f / 255f, 20f / 255f, 1f);
                verticalScroller.style.backgroundColor = darkBg;
                verticalScroller.style.borderLeftWidth = 0;
                verticalScroller.style.borderRightWidth = 0;
                verticalScroller.style.borderTopWidth = 0;
                verticalScroller.style.borderBottomWidth = 0;
                
                // Style the scrollbar track (drag container) - match background, no borders
                var track = verticalScroller.Q("unity-drag-container");
                if (track != null)
                {
                    track.style.backgroundColor = darkBg;
                    track.style.borderLeftWidth = 0;
                    track.style.borderRightWidth = 0;
                    track.style.borderTopWidth = 0;
                    track.style.borderBottomWidth = 0;
                }
                
                // Style scrollbar buttons (up/down arrows) - match background, no borders
                var upButton = verticalScroller.Q("unity-up-button");
                if (upButton != null)
                {
                    upButton.style.backgroundColor = darkBg;
                    upButton.style.borderLeftWidth = 0;
                    upButton.style.borderRightWidth = 0;
                    upButton.style.borderTopWidth = 0;
                    upButton.style.borderBottomWidth = 0;
                }
                
                var downButton = verticalScroller.Q("unity-down-button");
                if (downButton != null)
                {
                    downButton.style.backgroundColor = darkBg;
                    downButton.style.borderLeftWidth = 0;
                    downButton.style.borderRightWidth = 0;
                    downButton.style.borderTopWidth = 0;
                    downButton.style.borderBottomWidth = 0;
                }
                
                // Style all children to ensure no white backgrounds and no borders
                var allChildren = verticalScroller.Children();
                foreach (var child in allChildren)
                {
                    if (child.name != "unity-dragger") // Don't override thumb color
                    {
                        child.style.backgroundColor = darkBg;
                        child.style.borderLeftWidth = 0;
                        child.style.borderRightWidth = 0;
                        child.style.borderTopWidth = 0;
                        child.style.borderBottomWidth = 0;
                    }
                }
                
                // Style the scrollbar thumb (dragger) - clean orange, no border
                var thumb = verticalScroller.Q("unity-dragger");
                if (thumb != null)
                {
                    thumb.style.backgroundColor = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
                    thumb.style.borderLeftWidth = 0;
                    thumb.style.borderRightWidth = 0;
                    thumb.style.borderTopWidth = 0;
                    thumb.style.borderBottomWidth = 0;
                    thumb.style.borderTopLeftRadius = 3;
                    thumb.style.borderTopRightRadius = 3;
                    thumb.style.borderBottomLeftRadius = 3;
                    thumb.style.borderBottomRightRadius = 3;
                }
            }
            
            // Hide horizontal scrollbar
            var horizontalScroller = scrollView.horizontalScroller;
            if (horizontalScroller != null)
            {
                horizontalScroller.style.display = DisplayStyle.None;
            }
        }
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
        if (isPaused)
        {
            Debug.Log("GameUIManager: Already paused, skipping");
            return;
        }
        
        Debug.Log($"GameUIManager: PauseGame called. pauseMenuDocument: {(pauseMenuDocument != null ? "exists" : "NULL")}");
        
        isPaused = true;
        Time.timeScale = 0f;
        UnlockCursor();
        
        if (pauseMenuDocument != null)
        {
            pauseMenuDocument.enabled = true;
            if (pauseMenuDocument.rootVisualElement != null)
        {
            pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                Debug.Log("GameUIManager: Pause menu should now be visible");
            }
            else
            {
                Debug.LogError("GameUIManager: pauseMenuDocument.rootVisualElement is null! Document enabled: " + pauseMenuDocument.enabled);
            }
        }
        else
        {
            Debug.LogError("GameUIManager: pauseMenuDocument is null! Make sure it's assigned in the inspector!");
        }
    }
    
    // Public method to lock cursor - can be called from UI buttons
    public void LockCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
    
    // Public method to unlock cursor - can be called from UI buttons
    public void UnlockCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        // Hide all pause-related UI first
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
        
        // Lock cursor using the button-triggered method
        LockCursor();
    }
    
    // Called when Resume button is clicked - this ensures cursor locks from button click
    private void OnPauseResumeClicked()
    {
        ResumeGame();
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
        UnlockCursor();
        
        if (controlsPauseDocument != null)
        {
            controlsPauseDocument.enabled = true;
            
            if (controlsPauseDocument.rootVisualElement != null)
            {
                controlsPauseDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                
                // Initialize keybinds if not already done, or re-initialize to ensure they work
                InitializePauseKeybinds(controlsPauseDocument.rootVisualElement);
                
                // Refresh settings values when opening
                RefreshPauseControlsSettings();
                
                // Refresh keybind displays
                RefreshPauseKeybindDisplay();
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
