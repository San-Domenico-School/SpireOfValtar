using UnityEngine;

/************************************
 * Drop this script on any GameObject in the ChasingLevelTeddy scene.
 * It shows the "Chase the book" HUD message when the scene starts.
 * Hanna 04/16/26
 ************************************/
public class ChaseBookTrigger : MonoBehaviour
{
    [Tooltip("Delay in seconds before the message appears after the scene loads.")]
    [SerializeField] private float delayBeforeShow = 0.5f;

    private void Start()
    {
        if (delayBeforeShow <= 0f)
            TriggerMessage();
        else
            Invoke(nameof(TriggerMessage), delayBeforeShow);
    }

    private void TriggerMessage()
    {
        var manager = FindFirstObjectByType<GameUIManager>(FindObjectsInactive.Include);
        if (manager != null)
            manager.ShowChaseBookMessage();
        else
            Debug.LogWarning("[ChaseBookTrigger] GameUIManager not found in scene.");
    }
}
