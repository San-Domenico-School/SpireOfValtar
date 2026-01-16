using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/************************************
 * Manages spell selection UI display in the game view.
 * Shows spell boxes in bottom right corner with highlighting for current spell.
 * Gleb 01/09/26
 * Version 1.0
 ************************************/
public class SpellUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    
    [Header("Spell Settings")]
    [SerializeField] private List<string> spellNames = new List<string> { "Lightning", "Fireball", "Freeze" };
    
    private VisualElement spellContainer;
    private List<VisualElement> spellBoxes = new List<VisualElement>();
    private int currentSpellIndex = 0;

    private void Start()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            return;
        }

        InitializeSpellUI();
    }

    private void InitializeSpellUI()
    {
        var root = uiDocument.rootVisualElement;
        spellContainer = root.Q<VisualElement>("SpellContainer");

        if (spellContainer == null)
        {
            return;
        }

        // Clear any existing boxes
        spellContainer.Clear();
        spellBoxes.Clear();

        // Create spell boxes dynamically
        for (int i = 0; i < spellNames.Count; i++)
        {
            CreateSpellBox(i, spellNames[i]);
        }

        // Highlight the first spell by default
        UpdateHighlight(0);
    }

    private void CreateSpellBox(int index, string spellName)
    {
        // Main box container
        VisualElement spellBox = new VisualElement();
        spellBox.name = $"SpellBox_{index}";
        
        // Style the spell box to match UI theme
        spellBox.style.width = 100;
        spellBox.style.height = 100;
        spellBox.style.backgroundColor = new Color(40f / 255f, 40f / 255f, 40f / 255f, 1f);
        spellBox.style.borderLeftWidth = 3;
        spellBox.style.borderRightWidth = 3;
        spellBox.style.borderTopWidth = 3;
        spellBox.style.borderBottomWidth = 3;
        spellBox.style.borderLeftColor = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        spellBox.style.borderRightColor = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        spellBox.style.borderTopColor = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        spellBox.style.borderBottomColor = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        spellBox.style.borderTopLeftRadius = 8;
        spellBox.style.borderTopRightRadius = 8;
        spellBox.style.borderBottomLeftRadius = 8;
        spellBox.style.borderBottomRightRadius = 8;
        spellBox.style.justifyContent = Justify.Center;
        spellBox.style.alignItems = Align.Center;
        spellBox.style.position = Position.Relative;
        spellBox.style.paddingTop = 5;
        spellBox.style.paddingBottom = 5;
        spellBox.style.paddingLeft = 5;
        spellBox.style.paddingRight = 5;

        // Spell name label
        Label spellLabel = new Label(spellName);
        spellLabel.name = "SpellLabel";
        spellLabel.style.color = new Color(236f / 255f, 165f / 255f, 41f / 255f, 1f);
        spellLabel.style.fontSize = 16;
        spellLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        spellLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        spellBox.Add(spellLabel);

        // Highlight overlay (initially hidden) - red box on top
        VisualElement highlightOverlay = new VisualElement();
        highlightOverlay.name = "HighlightOverlay";
        highlightOverlay.style.position = Position.Absolute;
        highlightOverlay.style.left = 0;
        highlightOverlay.style.right = 0;
        highlightOverlay.style.top = 0;
        highlightOverlay.style.bottom = 0;
        highlightOverlay.style.borderLeftWidth = 4;
        highlightOverlay.style.borderRightWidth = 4;
        highlightOverlay.style.borderTopWidth = 4;
        highlightOverlay.style.borderBottomWidth = 4;
        highlightOverlay.style.borderLeftColor = Color.red;
        highlightOverlay.style.borderRightColor = Color.red;
        highlightOverlay.style.borderTopColor = Color.red;
        highlightOverlay.style.borderBottomColor = Color.red;
        highlightOverlay.style.display = DisplayStyle.None;
        spellBox.Add(highlightOverlay);

        spellContainer.Add(spellBox);
        spellBoxes.Add(spellBox);
    }

    public void SetSpellNames(List<string> names)
    {
        spellNames = new List<string>(names);
        InitializeSpellUI();
    }

    public void SetCurrentSpell(int index)
    {
        if (index >= 0 && index < spellBoxes.Count)
        {
            currentSpellIndex = index;
            UpdateHighlight(index);
        }
    }

    private void UpdateHighlight(int activeIndex)
    {
        for (int i = 0; i < spellBoxes.Count; i++)
        {
            VisualElement box = spellBoxes[i];
            VisualElement highlight = box.Q<VisualElement>("HighlightOverlay");
            
            if (highlight != null)
            {
                if (i == activeIndex)
                {
                    highlight.style.display = DisplayStyle.Flex;
                }
                else
                {
                    highlight.style.display = DisplayStyle.None;
                }
            }
        }
    }

    public int GetSpellCount()
    {
        return spellBoxes.Count;
    }
}

