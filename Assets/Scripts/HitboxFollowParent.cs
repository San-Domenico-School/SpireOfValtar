using UnityEngine;

public class HitboxFollowParent : MonoBehaviour
{
    public Vector3 offset;

    [Tooltip("The transform to follow. If left empty, defaults to this object's parent.")]
    [SerializeField] private Transform targetOverride;

    private Transform target;

    private void Awake()
    {
        target = targetOverride != null ? targetOverride : transform.parent;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = target.rotation;
    }
}
