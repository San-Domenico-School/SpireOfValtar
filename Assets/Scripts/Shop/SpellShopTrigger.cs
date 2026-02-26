// v1 Gleb created
// Attach to the shop cube GameObject.
// When the player enters the interaction radius a "Press B to open shop" prompt appears on the HUD.
// Pressing B opens the shop (game freezes); pressing B again closes it.
// Uses a dedicated ShopPrompt UIDocument so the label renders correctly at screen position.

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpellShopTrigger : MonoBehaviour
{
    [Header("Proximity")]
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private Transform playerTransform;

    [Header("Shop")]
    [SerializeField] private SpellShopUI spellShopUI;

    [Header("Prompt UI (ShopPrompt UIDocument)")]
    [SerializeField] private UIDocument shopPromptDocument;

    [Header("Ground Circle")]
    [SerializeField] private ShopRadiusIndicator radiusIndicator;

    private bool playerInRange = false;
    private Label promptLabel;

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) playerTransform = go.transform;
        }

        // Auto-find SpellShopUI if not assigned
        if (spellShopUI == null)
            spellShopUI = FindFirstObjectByType<SpellShopUI>();

        // Auto-find radius indicator on this GameObject if not assigned
        if (radiusIndicator == null)
            radiusIndicator = GetComponent<ShopRadiusIndicator>();

        // Auto-find the ShopPrompt UIDocument if not assigned in Inspector
        if (shopPromptDocument == null)
        {
            var docs = FindObjectsOfType<UIDocument>(true);
            foreach (var doc in docs)
            {
                if (doc != null && doc.visualTreeAsset != null &&
                    doc.visualTreeAsset.name.Equals("ShopPrompt", System.StringComparison.OrdinalIgnoreCase))
                {
                    shopPromptDocument = doc;
                    break;
                }
            }
        }

        BuildPromptLabel();
    }

    private void BuildPromptLabel()
    {
        if (shopPromptDocument == null || shopPromptDocument.rootVisualElement == null) return;

        promptLabel = shopPromptDocument.rootVisualElement.Q<Label>("ShopPromptLabel");

        // Start hidden
        SetPromptVisible(false);
    }

    private void Update()
    {
        // Retry finding the label every frame until found (UIDocument may not be ready in Start)
        if (promptLabel == null)
            BuildPromptLabel();

        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactionRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            SetPromptVisible(inRange && (spellShopUI == null || !spellShopUI.IsOpen));
            // Circle shows when far away, hides once player steps inside
            if (radiusIndicator != null) radiusIndicator.SetVisible(!inRange);
        }

        // B key: open or close shop
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (spellShopUI == null) return;

            if (spellShopUI.IsOpen)
            {
                spellShopUI.CloseShop();
                SetPromptVisible(playerInRange);
            }
            else if (playerInRange)
            {
                SetPromptVisible(false);
                spellShopUI.OpenShop();
            }
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptLabel == null) return;
        promptLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // Draw radius in the editor for easy tuning
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
