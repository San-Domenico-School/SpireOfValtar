using UnityEngine;
using UnityEngine.UIElements;
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
        HideMainMenu();
        
        if (controlsUIDocument == null)
        {
            return;
        }
        
        if (controlsUIDocument.rootVisualElement != null)
        {
            controlsUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        
        if (controlsManager == null)
        {
            return;
        }
        
        controlsManager.ShowControls();
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
