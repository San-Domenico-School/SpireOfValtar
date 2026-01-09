using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

/************************************
 * Manages the controls menu UI display and navigation.
 * Handles showing/hiding controls menu and back button functionality.
 * Now includes settings management for mouse sensitivity and volume.
 * Gleb 11/4/25
 * Version 2.0
 ************************************/
public class ControlsManager : MonoBehaviour
{
    [SerializeField] private Texture2D controlsImage;
    
    private UIDocument controlledUIDocument;
    private VisualElement controlsContainer;
    private VisualElement controlsImageContainer;
    private Button backButton;
    private MainMenuManager mainMenuManager;
    
    // Settings UI elements
    private Slider sensitivitySlider;
    private Slider volumeSlider;
    private Label sensitivityValueLabel;
    private Label volumeValueLabel;
    private SettingsManager settingsManager;
    
    // Keybinding
    [SerializeField] private InputActionAsset inputActions;
    private Dictionary<string, Button> keybindButtons = new Dictionary<string, Button>();
    private VisualElement rebindPrompt;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    private string currentRebindingAction = null;
    
    void Start()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        settingsManager = SettingsManager.Instance;
        
        // Find input actions if not assigned
        if (inputActions == null)
        {
            inputActions = Resources.FindObjectsOfTypeAll<InputActionAsset>().FirstOrDefault();
        }
        
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
        
        // Try to find ControlsContainer - Unity might wrap the UXML root
        // Method 1: Direct query
        controlsContainer = rootVisualElement.Q<VisualElement>("ControlsContainer");
        
        // Method 2: Check if root IS the ControlsContainer (unlikely but possible)
        if (controlsContainer == null && rootVisualElement.name == "ControlsContainer")
        {
            controlsContainer = rootVisualElement;
        }
        
        // Method 3: Check direct children
        if (controlsContainer == null)
        {
            foreach (var child in rootVisualElement.Children())
            {
                if (child.name == "ControlsContainer")
                {
                    controlsContainer = child;
                    break;
                }
            }
        }
        
        // Method 4: Check nested in first child (Unity might wrap UXML root)
        if (controlsContainer == null && rootVisualElement.childCount > 0)
        {
            var firstChild = rootVisualElement.ElementAt(0);
            if (firstChild.name == "ControlsContainer")
            {
                controlsContainer = firstChild as VisualElement;
            }
            else
            {
                controlsContainer = firstChild.Q<VisualElement>("ControlsContainer");
            }
        }
        
        // Method 5: Search all descendants
        if (controlsContainer == null)
        {
            var allDescendants = rootVisualElement.Query<VisualElement>().ToList();
            foreach (var elem in allDescendants)
            {
                if (elem.name == "ControlsContainer")
                {
                    controlsContainer = elem;
                    Debug.Log("ControlsManager: Found ControlsContainer via full descendant search!");
                    break;
                }
            }
        }
        
        if (controlsContainer == null)
        {
            Debug.LogWarning("ControlsManager: ControlsContainer not found in InitializeFromUIDocument. Root: " + rootVisualElement.name + ", children: " + rootVisualElement.childCount);
        }
        
        var allVisualElements = rootVisualElement.Query<VisualElement>().ToList();
        controlsImageContainer = allVisualElements.FirstOrDefault(e => e.style.backgroundImage.value.texture != null);
        
        backButton = rootVisualElement.Q<Button>("Back");
        
        if (backButton != null)
        {
            backButton.clicked += OnBackButtonClicked;
        }
        
        // Initialize settings UI
        sensitivitySlider = rootVisualElement.Q<Slider>("SensitivitySlider");
        volumeSlider = rootVisualElement.Q<Slider>("VolumeSlider");
        sensitivityValueLabel = rootVisualElement.Q<Label>("SensitivityValueLabel");
        volumeValueLabel = rootVisualElement.Q<Label>("VolumeValueLabel");
        
        if (settingsManager != null)
        {
            // Load current settings
            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = settingsManager.GetMouseSensitivity();
                sensitivitySlider.RegisterValueChangedCallback(OnSensitivityChanged);
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.value = settingsManager.GetMasterVolume();
                volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
            }
            
