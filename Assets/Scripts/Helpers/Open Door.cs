using Unity.VisualScripting;
using UnityEngine;

/************************************
* Opens the door to the great hall from the outside 
* and controls when audio plays 
* Seamus
* Version 1.0
************************************/

public class OpenDoor : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    private bool first; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator.speed = 0;
    }


    private void OnTriggerEnter(Collider other) 
    {
        if (first)
            return;

        if (other.CompareTag("Player"))
        {
            animator.speed = 1;
            audioSource.Play();
            first = true;
        }
    }
}
