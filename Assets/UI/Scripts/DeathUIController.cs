using UnityEngine;
using UnityEngine.UIElements;
/***************************************************
 * DeathUIController.cs
 * 
 * This script is responsible for the death screen UI.
 * It is used to display the death screen UI when the player dies.
 * It is also used to handle the restart and exit buttons.
 * Gleb
 * 01.27.2026
 ***************************************************/

public class DeathUIController : MonoBehaviour
{
    private const string DeathUxmlName = "DeathScreen";

    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Fade Settings")]
    [SerializeField] private float fadeToBlackDuration = 1.5f;
    [SerializeField] private float uiFadeDelay = 0.3f;
    [SerializeField] private float uiFadeDuration = 0.6f;
    [SerializeField] private float maxBackdropOpacity = 0.9f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private int sortingOrder = 30;

    private VisualElement root;
    private VisualElement backdrop;
    private VisualElement panel;
    private Button restartButton;
    private Button exitButton;

    private bool isAnimating;
    private float elapsed;

    void Awake()
    {
        ResolveUIDocument();

        if (uiDocument != null)
        {
            uiDocument.sortingOrder = sortingOrder;
        }

        BindUI();
        HideImmediate();
        Debug.Log("[DeathUI] Awake -> UI disabled");
    }

    void OnEnable()
    {
        BindUI();
    }

    void Update()
    {
        if (!isAnimating) return;

        elapsed += Time.unscaledDeltaTime;

        float backdropDuration = Mathf.Max(0.0001f, fadeToBlackDuration);
        float backdropT = Mathf.Clamp01(elapsed / backdropDuration);
        float backdropAlpha = fadeCurve.Evaluate(backdropT) * maxBackdropOpacity;
        if (backdrop != null)
        {
            backdrop.style.opacity = backdropAlpha;
        }

        float uiStart = uiFadeDelay;
        float uiDuration = Mathf.Max(0.0001f, uiFadeDuration);
        float uiElapsed = Mathf.Clamp(elapsed - uiStart, 0f, uiDuration);
        float uiT = Mathf.Clamp01(uiElapsed / uiDuration);
        float uiAlpha = fadeCurve.Evaluate(uiT);
        if (panel != null)
        {
            panel.style.opacity = uiAlpha;
        }

        if (backdropT >= 1f && uiT >= 1f)
        {
            isAnimating = false;
            if (root != null)
            {
                root.pickingMode = PickingMode.Position;
            }
            SetButtonsEnabled(true);
        }
    }

    public void PlayDeathSequence()
    {
        ShowDeathUI();
    }

    private void ShowDeathUI()
    {
        Debug.Log(
            "[DeathUI] SHOW CALLED\n" +
            UnityEngine.StackTraceUtility.ExtractStackTrace()
        );

        // #region agent log
        RuntimeDebugLogger.Log(
            "DeathUIController.cs:ShowDeathUI",
            "ShowDeathUI called",
            "H1",
            "{\"stack\":\"" + RuntimeDebugLogger.Escape(UnityEngine.StackTraceUtility.ExtractStackTrace()) + "\"}"
        );
        // #endregion

        Debug.Log("[DeathUI] Showing Death UI");

        ResolveUIDocument();

        if (uiDocument == null)
        {
            return;
        }

        uiDocument.enabled = true;
        BindUI();

        if (root == null)
        {
            return;
        }

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Ignore;
        elapsed = 0f;
        isAnimating = true;

        ResetVisualState();
        SetButtonsEnabled(false);
    }

    public void HideImmediate()
    {
        Debug.Log("[DeathUI] Hiding Death UI");

        ResolveUIDocument();

        if (uiDocument != null)
        {
            uiDocument.enabled = false;
        }

        if (root == null && uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
        }

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            root.pickingMode = PickingMode.Ignore;
        }

        if (backdrop != null) backdrop.style.opacity = 0f;
        if (panel != null) panel.style.opacity = 0f;
        SetButtonsEnabled(false);
        isAnimating = false;
        elapsed = 0f;
    }

    private void BindUI()
    {
        ResolveUIDocument();

        if (uiDocument == null)
        {
            Debug.LogWarning("[DeathUI] UIDocument not found.");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("[DeathUI] Root VisualElement not found.");
            return;
        }

        backdrop = root.Q<VisualElement>("DeathBackdrop");
        panel = root.Q<VisualElement>("DeathPanel");
        restartButton = root.Q<Button>("RestartButton");
        exitButton = root.Q<Button>("ExitButton");

        if (restartButton != null)
        {
            restartButton.clicked -= OnRestartClicked;
            restartButton.clicked += OnRestartClicked;
        }

        if (exitButton != null)
        {
            exitButton.clicked -= OnExitClicked;
            exitButton.clicked += OnExitClicked;
        }
    }

    private void ResolveUIDocument()
    {
        if (uiDocument != null)
        {
            return;
        }

        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            return;
        }

        uiDocument = GetComponentInChildren<UIDocument>(true);
        if (uiDocument != null)
        {
            return;
        }

        var documents = Resources.FindObjectsOfTypeAll<UIDocument>();
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals(DeathUxmlName, System.StringComparison.OrdinalIgnoreCase))
            {
                uiDocument = document;
                return;
            }
        }
    }

    private void ResetVisualState()
    {
        if (backdrop != null) backdrop.style.opacity = 0f;
        if (panel != null) panel.style.opacity = 0f;
    }

    private void SetButtonsEnabled(bool enabled)
    {
        if (restartButton != null)
        {
            restartButton.SetEnabled(enabled);
        }

        if (exitButton != null)
        {
            exitButton.SetEnabled(enabled);
        }
    }

    private void OnRestartClicked()
    {
        if (RestartManager.Instance != null)
        {
            RestartManager.Instance.RequestRestartFromDeath();
        }
    }

    private void OnExitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
