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
    [SerializeField] private LightningSpell spell1; // make sure that the class is the name of the script.
    [SerializeField] private FireballSpell spell2;
    [SerializeField] private TestSpell3 spell3;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference nextAction;
    [SerializeField] private InputActionReference previousAction;
    [SerializeField] private InputActionReference attackAction;

    private int currentIndex = 0;

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
            case 0:
                if (spell1 != null)
                {
                    Debug.Log("Casting Spell 1");
                    spell1.OnCast();
                }
                else Debug.LogWarning("Spell 1 is not assigned");
                break;

            case 1:
                if (spell2 != null)
                {
                    Debug.Log("Casting Spell 2");
                    spell2.OnCast();
                }
                else Debug.LogWarning("Spell 2 is not assigned");
                break;

            case 2:
                if (spell3 != null)
                {
                    Debug.Log("Casting Spell 3");
                    spell3.OnCast();
                }
                else Debug.LogWarning("Spell 3 is not assigned");
                break;
        }
    }
}
