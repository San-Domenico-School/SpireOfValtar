using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityController : MonoBehaviour
{
    /************************************
     * Handles spell selection and casting.
     * Now includes a stamina system.
     * Teddy F 10/21/25
     * Version 2.0
     ************************************/

    [Header("Spells")]
    [SerializeField] private LightningSpell spell1;
    [SerializeField] private FireballCaster spell2;
    [SerializeField] private FreezeCaster spell3;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference nextAction;
    [SerializeField] private InputActionReference previousAction;
    [SerializeField] private InputActionReference attackAction;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 10f; // stamina per second
    private float currentStamina;

    private int currentIndex = 0;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void OnEnable()
    {
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
    }

    private void OnDisable()
    {
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
    }

    private void Update()
    {
        RegenerateStamina();
        Debug.Log($"Current Stamina: {currentStamina:F1}");
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
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
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        currentIndex = (currentIndex - 1 + 3) % 3;
        Debug.Log($"Selected Spell {currentIndex + 1}");
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        switch (currentIndex)
        {
            case 0: // Lightning Spell
                if (spell1 != null)
                {
                    if (TryUseStamina(20f))
                    {
                        Debug.Log("Casting Spell 1 (Lightning)");
                        spell1.OnCast();
                    }
                }
                else Debug.LogWarning("Spell 1 is not assigned");
                break;

            case 1: // Fireball Spell
                if (spell2 != null)
                {
                    if (TryUseStamina(15f))
                    {
                        Debug.Log("Casting Spell 2 (Fireball)");
                        spell2.OnCast();
                    }
                }
                else Debug.LogWarning("Spell 2 is not assigned");
                break;

            case 2: // Freeze Spell (no stamina use yet)
                if (spell3 != null)
                {
                    Debug.Log("Casting Spell 3 (Freeze)");
                    spell3.OnCast();
                }
                else Debug.LogWarning("Spell 3 is not assigned");
                break;
        }
    }
}
