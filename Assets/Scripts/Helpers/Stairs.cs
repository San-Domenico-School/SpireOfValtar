using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Stairs : MonoBehaviour
{
    public float numberOfEnemies = 1; 

    private Animator animator;
    private float playTimeDuration;
    private float playTimeLeft;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        playTimeDuration = 4.0f / numberOfEnemies;
        animator.Play("Scene");
        animator.speed = 0;

    }

    // Update is called once per frame
    void Update()
    {
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

    public void Progress()
    {
        playTimeLeft += playTimeDuration; 
    }
}
