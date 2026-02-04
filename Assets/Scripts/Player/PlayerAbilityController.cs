using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; // ✅ Added for UI Toolkit

public class PlayerAbilityController : MonoBehaviour
{
    private const string GameViewUxmlName = "Game_View";
    /************************************
     * Handles spell selection and casting.
     * Now includes a stamina system and connected to UI.
     * Teddy F 10/21/25
     * Version 2.1
     ************************************/

    [Header("Spells")]
    [SerializeField] private LightningSpell spell1;
    [SerializeField] private FireballCaster spell2;
    [SerializeField] private FreezeCaster spell3;

    [Header("Spell Audio")]
    [Tooltip("Optional: plays cast/hit SFX for spells. Put SpellAudioController on the Player and assign here, or leave empty to auto-find on this GameObject.")]
    [SerializeField] private SpellAudioController spellAudio;

    [Header("Audio")]
    [Tooltip("Optional: plays a sound each time a spell is successfully cast.")]
    [SerializeField] private PlayerSoundController playerSound;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference nextAction;
    [SerializeField] private InputActionReference previousAction;
    [SerializeField] private InputActionReference attackAction;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 10f; // stamina per second
    private float currentStamina;

    [Header("UI")] // ✅ Added UI connection
    [SerializeField] private UIDocument uiDocument; // Drag your UI Document here
    [SerializeField] private SpellUI spellUI; // Reference to SpellUI component
    private ProgressBar staminaBar; // Reference to ProgressBar in UI Builder

    private int currentIndex = 0;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    public void ApplySessionData(float current, float max)
    {
        maxStamina = Mathf.Max(1f, max);
        currentStamina = Mathf.Clamp(current, 0f, maxStamina);
        UpdateStaminaUI();
    }

    private void Awake()
    {
        currentStamina = maxStamina;
        if (playerSound == null) playerSound = GetComponent<PlayerSoundController>();
        if (spellAudio == null) spellAudio = GetComponent<SpellAudioController>();
        if (spellAudio == null) spellAudio = GetComponentInParent<SpellAudioController>();
        if (spellAudio == null) spellAudio = FindFirstObjectByType<SpellAudioController>();
    }

    public void ResetState()
    {
        currentStamina = maxStamina;
        currentIndex = 0;
        UpdateStaminaUI();
        UpdateSpellUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (nextAction != null)
        {
            nextAction.action.performed += OnNext;
            nextAction.action.Enable();
        }

        if (previousAction != null)
        {
            previousAction.action.performed += OnPrevious;
            previousAction.action.Enable();
        }

        if (attackAction != null)
        {
            attackAction.action.performed += OnAttack;
            attackAction.action.Enable();
        }

        Debug.Log("Starting on Spell 1");

        BindStaminaUI();

        // Initialize spell UI
        if (spellUI != null)
        {
            spellUI.SetCurrentSpell(currentIndex);
        }
        else
        {
            // Try to find SpellUI automatically if not assigned
            spellUI = FindFirstObjectByType<SpellUI>();
            if (spellUI != null)
            {
                spellUI.SetCurrentSpell(currentIndex);
            }
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (nextAction != null)
        {
            nextAction.action.performed -= OnNext;
            nextAction.action.Disable();
        }

        if (previousAction != null)
        {
            previousAction.action.performed -= OnPrevious;
            previousAction.action.Disable();
        }

        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttack;
            attackAction.action.Disable();
        }

        staminaBar = null;
        uiDocument = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindStaminaUI();
        ResolveSpellUI();
        UpdateSpellUI();
    }

