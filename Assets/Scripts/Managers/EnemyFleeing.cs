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
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= fleeDistance)
        {
            Vector3 fleeDirection = (transform.position - player.position).normalized;
            if (!Physics.Raycast(transform.position, fleeDirection, wallCheckDistance, wallLayer))
            {
                transform.position += fleeDirection * moveSpeed * Time.deltaTime;
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