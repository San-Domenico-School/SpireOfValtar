using UnityEngine;

public class DontDestroyPlayer : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
