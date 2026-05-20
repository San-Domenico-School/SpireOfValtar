// v1 Gleb created
// Attach to CoinsPlaceholder prefab.
// Works with CharacterController players by polling proximity each frame.
// OnTriggerEnter is a bonus fallback in case a trigger overlap fires too.

using Unity.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    private bool collected = false;
    private Transform playerTransform;
    private const float pickupRadius = 1.2f;
    private float randomRotationOffset;
    private float timeToDestroy;

    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        randomRotationOffset = Random.Range(0, 360);
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0,Time.timeSinceLevelLoad * 100 + randomRotationOffset,0);

        if (collected)
        {
            timeToDestroy += Time.deltaTime;
            if(timeToDestroy > 1)
            {
                // destroy after sound stops playing
                Destroy(this.gameObject);
            }
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) <= pickupRadius)
        {
            Collect();
        }
        
    }
    

    private void Collect()
    {
        collected = true;
        if (GoldCollector.Instance != null)
            GoldCollector.Instance.AddGold(1);
        
        audioSource.Play();
        gameObject.GetComponent<BoxCollider>().enabled =  false;
        gameObject.GetComponent<MeshRenderer>().enabled =  false;

    }
}
