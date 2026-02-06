using UnityEngine;
using UnityEngine.AI;

public class EnemyFleeing : MonoBehaviour
{
    // Movement
    public Transform player;
    public float fleeDistance = 10f;
    public float moveSpeed = 3f;
    private Rigidbody rb;
    private Vector3 currentMoveDir;

    // Rotate enemy
    public float rotationSpeed = 10f;
    public float lockedY = 2f;

    // Check distance from wall
    public float wallCheckDistance = 1f;
    public LayerMask wallLayer;

    // Level Progression GameObject
    public GameObject Teleporter;

        void Awake()
{
    rb = GetComponent<Rigidbody>();
    AssignPlayer();
    lockedY = transform.position.y;
}

    void AssignPlayer()
    { 
            GameObject.FindGameObjectWithTag("Player");
    }

void FixedUpdate()
{
    float distance = Vector3.Distance(transform.position, player.position);
    if (distance > fleeDistance) return;

    Vector3 fleeDir = (transform.position - player.position).normalized;
    fleeDir = GetFleeDirectionWithSliding(fleeDir);

    currentMoveDir = fleeDir;

    rb.MovePosition(rb.position + currentMoveDir * moveSpeed * Time.fixedDeltaTime);
}

void Update()
{
    if (currentMoveDir == Vector3.zero) return;

    Quaternion targetRotation = Quaternion.LookRotation(currentMoveDir);
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRotation,
        rotationSpeed * Time.deltaTime
    );
}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }

    Vector3 GetFleeDirectionWithSliding(Vector3 fleeDir)
{
    RaycastHit hit;

    if (Physics.Raycast(transform.position, fleeDir, out hit, wallCheckDistance, wallLayer))
    {
        // Slide along the wall instead of stopping
        Vector3 slideDir = Vector3.ProjectOnPlane(fleeDir, hit.normal).normalized;

        // If sliding fails, force an escape
        if (slideDir == Vector3.zero)
        {
            slideDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
        }

        return slideDir;
    }

    return fleeDir;
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