using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFleeing : MonoBehaviour
{
    // Waypoints to choose from when fleeing
    public Transform[] waypoints;

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

    // Optional door to open when player collides
    public GameObject door;

    private NavMeshAgent agent;
    private Transform player;

    private bool isFleeing = false;
    private Transform currentWaypoint;
    private HashSet<Transform> visitedWaypoints = new HashSet<Transform>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent required on EnemyFleeing.");
            enabled = false;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // initialize agent with normal settings
        agent.speed = normalSpeed;
        agent.angularSpeed = normalAngularSpeed;
        agent.acceleration = normalAcceleration;
        agent.autoBraking = false;
    }

    void Update()
    {
        if (player == null || waypoints == null || waypoints.Length == 0)
            return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        if (playerDistance <= fleeRange)
        {
            if (!isFleeing)
            {
                isFleeing = true;
                agent.speed = fleeSpeed;
                agent.angularSpeed = fleeAngularSpeed;
                agent.acceleration = fleeAcceleration;

                visitedWaypoints.Clear();
                SelectNextWaypoint();
            }
        }
        else if (isFleeing)
        {
            // stop fleeing -> restore normal movement
            isFleeing = false;
            agent.speed = normalSpeed;
            agent.angularSpeed = normalAngularSpeed;
            agent.acceleration = normalAcceleration;

            visitedWaypoints.Clear();
            agent.ResetPath();
            currentWaypoint = null;
            return;
        }

        // While fleeing, manage waypoint switching
        if (isFleeing && currentWaypoint != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointSwitchDistance)
            {
                visitedWaypoints.Add(currentWaypoint);
                SelectNextWaypoint();
            }
        }
    }

    void LateUpdate()
    {
        // Keep enemy on fixed Y plane to avoid sinking/floating
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
            if (waypoint == null) continue;
            if (visitedWaypoints.Contains(waypoint))
                continue;

            float distanceToEnemy = Vector3.Distance(transform.position, waypoint.position);
            float distanceToPlayer = Vector3.Distance(player.position, waypoint.position);

            // Prefer waypoints that are far from player and relatively close to enemy
            float score = distanceToPlayer - distanceToEnemy;

            if (score > bestScore)
            {
                bestScore = score;
                bestWaypoint = waypoint;
            }
        }

        if (bestWaypoint == null)
        {
            // all visited or none available: reset visited set and try again
            visitedWaypoints.Clear();
            // if still null after clearing (e.g., all waypoints null), bail out
            foreach (Transform wp in waypoints) if (wp != null) { bestWaypoint = wp; break; }
            if (bestWaypoint == null) return;
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
            Debug.LogWarning("Door not assigned on EnemyFleeing.");
        }
        Destroy(gameObject);
    }
}
