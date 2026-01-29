using UnityEngine;
using System.Collections;

public class FreezeProjectile : MonoBehaviour
{
  [Header("Freeze Projectile Settings")]
  [SerializeField] private float speed = 10f;
  [SerializeField] private float lifetime = 10f;
  [SerializeField] private AnimationCurve arcCurve;
  [SerializeField] private float arcHeight = 1f;
  [SerializeField] private float freezeSlowAmount = 0.6f; // 60% slower (0.4x speed) - Balanced for souls-like
  [SerializeField] private float freezeDuration = 4f; // How long the freeze effect lasts - Balanced for souls-like
  [SerializeField] private float freezeRadius = 5f; // AOE
  [SerializeField] private float damage = 15f; // ice damage amount. *Only effective to evo wizard at phase 3*


  // Called by FreezeCaster
  public void Launch(Vector3 direction)
  {
    StopAllCoroutines();
    StartCoroutine(MoveRoutine(direction));
  }

  private IEnumerator MoveRoutine(Vector3 direction)
  {
    Vector3 startPos = transform.position;
    Vector3 forward = direction.normalized;

    float t = 0f;
    float distanceTravelled = 0f;

    while (t < lifetime)
    {
      float dt = Time.deltaTime;
      float moveStep = speed * dt;
      distanceTravelled += moveStep;

      // Base position along the *true* shoot direction
      Vector3 basePos = startPos + forward * distanceTravelled;

      // Arc offset added on top (only vertical)
      float normalizedTime = lifetime > 0f ? t / lifetime : 0f;
      float curveValue = arcCurve != null ? arcCurve.Evaluate(normalizedTime) : normalizedTime;
      float arcOffset = curveValue * arcHeight;

      Vector3 nextPos = basePos + Vector3.up * arcOffset;

      // Raycast from current to next position
      Vector3 displacement = nextPos - transform.position;
      float dist = displacement.magnitude;

      if (dist > 0f && Physics.Raycast(transform.position, displacement.normalized, out RaycastHit hit, dist))
      {
        Debug.Log($"Freeze spell hit {hit.collider.name} (Tag: {hit.collider.tag})");

        if (hit.collider.CompareTag("Enemy"))
        {
          Debug.Log("Freeze spell hit an enemy! Effecting...");

          Collider[] hitCollider = Physics.OverlapSphere(hit.point, freezeRadius);
          foreach (Collider nearby in hitCollider)
          {
            if (nearby.CompareTag("Enemy"))
            {
              var boss = nearby.GetComponent<EvoWizardController>();
              var enemyHealth = nearby.GetComponent<EnemyHealth>();

              if (boss != null)
              {
                boss.ApplyTypedDamage(damage, DamageType.Ice);
                boss.ApplyFreezeEffect(freezeSlowAmount, freezeDuration); // IMPORTANT: boss needs this method
              }
              else
              {
                // Apply slow to normal enemies (WizardController / EnemyController)
                var wiz = nearby.GetComponent<WizardController>();
                if (wiz != null) wiz.ApplyFreezeEffect(freezeSlowAmount, freezeDuration);

                var enemy = nearby.GetComponent<EnemyController>();
                if (enemy != null) enemy.ApplyFreezeEffect(freezeSlowAmount, freezeDuration);

                var slime = nearby.GetComponent<SlimeController>();
                if (slime != null) slime.ApplyFreezeEffect(freezeSlowAmount, freezeDuration);
              }
            }
          }
        }

        Destroy(gameObject);
        yield break;
      }

      // Apply position & orientation
      transform.position = nextPos;
      transform.rotation = Quaternion.LookRotation(forward, Vector3.up); // only pitch/yaw

      t += dt;
      yield return null;
    }

    Debug.Log("Freeze spell expired");
    Destroy(gameObject);
  }
}
