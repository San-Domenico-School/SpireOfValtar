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

    private void OnEnable()
    {
        // Always start hidden - prevents bleed-over from previous scene
        playerInRange = false;
        SetPromptVisible(false);
    }

    private void OnDisable()
    {
        // Hide immediately when this trigger is disabled/scene unloads
        SetPromptVisible(false);
    }

    private void OnDestroy()
    {
        // Hide when shop is destroyed (scene switch)
        SetPromptVisible(false);
    }

    private void Start()
    {
       
        if (playerTransform == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) playerTransform = go.transform;
        }

        
        if (spellShopUI == null)
            spellShopUI = FindFirstObjectByType<SpellShopUI>();

        if (radiusIndicator == null)
            radiusIndicator = GetComponent<ShopRadiusIndicator>();

        
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

        
        SetPromptVisible(false);
    }

    private void Update()
    {
       
        if (promptLabel == null)
            BuildPromptLabel();

        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactionRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            SetPromptVisible(inRange && (spellShopUI == null || !spellShopUI.IsOpen));
          
            if (radiusIndicator != null) radiusIndicator.SetVisible(!inRange);
        }

     
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
