using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyFleeController : MonoBehaviour
{
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

    private NavMeshAgent agent;
    private Transform player;

    private bool isFleeing = false;
    private Transform currentWaypoint;
    private HashSet<Transform> visitedWaypoints = new HashSet<Transform>();

    public GameObject door;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

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
            isFleeing = false;

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
