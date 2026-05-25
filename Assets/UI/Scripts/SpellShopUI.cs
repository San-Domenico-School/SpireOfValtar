// v1 Gleb created
// Manages the Spell Shop overlay UI.
// Freezes the game (Time.timeScale = 0) while open, mirrors pause menu behaviour.
// Reads gold from GoldCollector, unlocks spells via SpellInventory on purchase.

using UnityEngine;
using UnityEngine.UIElements;

public class SpellShopUI : MonoBehaviour
{
    public static SpellShopUI Instance { get; private set; }
    [Header("UI")]
    [SerializeField] private UIDocument shopDocument;

    [Header("Spell Prices")]
    [SerializeField] private int fireballPrice  = 1;
    [SerializeField] private int lightningPrice = 3;
    [SerializeField] private int freezePrice    = 5;
    [SerializeField] private int dashPrice      = 1;

    private bool isOpen = false;

    // Spell index constants — must match SpellInventory array order
    private const int IDX_LIGHTNING = 0;
    private const int IDX_FIREBALL  = 1;
    private const int IDX_FREEZE    = 2;
    private const int IDX_DASH      = 3;

    // Cached UI elements
    private Label  goldLabel;
    private Label  priceFireball;
    private Label  priceLightning;
    private Label  priceFreeze;
    private Label  priceDash;
    private Button buyFireball;
    private Button buyLightning;
    private Button buyFreeze;
    private Button buyDash;
    private Button closeButton;

    private void Awake()
    {
        Instance = this;

        if (shopDocument == null)
        {
            shopDocument = GetComponent<UIDocument>();
        }
    }

    private void Start()
    {
        BindUI();
        HideShop();
    }

    private void BindUI()
    {
        if (shopDocument == null || shopDocument.rootVisualElement == null) return;

        var root = shopDocument.rootVisualElement;

        goldLabel      = root.Q<Label> ("GoldLabel");
        priceFireball  = root.Q<Label> ("Price_Fireball");
        priceLightning = root.Q<Label> ("Price_Lightning");
        priceFreeze    = root.Q<Label> ("Price_Freeze");
        priceDash      = root.Q<Label> ("Price_Dash");
        buyFireball    = root.Q<Button>("Buy_Fireball");
        buyLightning   = root.Q<Button>("Buy_Lightning");
        buyFreeze      = root.Q<Button>("Buy_Freeze");
        buyDash        = root.Q<Button>("Buy_Dash");
        closeButton    = root.Q<Button>("CloseShopButton");

        if (buyFireball  != null) buyFireball .clicked += () => TryBuy(IDX_FIREBALL,  fireballPrice);
        if (buyLightning != null) buyLightning.clicked += () => TryBuy(IDX_LIGHTNING, lightningPrice);
        if (buyFreeze    != null) buyFreeze   .clicked += () => TryBuy(IDX_FREEZE,    freezePrice);
        if (buyDash      != null) buyDash     .clicked += () => TryBuy(IDX_DASH,      dashPrice);
        if (closeButton  != null) closeButton .clicked += CloseShop;
    }

    public void OpenShop()
    {
        if (isOpen) return;
        isOpen = true;

        // Freeze game like pause menu
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible   = true;

        RefreshUI();

        if (shopDocument != null)
        {
            shopDocument.enabled = true;
            if (shopDocument.rootVisualElement != null)
            {
                BindUI();
                shopDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
    }

    public void CloseShop()
    {
        if (!isOpen) return;
        isOpen = false;

        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible   = false;

        HideShop();

        // Refresh the spell HUD so newly purchased spells show their names
        var spellUI = FindFirstObjectByType<SpellUI>();
        if (spellUI != null) spellUI.RefreshOwnership();
    }

    public bool IsOpen => isOpen;

    private void HideShop()
    {
        if (shopDocument != null && shopDocument.rootVisualElement != null)
        {
            shopDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void RefreshUI()
    {
        if (shopDocument == null || shopDocument.rootVisualElement == null) return;

        int gold = GoldCollector.Instance != null ? GoldCollector.Instance.GetGold() : 0;
        if (goldLabel != null) goldLabel.text = $"Gold: {gold}";

        if (priceFireball  != null) priceFireball.text  = $"{fireballPrice} Gold";
        if (priceLightning != null) priceLightning.text = $"{lightningPrice} Gold";
        if (priceFreeze    != null) priceFreeze.text    = $"{freezePrice} Gold";
        if (priceDash      != null) priceDash.text      = $"{dashPrice} Gold";

        RefreshCard(IDX_FIREBALL,  buyFireball,  "Buy_Fireball");
        RefreshCard(IDX_LIGHTNING, buyLightning, "Buy_Lightning");
        RefreshCard(IDX_FREEZE,    buyFreeze,    "Buy_Freeze");
        RefreshCard(IDX_DASH,      buyDash,      "Buy_Dash");
    }

    private void RefreshCard(int spellIndex, Button btn, string btnName)
    {
        if (btn == null) return;
        bool owned = SpellInventory.Instance != null && SpellInventory.Instance.IsUnlocked(spellIndex);
        btn.text = owned ? "Owned" : "Buy";
        btn.SetEnabled(!owned);
    }

    private void TryBuy(int spellIndex, int price)
    {
        if (SpellInventory.Instance == null || GoldCollector.Instance == null) return;
        if (SpellInventory.Instance.IsUnlocked(spellIndex))
        {
            Debug.Log("[SpellShop] Spell already owned.");
            return;
        }
        if (!GoldCollector.Instance.SpendGold(price))
        {
            Debug.Log("[SpellShop] Not enough gold.");
            return;
        }
        SpellInventory.Instance.UnlockSpell(spellIndex);
        RefreshUI();
    }
}
