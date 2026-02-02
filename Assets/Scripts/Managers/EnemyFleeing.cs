using UnityEngine;
using UnityEngine.AI;

public class EnemyFleeing : MonoBehaviour
{
    // Movement
    public Transform player;
    public float fleeDistance = 10f;
    public float moveSpeed = 3f;

    // Rotate enemy
    public float rotationSpeed = 10f;
    public float lockedY = 2f;

    // Check distance from wall
    public float wallCheckDistance = 1f;
    public LayerMask wallLayer;

    // Level Progression GameObject
    public GameObject Teleporter;

    void Start()
    {
        lockedY = transform.position.y;
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
    if (player == null) return;

    float distance = Vector3.Distance(transform.position, player.position);

    if (distance <= fleeDistance)
    {
        Vector3 fleeDir = (transform.position - player.position).normalized;
        fleeDir = GetFleeDirectionWithSliding(fleeDir);

        transform.position += fleeDir * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(fleeDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
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