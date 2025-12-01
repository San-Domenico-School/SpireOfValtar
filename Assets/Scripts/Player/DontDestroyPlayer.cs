using UnityEngine;

public class PersistentSingleton : MonoBehaviour
{
    private static PersistentSingleton _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // prevent duplicates
        }
    }
}