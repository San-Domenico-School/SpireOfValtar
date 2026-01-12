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
    }
    
    void Update()
    {
        // Only handle ESC if game UI is visible (game is actually running)
        bool gameUIVisible = gameUIContainer != null && gameUIContainer.style.display == DisplayStyle.Flex;
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!gameUIVisible)
            {
                return;
            }
            
            // Check for ESC key press during rebinding to cancel it
            if (pauseRebindOperation != null)
            {
                pauseRebindOperation.Cancel();
                return;
            }
            
            if (isPaused)
            {
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
        }
        
        pauseRebindPrompt = root.Q<VisualElement>("RebindPrompt");
        
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
                    
                    // Also style all children of the track to ensure they're dark
                    var trackChildren = track.Children();
                    foreach (var trackChild in trackChildren)
                    {
                        if (trackChild.name != "unity-dragger")
                        {
                            trackChild.style.backgroundColor = darkBg;
                            trackChild.style.borderLeftWidth = 0;
                            trackChild.style.borderRightWidth = 0;
                            trackChild.style.borderTopWidth = 0;
                            trackChild.style.borderBottomWidth = 0;
                        }
                    }
                }
                
                // Also check for slider element which might be the track
                var slider = verticalScroller.Q<UnityEngine.UIElements.Slider>();
                if (slider != null)
                {
                    slider.style.backgroundColor = darkBg;
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
                
                // Get the thumb first so we can reference it
                var thumb = verticalScroller.Q("unity-dragger");
                
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
                
                // Style all descendants recursively to catch any nested gray elements
                var allDescendants = verticalScroller.Query<VisualElement>().ToList();
                foreach (var descendant in allDescendants)
                {
                    if (descendant.name != "unity-dragger" && descendant != thumb)
                    {
                        descendant.style.backgroundColor = darkBg;
                        descendant.style.borderLeftWidth = 0;
                        descendant.style.borderRightWidth = 0;
                        descendant.style.borderTopWidth = 0;
                        descendant.style.borderBottomWidth = 0;
                    }
                }
                
                // Style the scrollbar thumb (dragger) - clean orange, no border
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
    
    private string FormatMouseButtonName(string button)
    {
        if (string.IsNullOrEmpty(button)) return "?";
        
        // Remove leading slash if present
        if (button.StartsWith("/")) button = button.Substring(1);
        
        // Normalize to lowercase for comparison
        string buttonLower = button.ToLower();
        
        // Handle standard button names
        if (buttonLower == "leftbutton" || buttonLower == "button" || buttonLower == "button<0>" || buttonLower == "button0") 
            return "Left Mouse";
        if (buttonLower == "rightbutton" || buttonLower == "button<1>" || buttonLower == "button1") 
            return "Right Mouse";
        if (buttonLower == "middlebutton" || buttonLower == "button<2>" || buttonLower == "button2") 
            return "Middle Mouse";
        
        // Handle directional button names (buttonWest, button west, button<west>, etc.)
        // Check for west, east, north, south in any format (case-insensitive)
        if (buttonLower.Contains("west"))
            return "Mouse Button 4";
        if (buttonLower.Contains("east"))
            return "Mouse Button 5";
        if (buttonLower.Contains("north"))
            return "Mouse Button 6";
        if (buttonLower.Contains("south"))
            return "Mouse Button 7";
        
        // Try to extract button number if it's in format "button<X>" or "buttonX"
        if (buttonLower.StartsWith("button"))
        {
            // Remove "button" prefix and angle brackets
            string numberPart = buttonLower.Replace("button<", "").Replace(">", "").Replace("button", "");
            
            // Try to parse as number
            if (int.TryParse(numberPart, out int buttonNum))
            {
                switch (buttonNum)
                {
                    case 0: return "Left Mouse";
                    case 1: return "Right Mouse";
                    case 2: return "Middle Mouse";
                    default: return $"Mouse Button {buttonNum + 1}";
                }
            }
        }
        
        return "Mouse " + button;
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
            
            return FormatMouseButtonName(button);
        }
        
        // Check if it's a mouse button path without Mouse prefix (e.g., just "buttonWest")
        string pathLower = path.ToLower();
        if (pathLower.StartsWith("button") || pathLower.Contains("button"))
        {
            return FormatMouseButtonName(path);
        }
        
        // If path doesn't match expected formats, try to extract just the key name
        if (path.Contains("/"))
        {
            string[] parts = path.Split('/');
            if (parts.Length > 0)
            {
                string lastPart = parts[parts.Length - 1];
                // Check if last part is a mouse button
                string lastPartLower = lastPart.ToLower();
                if (lastPartLower.StartsWith("button") || lastPartLower.Contains("button"))
                {
                    return FormatMouseButtonName(lastPart);
                }
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
            // For composite actions (like Move), find the specific part
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
            // For non-composite actions, prefer Keyboard&Mouse bindings
            // First pass: look for Keyboard&Mouse group
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    // Check if this binding is for Keyboard&Mouse
                    if (binding.groups != null && binding.groups.Contains("Keyboard&Mouse"))
                    {
                        bindingIndex = i;
                        break;
                    }
                }
            }
            
            // Second pass: if no Keyboard&Mouse binding found, use first non-composite binding
            if (bindingIndex == -1)
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
        }
        
        if (bindingIndex == -1)
        {
            return;
        }
        
        if (pauseRebindPrompt != null)
        {
            pauseRebindPrompt.style.display = DisplayStyle.Flex;
        }
        
        // Ensure the action map is enabled for rebinding to work
        if (!playerMap.enabled)
        {
            playerMap.Enable();
        }
        
        // Disable the action for rebinding (but keep the map enabled)
        action.Disable();
        
        // Start rebinding operation
        pauseRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                // Apply the binding override to the action
                action.ApplyBindingOverride(bindingIndex, operation.selectedControl.path);
                
                // Update button text with formatted key name
                string newKey = FormatKeyName(operation.selectedControl.path);
                button.text = newKey;
                
                // Save binding to PlayerPrefs
                string key = $"Keybind_{buttonName}_{actionName}_{partName}";
                PlayerPrefs.SetString(key, operation.selectedControl.path);
                PlayerPrefs.Save();
                
                // Cleanup
                operation.Dispose();
                pauseRebindOperation = null;
                
                // Re-enable the action
                action.Enable();
                
                // Hide prompt
                if (pauseRebindPrompt != null)
                {
                    pauseRebindPrompt.style.display = DisplayStyle.None;
                }
            })
            .OnCancel(operation =>
            {
                // Cleanup on cancel
                operation.Dispose();
                pauseRebindOperation = null;
                
                // Re-enable the action
                action.Enable();
                
                // Hide prompt
                if (pauseRebindPrompt != null)
                {
                    pauseRebindPrompt.style.display = DisplayStyle.None;
                }
            });
        
        // Start the rebinding operation
        pauseRebindOperation.Start();
    }
    
    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }
        
        isPaused = true;
        Time.timeScale = 0f;
        UnlockCursor();
        
        if (pauseMenuDocument != null)
        {
            pauseMenuDocument.enabled = true;
            if (pauseMenuDocument.rootVisualElement != null)
            {
                pauseMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
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
