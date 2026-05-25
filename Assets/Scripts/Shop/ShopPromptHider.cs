// v1 Gleb created
// Attach to the ShopPromptUI GameObject alongside its UIDocument.
// Hides the "Press B to open shop" label on every scene load so it never
// bleeds into scenes that have no shop.

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ShopPromptHider : MonoBehaviour
{
    private UIDocument doc;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hide the prompt immediately whenever any scene loads
        HidePrompt();
    }

    private void HidePrompt()
    {
        if (doc == null || doc.rootVisualElement == null) return;
        var label = doc.rootVisualElement.Q<Label>("ShopPromptLabel");
        if (label != null)
            label.style.display = DisplayStyle.None;
    }
}
