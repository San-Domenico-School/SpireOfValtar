using UnityEngine;

/************************************
 * controls the stair animation and audio 
 * Seamus
 * Version 1.0
 ************************************/

[RequireComponent(typeof(Animator))]
public class Stairs : MonoBehaviour
{
    public float numberOfEnemies = 1;

    [SerializeField] GameObject audioSource;
    [SerializeField] bool test = false;

    private Animator animator;
    private float playTimeDuration;
    private float playTimeLeft;
    private int progressCount = 0;

    // How many more kills are needed before the stairs fully lower.
    public int KillsRemaining => Mathf.Max(0, (int)numberOfEnemies - progressCount);
    public bool IsComplete    => progressCount >= (int)numberOfEnemies;

    public void Progress()
    {
        progressCount++;
        playTimeLeft += playTimeDuration;
        audioSource.GetComponent<AudioSource>().Play();
        Debug.Log("progress");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        playTimeDuration = 4.0f / numberOfEnemies;
        animator.Play("stairs.001|Cube.001Action");
        animator.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            Progress();
            test = false;
        }

        if (playTimeLeft > 0)
        {
            animator.speed = 1;
            playTimeLeft -= Time.deltaTime;
        }
        else
        {
            animator.speed = 0;
        }
    }

}
