// v1 Gleb created
// Drives the UI Toolkit crosshair reticle that lives in Game_View.uxml.
// Replaces the old uGUI ReticleController – no RectTransform or Image needed.
// Attach this to the same GameObject that holds GameUIManager (or any persistent GO).

using UnityEngine;
using UnityEngine.UIElements;

public class UIReticleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument gameViewDocument;

    // The reticle container element from Game_View.uxml
    private VisualElement reticleElement;

    private void Start()
    {
        // Auto-find the Game_View UIDocument if not wired in Inspector
        if (gameViewDocument == null)
        {
            var docs = FindObjectsOfType<UIDocument>(true);
            foreach (var doc in docs)
            {
                if (doc != null && doc.visualTreeAsset != null &&
                    doc.visualTreeAsset.name.Equals("Game_View", System.StringComparison.OrdinalIgnoreCase))
                {
                    gameViewDocument = doc;
                    break;
                }
            }
        }

        FindReticle();
    }

    private void FindReticle()
    {
        if (gameViewDocument == null || gameViewDocument.rootVisualElement == null) return;
        reticleElement = gameViewDocument.rootVisualElement.Q("Reticle");
    }

    /// <summary>Show or hide the crosshair reticle.</summary>
    public void SetVisible(bool visible)
    {
        if (reticleElement == null) FindReticle();
        if (reticleElement == null) return;
        reticleElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>Whether the reticle is currently visible.</summary>
    public bool IsVisible =>
        reticleElement != null &&
        reticleElement.style.display != DisplayStyle.None;
}
