using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

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
    
    void Start()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        settingsManager = SettingsManager.Instance;
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
    }
}