    private void BindStaminaUI()
    {
        ResolveStaminaUIDocument();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                uiDocument = null;
                return;
            }
            staminaBar = root.Q<ProgressBar>("StaminaProgressBar"); // Must match name in UI Builder
            if (staminaBar != null)
            {
                staminaBar.lowValue = 0;
                staminaBar.highValue = maxStamina;
                staminaBar.value = currentStamina;
            }
            else
            {
                Debug.LogWarning("No ProgressBar named 'StaminaProgressBar' found in UI Document!");
            }
        }
    }

    private void ResolveStaminaUIDocument()
    {
        if (uiDocument != null)
        {
            return;
        }

        var liveDocuments = FindObjectsOfType<UIDocument>(true);
        foreach (var document in liveDocuments)
        {
            if (document == null || document.rootVisualElement == null)
            {
                continue;
            }

            if (document.rootVisualElement.Q<ProgressBar>("StaminaProgressBar") != null)
            {
                uiDocument = document;
                return;
            }
        }

        var documents = FindObjectsOfType<UIDocument>(true);
        foreach (var document in documents)
        {
            if (document == null || document.visualTreeAsset == null)
            {
                continue;
            }

            if (document.visualTreeAsset.name.Equals(GameViewUxmlName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (document.rootVisualElement != null)
                {
                    uiDocument = document;
                }
                return;
            }
        }
    }

    public void RefreshStaminaUI()
    {
        uiDocument = null;
        BindStaminaUI();
        UpdateStaminaUI();
    }

    private void Update()
    {
        RegenerateStamina();
        UpdateStaminaUI(); //update bar each frame
        // Debug.Log($"Current Stamina: {currentStamina:F1}"); //use this for debugging
        // Debug.Log("update is called in PAC");
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
    }

    private void UpdateStaminaUI() // ✅ Added method to sync stamina bar
    {
        if (staminaBar != null)
            staminaBar.value = currentStamina;
    }

    private bool TryUseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            return true;
        }
        else
        {
            Debug.Log("Not enough stamina!");
            return false;
        }
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        currentIndex = (currentIndex + 1) % 3;
        Debug.Log($"Selected Spell {currentIndex + 1}");
        UpdateSpellUI();
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        currentIndex = (currentIndex - 1 + 3) % 3;
        Debug.Log($"Selected Spell {currentIndex + 1}");
        UpdateSpellUI();
    }

    private void UpdateSpellUI()
    {
        if (spellUI == null)
        {
            ResolveSpellUI();
        }

        if (spellUI != null)
        {
            spellUI.SetCurrentSpell(currentIndex);
        }
    }

    private void ResolveSpellUI()
    {
        if (spellUI != null)
        {
            return;
        }

        spellUI = FindFirstObjectByType<SpellUI>(FindObjectsInactive.Include);
        if (spellUI == null)
        {
            spellUI = FindFirstObjectByType<SpellUI>();
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        switch (currentIndex)
        {
            case 0: // Lightning Spell
                if (spell1 != null)
                {
                    if (!spell1.canCast)
                    {
                        return;
                    }
                    if (TryUseStamina(30f)) // Balanced for souls-like: High cost for high damage
                    {
                        Debug.Log("Casting Spell 1 (Lightning)");
                        if (spellAudio != null) spellAudio.PlayCast(SpellSfxId.Lightning);
                        spell1.OnCast();
                    }
                }
                else Debug.LogWarning("Spell 1 is not assigned");
                break;

            case 1: // Fireball Spell
                if (spell2 != null)
                {
                    if (!spell2.canCast)
                    {
                        return;
                    }
                    if (TryUseStamina(25f)) // Balanced for souls-like: Medium cost for AOE damage
                    {
                        Debug.Log("Casting Spell 2 (Fireball)");
                        if (spellAudio != null) spellAudio.PlayCast(SpellSfxId.Fireball);
                        spell2.OnCast();
                    }
                }
                else Debug.LogWarning("Spell 2 is not assigned");
                break;

            case 2: // Freeze Spell
                if (spell3 != null)
                {
                    if (!spell3.canCast)
                    {
                        return;
                    }
                    if (TryUseStamina(30f)) // Balanced for souls-like: High cost for utility/CC
                    {
                        Debug.Log("Casting Spell 3 (Freeze)");
                        if (spellAudio != null) spellAudio.PlayCast(SpellSfxId.Freeze);
                        spell3.OnCast();
                    }
                }
                else Debug.LogWarning("Spell 3 is not assigned");
                break;
        }
    }
}
