using UnityEngine;

public class AnimEventRelay : MonoBehaviour
{
    private EvoWizardController boss;

    void Start()
    {
        // Find the boss script on this object or any parent
        boss = GetComponentInParent<EvoWizardController>();
    }

    // This is what the Animation Event calls
    public void OnAttackHit()
    {
        if (boss != null)
            boss.OnAttackHit();
    }
}