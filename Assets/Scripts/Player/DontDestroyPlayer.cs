using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject); // prevent duplicates
        }
    }
}