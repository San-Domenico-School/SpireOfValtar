using UnityEngine;

// Attach this to the Animator GameObject (Animation_pack_1).
// Animation events will fire here and relay to the root controller.
public class SkeletonMeleeAnimEvents : MonoBehaviour
{
    private SkeletonMeleeController controller;

    private void Awake()
    {
        controller = GetComponentInParent<SkeletonMeleeController>();
        if (controller == null)
        {
            Debug.LogWarning("SkeletonMeleeAnimEvents could not find SkeletonMeleeController in parents.", this);
        }
    }

    public void AnimAttackStart()
    {
        controller?.AnimAttackStart();
    }

    public void AnimAttackEnd()
    {
        controller?.AnimAttackEnd();
    }
}
