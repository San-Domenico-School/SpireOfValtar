using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RestartManager : MonoBehaviour
{
    private const string RestartMenuResourcePath = "RestartMenu";

    [Header("Restart UI (UI Toolkit)")]
    [SerializeField] private UIDocument restartUIDocument;
    [SerializeField] private string restartRootName = "GameOverRoot";
    [SerializeField] private string restartButtonName = "RestartButton";
    [SerializeField] private string exitButtonName = "ExitButton";
    [SerializeField] private PanelSettings panelSettingsOverride;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 2f;

    [Header("Hide Other UI")]
    [SerializeField] private UIDocument[] uiDocumentsToHide;
    [SerializeField] private GameUIManager gameUIManager;

    [Header("Future SFX")]
    [SerializeField] private AudioClip restartMenuSfx;
    [SerializeField] private AudioSource restartMenuSfxSource;

    private VisualElement restartRoot;
    private Button restartButton;
    private Button exitButton;
    private bool hasShownMenu = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        CacheRestartUI();
        HideRestartMenuInstant();
    }

    private void Start()
    {
        InitializeButtons();
    }

    public void ShowRestartMenu()
    {
        if (hasShownMenu)
        {
            return;
        }

        CacheRestartUI();
        if (restartRoot == null)
        {
            Debug.LogWarning("RestartManager: Restart UI root not found in UIDocument.");
            return;
        }

        hasShownMenu = true;
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        HideOtherUI();

        restartRoot.style.display = DisplayStyle.Flex;
        restartRoot.style.opacity = 0f;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInRestartMenu());
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void InitializeButtons()
    {
        CacheRestartUI();

        if (restartButton != null)
        {
            restartButton.clicked -= RestartLevel;
            restartButton.clicked += RestartLevel;
        }

        if (exitButton != null)
        {
            exitButton.clicked -= ExitGame;
            exitButton.clicked += ExitGame;
        }
    }

    private void HideRestartMenuInstant()
    {
        CacheRestartUI();
        if (restartRoot != null)
        {
            restartRoot.style.opacity = 0f;
            restartRoot.style.display = DisplayStyle.None;
        }
    }

    private void HideOtherUI()
    {
        if (gameUIManager == null)
        {
            gameUIManager = FindObjectOfType<GameUIManager>();
        }

        if (gameUIManager != null)
        {
            gameUIManager.HideGameUI();
        }

        if (uiDocumentsToHide != null)
        {
            foreach (var document in uiDocumentsToHide)
            {
                if (document != null && document.rootVisualElement != null)
                {
                    document.rootVisualElement.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private IEnumerator FadeInRestartMenu()
    {
        if (restartRoot == null || fadeDuration <= 0f)
        {
            if (restartRoot != null)
            {
                restartRoot.style.opacity = 1f;
            }
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            restartRoot.style.opacity = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        restartRoot.style.opacity = 1f;
    }

    private void CacheRestartUI()
    {
        if (restartUIDocument == null)
        {
            restartUIDocument = GetComponent<UIDocument>();
            if (restartUIDocument == null)
            {
                restartUIDocument = FindRestartUIDocumentByName("RestartMenu");
            }

            if (restartUIDocument == null)
            {
                restartUIDocument = CreateRuntimeUIDocumentFromResources();
            }
        }

        if (restartUIDocument == null || restartUIDocument.rootVisualElement == null)
        {
            return;
        }

        var root = restartUIDocument.rootVisualElement;
        restartRoot = root.Q<VisualElement>(restartRootName) ?? root;

        restartButton = restartRoot.Q<Button>(restartButtonName);
        exitButton = restartRoot.Q<Button>(exitButtonName);
    }

    private UIDocument FindRestartUIDocumentByName(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        var documents = FindObjectsOfType<UIDocument>(true);
        foreach (var document in documents)
        {
            if (document == null)
            {
                continue;
            }

            if (document.visualTreeAsset != null
                && document.visualTreeAsset.name.Equals(assetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }
        }

        return null;
    }

    private UIDocument CreateRuntimeUIDocumentFromResources()
    {
        var visualTree = Resources.Load<VisualTreeAsset>(RestartMenuResourcePath);
        if (visualTree == null)
        {
            Debug.LogWarning("RestartManager: RestartMenu.uxml not found in Resources.");
            return null;
        }

        PanelSettings panelSettings = panelSettingsOverride;
        if (panelSettings == null)
        {
            var anyDocument = FindObjectOfType<UIDocument>(true);
            if (anyDocument != null)
            {
                panelSettings = anyDocument.panelSettings;
            }
        }

        if (panelSettings == null)
        {
            var panelSettingsAssets = Resources.FindObjectsOfTypeAll<PanelSettings>();
            if (panelSettingsAssets.Length > 0)
            {
                panelSettings = panelSettingsAssets[0];
            }
        }

        if (panelSettings == null)
        {
            Debug.LogWarning("RestartManager: PanelSettings not found. Assign PanelSettings Override.");
            return null;
        }

        var restartObject = new GameObject("RestartUI");
        var document = restartObject.AddComponent<UIDocument>();
        document.visualTreeAsset = visualTree;
        document.panelSettings = panelSettings;
        return document;
    }
}
