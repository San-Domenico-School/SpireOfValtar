using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class InteractTarget : MonoBehaviour
{
    [Header("What happens when player interacts")]
    public UnityEvent onInteract;        // no params (simplest)

    public void Interact(GameObject _)
    {
        onInteract?.Invoke();            // do whatever you wired in the Inspector
    }
}