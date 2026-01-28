using UnityEngine;
using UnityEngine.UIElements;

/************************************
 * Manages game settings including mouse sensitivity and master volume.
 * Persists settings using PlayerPrefs and applies them to game systems.
 * Gleb 01/09/26
 * Version 1.0
 ************************************/
public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    
    [Header("Settings")]
    [SerializeField] private float defaultMouseSensitivity = 1.2f;
    [SerializeField] private float defaultMasterVolume = 1.0f;
    [SerializeField] private float defaultUIScale = 1.0f;
    
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string UI_SCALE_KEY = "UIScale";
    
    private float mouseSensitivity;
    private float masterVolume;
    private float uiScale;
    
    // Events for UI updates
    public System.Action<float> OnSensitivityChanged;
    public System.Action<float> OnVolumeChanged;
    public System.Action<float> OnUIScaleChanged;
    
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SettingsManager>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadSettings();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        ApplySettings();
    }
    
    public void LoadSettings()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, defaultMouseSensitivity);
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
        uiScale = PlayerPrefs.GetFloat(UI_SCALE_KEY, defaultUIScale);
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, mouseSensitivity);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(UI_SCALE_KEY, uiScale);
        PlayerPrefs.Save();
    }
    
    public void ApplySettings()
    {
        // Apply mouse sensitivity to PlayerMovement
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetMouseSensitivity(mouseSensitivity);
        }
        
        // Apply master volume to AudioListener
        AudioListener.volume = masterVolume;

        ApplyUIScale();
    }
    
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5.0f);
        SaveSettings();
        ApplySettings();
        OnSensitivityChanged?.Invoke(mouseSensitivity);
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        SaveSettings();
        ApplySettings();
        OnVolumeChanged?.Invoke(masterVolume);
    }

    public void SetUIScale(float scale)
    {
        uiScale = Mathf.Clamp(scale, 0.5f, 2.0f);
        SaveSettings();
        ApplySettings();
        OnUIScaleChanged?.Invoke(uiScale);
    }
    
    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetUIScale()
    {
        return uiScale;
    }

    private void ApplyUIScale()
    {
        var panelSettingsAssets = Resources.FindObjectsOfTypeAll<PanelSettings>();
        foreach (var panelSettings in panelSettingsAssets)
        {
            if (panelSettings == null)
            {
                continue;
            }
            panelSettings.scale = uiScale;
        }

        var documents = FindObjectsOfType<UIDocument>(true);
        foreach (var document in documents)
        {
            if (document == null || document.panelSettings == null)
            {
                continue;
            }
            document.panelSettings.scale = uiScale;
        }
    }
}

