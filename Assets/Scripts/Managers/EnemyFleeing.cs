using UnityEngine;
using UnityEngine.AI;

public class EnemyFleeing : MonoBehaviour
{
    // Movement
    public Transform player;
    public float fleeDistance = 10f;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;

    // Rotate enemy
    public float rotationSpeed = 10f;
    public float lockedY = 2f;

    // Check distance from wall
    public float wallCheckDistance = 1f;
    public LayerMask wallLayer;

    // Level Progression GameObject
    public GameObject Teleporter;

    public float stuckThreshold = 0.05f;
    private Vector3 lastPosition;

    public float bodyRadius = 0.3f;

    void Start()
    {
        lockedY = transform.position.y;

        lastPosition = transform.position;
    }

    private void Awake()
    {
        AssignPlayer();
    }

    void AssignPlayer()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("Player not found in scene!");
            }
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= fleeDistance)
        {
            Vector3 fleeDirection = (transform.position - player.position).normalized;

            if (IsBlocked(fleeDirection) || IsStuck())
            {
                fleeDirection = FindEscapeDirection(fleeDirection);
            }

            lastPosition = transform.position;

            if (!Physics.Raycast(transform.position, fleeDirection, wallCheckDistance, wallLayer))
            {
                float speedMultiplier = Mathf.Clamp01(1f - (distance / fleeDistance));
                float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedMultiplier);

                transform.position += fleeDirection * currentSpeed * Time.deltaTime;
            }
            else
            {
                // Optional: slightly turn away if hitting a wall
                fleeDirection = Quaternion.Euler(0, Random.Range(-90, 90), 0) * fleeDirection;
            }

            if (fleeDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

            }

        }
    }

    bool IsBlocked(Vector3 direction)
    {
        return Physics.SphereCast(
            transform.position,
            bodyRadius,
            direction,
            out _,
            wallCheckDistance,
            wallLayer
        );
    }

    bool IsStuck()
    {
        return Vector3.Distance(transform.position, lastPosition) < stuckThreshold;
    }

    Vector3 FindEscapeDirection(Vector3 baseDirection)
    {
        for (int angle = 30; angle <= 180; angle += 30)
        {
            Vector3 left = Quaternion.Euler(0, angle, 0) * baseDirection;
            Vector3 right = Quaternion.Euler(0, -angle, 0) * baseDirection;

            if (!IsBlocked(left)) return left;
            if (!IsBlocked(right)) return right;
        }

        return Random.insideUnitSphere.normalized;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = lockedY;
        transform.position = pos;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Teleporter != null)
            {
                Teleporter.SetActive(true);
                Debug.Log("Next room is open");
            }

            Destroy(gameObject);

            Debug.Log($"{gameObject.name} was caught");
        }
    }
}