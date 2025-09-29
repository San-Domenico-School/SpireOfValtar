using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerInteractor : MonoBehaviour
{
    private InteractTarget _current;

    void Update()
    {
        // Press E to interact (new Input System polling)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_current != null)
            {
                _current.Interact(gameObject);
            }
            else
            {
                Debug.Log("No interactable in range.");
            }
        }
    }

    // Called when *either* collider is a trigger; works with pickup triggers
    void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<InteractTarget>();
        if (t != null)
        {
            _current = t;
            Debug.Log("Focused: " + t.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var t = other.GetComponent<InteractTarget>();
        if (t != null && _current == t)
        {
            _current = null;
            Debug.Log("Unfocused: " + t.name);
        }
    }
}