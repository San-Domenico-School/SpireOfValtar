using UnityEngine;

[DisallowMultipleComponent]
public class SkeletonMeleeHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    [Tooltip("Trigger collider used as the weapon hitbox. If empty, will auto-find on this GameObject.")]
    [SerializeField] private Collider hitboxCollider;

    [SerializeField] private SkeletonMeleeController controller;

    private void Awake()
    {
        if (hitboxCollider == null) hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            Debug.LogWarning("SkeletonMeleeHitbox requires a Collider (set as Trigger).", this);
            return;
        }

        if (controller == null) controller = GetComponentInParent<SkeletonMeleeController>();
        if (controller == null)
        {
            Debug.LogWarning("SkeletonMeleeHitbox could not find SkeletonMeleeController in parents.", this);
        }

        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = true;
    }

    private void OnEnable()
    {
        if (hitboxCollider == null) hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = true;
        }
    }

    public void BeginAttack()
    {
        if (hitboxCollider != null) hitboxCollider.enabled = true;
    }

    public void EndAttack()
    {
        // Intentionally do not disable hitbox.
    }

    private void OnTriggerEnter(Collider other)
    {
        controller?.TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        controller?.TryDamage(other);
    }

    private void OnTriggerExit(Collider other)
    {
        controller?.ClearDamageTarget(other);
    }

    private void OnDisable()
    {
        // Keep default behavior on disable: turn off hitbox.
        if (hitboxCollider != null) hitboxCollider.enabled = false;
        controller?.ClearDamageTarget(null);
    }
}
