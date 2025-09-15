using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityController : MonoBehaviour
{
    /************************************
     * This script is responsible for calling the method that will
     * activate the selected spell. it goes on the player and is
     * independent of all other scripts except the spells equiped.
     * Teddy F 8/15/25
     * Version 1.0
     * **********************************/

    [Header("Spells")]
    [SerializeField] private MonoBehaviour spell1;
    [SerializeField] private MonoBehaviour spell2;
    [SerializeField] private MonoBehaviour spell3;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference nextAction;
    [SerializeField] private InputActionReference previousAction;
    [SerializeField] private InputActionReference attackAction;

    private MonoBehaviour[] spells;
    private int currentIndex = 0;

    private void Awake()
    {
        // Put the spells in an array for easy cycling
        spells = new MonoBehaviour[] { spell1, spell2, spell3 };
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

    private void OnNext(InputAction.CallbackContext context)
    {
        currentIndex = (currentIndex + 1) % spells.Length;
        Debug.Log($"Selected Spell {currentIndex + 1}");
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        currentIndex = (currentIndex - 1 + spells.Length) % spells.Length;
        Debug.Log($"Selected Spell {currentIndex + 1}");
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        var spell = spells[currentIndex];
        if (spell == null)
        {
            Debug.LogWarning($"No spell assigned to slot {currentIndex + 1}");
            return;
        }

        var method = spell.GetType().GetMethod("OnCast");
        if (method != null)
        {
            Debug.Log($"Casting spell {currentIndex + 1}");
            method.Invoke(spell, null);
        }
        else
        {
            Debug.LogWarning($"Spell {currentIndex + 1} has no OnCast method");
        }
    }
}
