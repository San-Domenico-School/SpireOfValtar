using UnityEngine;
using UnityEngine.AI;

public class EnemyFleeing : MonoBehaviour
{
    public Transform player;
    public float fleeDistance = 10f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float lockedY = 2f;
    

    void Start()
    {
        lockedY = transform.position.y;
    }

 
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= fleeDistance)
        {
            Vector3 fleeDirection = (transform.position - player.position).normalized;
            transform.position += fleeDirection * moveSpeed * Time.deltaTime;

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
            Destroy(gameObject);
            Debug.Log($"{gameObject.name} was caught");
        }
    }
}