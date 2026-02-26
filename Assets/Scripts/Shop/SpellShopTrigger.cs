// v1 Gleb created
// Attach to the shop cube GameObject.
// When the player enters the interaction radius a "Press B to open shop" prompt appears on the HUD.
// Pressing B opens the shop (game freezes); pressing B again closes it.

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

    [Header("Prompt UI (Game_View UIDocument)")]
    [SerializeField] private UIDocument gameViewDocument;

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

        // Build the prompt label and inject it into the Game_View UIDocument
        BuildPromptLabel();
    }

    private void BuildPromptLabel()
    {
        // Try to locate the Game_View UIDocument if not set
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

        if (gameViewDocument == null || gameViewDocument.rootVisualElement == null) return;

        promptLabel = new Label("Press B to open shop");
        promptLabel.name = "ShopPromptLabel";
        promptLabel.style.position        = Position.Absolute;
        promptLabel.style.bottom          = new StyleLength(new Length(18, LengthUnit.Percent));
        promptLabel.style.left            = 0;
        promptLabel.style.right           = 0;
        promptLabel.style.unityTextAlign  = TextAnchor.MiddleCenter;
        promptLabel.style.fontSize        = 20;
        promptLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        promptLabel.style.color           = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        promptLabel.style.display         = DisplayStyle.None;

        gameViewDocument.rootVisualElement.Add(promptLabel);
    }

    private void Update()
    {
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
