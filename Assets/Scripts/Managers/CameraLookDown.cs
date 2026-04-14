using UnityEngine;
using System.Collections;

public class CameraLookDownSequence : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float lookDownAngle = 60f;
    [SerializeField] private float rotateSpeed = 3f;
    [SerializeField] private float lookDuration = 2f;

    private Transform cameraPivot;
    private PlayerMovement playerMovement;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            // Get the player movement script
            playerMovement = other.GetComponent<PlayerMovement>();

            // Get the player's camera
            cameraPivot = other.GetComponentInChildren<Camera>().transform;

            StartCoroutine(LookSequence());
        }
    }

    IEnumerator LookSequence()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        float startAngle = cameraPivot.localEulerAngles.x;
        if (startAngle > 180f) startAngle -= 360f;

        float targetAngle = lookDownAngle;

        float t = 0f;

        // Look down
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;

            float angle = Mathf.Lerp(startAngle, targetAngle, t);
            cameraPivot.localRotation = Quaternion.Euler(angle, 0f, 0f);

            yield return null;
        }

        yield return new WaitForSeconds(lookDuration);

        t = 0f;

        // Look back up
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;

            float angle = Mathf.Lerp(targetAngle, startAngle, t);
            cameraPivot.localRotation = Quaternion.Euler(angle, 0f, 0f);

            yield return null;
        }

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}