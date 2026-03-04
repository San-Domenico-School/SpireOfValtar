using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyFleeController : MonoBehaviour
{
<<<<<<< HEAD
    // Movement
    public Transform player;
    public float fleeDistance = 10f;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
=======
    public Transform[] waypoints;
>>>>>>> 651b3d4d2eb165bb2f2cf145893e7bc3c3df0538

    // Fleeing settings
    public float fleeRange = 10f;
    public float waypointSwitchDistance = 2.5f;

    // Enemy movement settings
    public float normalSpeed = 3.5f;
    public float fleeSpeed = 6.5f;
    public float normalAngularSpeed = 360f;
    public float fleeAngularSpeed = 360f;
    public float normalAcceleration = 5f;
    public float fleeAcceleration = 25f;
    public float fixedYPosition = 1.1f;

    private NavMeshAgent agent;
    private Transform player;

    private bool isFleeing = false;
    private Transform currentWaypoint;
    private HashSet<Transform> visitedWaypoints = new HashSet<Transform>();

    public GameObject door;

    public float stuckThreshold = 0.05f;
    private Vector3 lastPosition;

    public float bodyRadius = 0.3f;

    void Start()
    {
<<<<<<< HEAD
        lockedY = transform.position.y;

        lastPosition = transform.position;
    }
=======
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
>>>>>>> 651b3d4d2eb165bb2f2cf145893e7bc3c3df0538

        agent.speed = normalSpeed;
        agent.angularSpeed = normalAngularSpeed;
        agent.acceleration = normalAcceleration;
        agent.autoBraking = false;
    }

    void Update()
    {
        if (player == null || waypoints.Length == 0)
            return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        if (playerDistance <= fleeRange)
        {
<<<<<<< HEAD
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
=======
            if (!isFleeing)
            {
                isFleeing = true;
>>>>>>> 651b3d4d2eb165bb2f2cf145893e7bc3c3df0538

                agent.speed = fleeSpeed;
                agent.angularSpeed = fleeAngularSpeed;
                agent.acceleration = fleeAcceleration;

                visitedWaypoints.Clear();
                SelectNextWaypoint();
            }

        }
        else if (isFleeing)
        {
            isFleeing = false;

<<<<<<< HEAD
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
=======
            agent.speed = normalSpeed;
            agent.angularSpeed = normalAngularSpeed;
            agent.acceleration = normalAcceleration;

            visitedWaypoints.Clear();
            agent.ResetPath();
            currentWaypoint = null;
            return;
        }

        if (isFleeing && currentWaypoint != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointSwitchDistance)
            {
                visitedWaypoints.Add(currentWaypoint);
                SelectNextWaypoint();
            }
        }
>>>>>>> 651b3d4d2eb165bb2f2cf145893e7bc3c3df0538
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = fixedYPosition;
        transform.position = pos;
    }

    void SelectNextWaypoint()
    {
        Transform bestWaypoint = null;
        float bestScore = Mathf.NegativeInfinity;

        foreach (Transform waypoint in waypoints)
        {
            if (visitedWaypoints.Contains(waypoint))
                continue;

            float distanceToEnemy = Vector3.Distance(transform.position, waypoint.position);
            float distanceToPlayer = Vector3.Distance(player.position, waypoint.position);

            float score = distanceToPlayer - distanceToEnemy;

            if (score > bestScore)
            {
                bestScore = score;
                bestWaypoint = waypoint;
            }
        }

        if (bestWaypoint == null)
        {
            visitedWaypoints.Clear();
            SelectNextWaypoint();
            return;
        }

        currentWaypoint = bestWaypoint;
        agent.SetDestination(currentWaypoint.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
    }

    void HandlePlayerCollision()
    {
        Debug.Log("Enemy caught");
        if (door != null)
        {
            door.SetActive(true);
            Debug.Log("Door enabled");
        }
        else
        {
            Debug.LogWarning("Door not found in the scene!");
        }
        Destroy(gameObject);
    }
}
