using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class KeybindUtils
{
    private static readonly Dictionary<string, (string actionName, string partName)> KeybindMap =
        new Dictionary<string, (string actionName, string partName)>
        {
            { "Keybind_MoveForward", ("Move", "up") },
            { "Keybind_MoveBackward", ("Move", "down") },
            { "Keybind_MoveLeft", ("Move", "left") },
            { "Keybind_MoveRight", ("Move", "right") },
            { "Keybind_Jump", ("Jump", null) },
            { "Keybind_CastSpell", ("Attack", null) },
            { "Keybind_MeleeAttack", ("MeleeAttack", null) },
            { "Keybind_NextSpell", ("Next", null) },
            { "Keybind_PreviousSpell", ("Previous", null) }
        };

    public static void ApplySavedKeybinds(InputActionAsset inputActions)
    {
        if (inputActions == null) return;

        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) return;

        foreach (var kvp in KeybindMap)
        {
            ApplySavedBinding(playerMap, kvp.Key, kvp.Value.actionName, kvp.Value.partName);
        }
    }

    private static void ApplySavedBinding(InputActionMap playerMap, string buttonName, string actionName, string partName)
    {
        InputAction action = playerMap.FindAction(actionName);
        if (action == null) return;

        int bindingIndex = FindBindingIndex(action, partName);
        if (bindingIndex == -1) return;

        string key = $"Keybind_{buttonName}_{actionName}_{partName}";
        if (!PlayerPrefs.HasKey(key)) return;

        string overridePath = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(overridePath)) return;

        action.ApplyBindingOverride(bindingIndex, overridePath);
    }

    private static int FindBindingIndex(InputAction action, string partName)
    {
        if (partName != null)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isPartOfComposite && action.bindings[i].name == partName)
                {
                    return i;
                }
            }
        }
        else
        {
            // Prefer Keyboard&Mouse bindings
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    if (binding.groups != null && binding.groups.Contains("Keyboard&Mouse"))
                    {
                        return i;
                    }
                }
            }

            // Fallback to first non-composite binding
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}

