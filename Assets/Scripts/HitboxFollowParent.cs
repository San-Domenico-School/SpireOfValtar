using UnityEngine;

public class HitboxFollowParent : MonoBehaviour
{
    public Vector3 offset;

    private Transform target;

    private void Awake()
    {
        target = transform.parent;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = target.rotation;
    }
}