            // Subscribe to settings changes to update labels
            settingsManager.OnSensitivityChanged += UpdateSensitivityLabel;
            settingsManager.OnVolumeChanged += UpdateVolumeLabel;
            
            // Initialize labels
            UpdateSensitivityLabel(settingsManager.GetMouseSensitivity());
            UpdateVolumeLabel(settingsManager.GetMasterVolume());
        }
        
        // Initialize keybinds
        InitializeKeybinds(rootVisualElement);
    }
    
    private void InitializeKeybinds(VisualElement root)
    {
        if (inputActions == null) return;
        
        // Map button names to action names and composite parts
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
                keybindButtons[kvp.Key] = button;
                
                // Get current key display
                string keyName = GetCurrentKeyName(playerMap, kvp.Value.actionName, kvp.Value.partName);
                button.text = keyName;
                
                // Add click handler
                string actionName = kvp.Value.actionName;
                string partName = kvp.Value.partName;
                button.clicked += () => StartRebinding(kvp.Key, actionName, partName, button);
            }
        }
        
        rebindPrompt = root.Q<VisualElement>("RebindPrompt");
    }
    
    private string GetCurrentKeyName(InputActionMap playerMap, string actionName, string partName)
    {
        InputAction action = playerMap.FindAction(actionName);
        if (action == null) return "?";
        
        if (partName != null)
        {
            // For composite actions (Move)
            foreach (var binding in action.bindings)
            {
                if (binding.isPartOfComposite && binding.name == partName)
                {
                    return FormatKeyName(binding.path);
                }
            }
        }
        else
        {
            // For regular actions
            foreach (var binding in action.bindings)
            {
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    return FormatKeyName(binding.path);
                }
            }
        }
        
        return "?";
    }
    
    private string FormatKeyName(string path)
    {
        if (path.Contains("<Keyboard>/"))
        {
            string key = path.Replace("<Keyboard>/", "");
            if (key.Length == 1) return key.ToUpper();
            
            switch (key.ToLower())
            {
                case "uparrow": return "↑";
                case "downarrow": return "↓";
                case "leftarrow": return "←";
                case "rightarrow": return "→";
                case "space": return "Space";
                default: return char.ToUpper(key[0]) + key.Substring(1);
            }
        }
        else if (path.Contains("<Mouse>/"))
        {
            string button = path.Replace("<Mouse>/", "");
            if (button == "leftButton") return "Left Mouse";
            if (button == "rightButton") return "Right Mouse";
            return "Mouse " + button;
        }
        
        return path;
    }
    
    private void StartRebinding(string buttonName, string actionName, string partName, Button button)
    {
        if (rebindOperation != null) return; // Already rebinding
        
        if (inputActions == null) return;
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) return;
        
        InputAction action = playerMap.FindAction(actionName);
        if (action == null) return;
        
        // Find binding index
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
        
        // Show prompt
        if (rebindPrompt != null)
        {
            rebindPrompt.style.display = DisplayStyle.Flex;
        }
        
        action.Disable();
        
        // Start rebinding
        rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                string newKey = FormatKeyName(operation.selectedControl.path);
                button.text = newKey;
                
                // Save binding
                string key = $"Keybind_{buttonName}_{actionName}_{partName}";
                PlayerPrefs.SetString(key, operation.selectedControl.path);
                PlayerPrefs.Save();
                
                operation.Dispose();
                rebindOperation = null;
                action.Enable();
                
                if (rebindPrompt != null)
                {
                    rebindPrompt.style.display = DisplayStyle.None;
                }
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                rebindOperation = null;
                action.Enable();
                
                if (rebindPrompt != null)
                {
                    rebindPrompt.style.display = DisplayStyle.None;
                }
            });
        
        rebindOperation.Start();
    }
    
    public void ShowControls()
    {
        // If no document is set, we can't show controls
        if (controlledUIDocument == null)
        {
            Debug.LogWarning("ControlsManager: No UIDocument set! Make sure InitializeFromUIDocument is called first.");
            return;
        }
        
        // Ensure document is enabled
        controlledUIDocument.enabled = true;
        
        // Wait a frame to ensure rootVisualElement is created
        if (controlledUIDocument.rootVisualElement == null)
        {
            Debug.LogWarning("ControlsManager: rootVisualElement is null! UIDocument may not be ready.");
            return;
        }
        
        // Ensure root is visible - this is critical
        controlledUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        
        // Re-initialize if needed (in case elements weren't found before)
        if (controlsContainer == null || keybindButtons.Count == 0)
        {
            InitializeFromUIDocument(controlledUIDocument);
        }
        
        // Double-check controlsContainer - try multiple search methods
        if (controlsContainer == null)
        {
            var root = controlledUIDocument.rootVisualElement;
            
            // Method 1: Direct query
            controlsContainer = root.Q<VisualElement>("ControlsContainer");
            
            // Method 2: Check direct children
            if (controlsContainer == null)
            {
                foreach (var child in root.Children())
                {
                    if (child.name == "ControlsContainer")
                    {
                        controlsContainer = child;
                        break;
                    }
                }
            }
            
            // Method 3: Search all descendants
            if (controlsContainer == null)
            {
                var allDescendants = root.Query<VisualElement>().ToList();
                foreach (var elem in allDescendants)
                {
                    if (elem.name == "ControlsContainer")
                    {
                        controlsContainer = elem;
                        Debug.Log("ControlsManager: Found ControlsContainer via full search!");
                        break;
                    }
                }
            }
        }
        
        // Show the container
        if (controlsContainer != null)
        {
            controlsContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogError("ControlsManager: ControlsContainer not found! Root name: " + controlledUIDocument.rootVisualElement.name + ", children: " + controlledUIDocument.rootVisualElement.childCount);
            // List all children for debugging
            foreach (var child in controlledUIDocument.rootVisualElement.Children())
            {
                Debug.Log("ControlsManager: Found child: " + child.name);
            }
        }
        
        // Ensure root is still visible (in case something hid it)
        controlledUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        
        if (controlsImage != null && controlsImageContainer != null)
        {
            controlsImageContainer.style.backgroundImage = new StyleBackground(controlsImage);
        }
        
        // Refresh keybind displays
        RefreshKeybindDisplays();
        
        Debug.Log("ControlsManager: ShowControls completed. Root display: " + controlledUIDocument.rootVisualElement.style.display);
    }
    
    private void RefreshKeybindDisplays()
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
            { "Keybind_CastSpell", ("Attack", null) },
            { "Keybind_NextSpell", ("Next", null) },
            { "Keybind_PreviousSpell", ("Previous", null) }
        };
        
        foreach (var kvp in keybindMap)
        {
            if (keybindButtons.ContainsKey(kvp.Key))
            {
                string keyName = GetCurrentKeyName(playerMap, kvp.Value.actionName, kvp.Value.partName);
                keybindButtons[kvp.Key].text = keyName;
            }
        }
    }
    
    public void HideControls()
    {
        if (controlsContainer != null)
        {
            controlsContainer.style.display = DisplayStyle.None;
        }
        
        // Don't hide the root - let MainMenuManager handle that
        // This prevents accidental hiding when we want to show controls
    }
    
    private void OnBackButtonClicked()
    {
        HideControls();
        
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
    
    private void OnSensitivityChanged(ChangeEvent<float> evt)
    {
        if (settingsManager != null)
        {
            settingsManager.SetMouseSensitivity(evt.newValue);
        }
    }
    
    private void OnVolumeChanged(ChangeEvent<float> evt)
    {
        if (settingsManager != null)
        {
            settingsManager.SetMasterVolume(evt.newValue);
        }
    }
    
    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityValueLabel != null)
        {
            sensitivityValueLabel.text = value.ToString("F2");
        }
    }
    
    private void UpdateVolumeLabel(float value)
    {
        if (volumeValueLabel != null)
        {
            volumeValueLabel.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
        }
    }
    
    private void OnDestroy()
    {
        if (settingsManager != null)
        {
            settingsManager.OnSensitivityChanged -= UpdateSensitivityLabel;
            settingsManager.OnVolumeChanged -= UpdateVolumeLabel;
        }
        
        // Cancel any ongoing rebinding
        if (rebindOperation != null)
        {
            rebindOperation.Cancel();
            rebindOperation = null;
        }
    }
}
