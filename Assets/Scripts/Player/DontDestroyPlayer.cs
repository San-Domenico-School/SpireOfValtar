using UnityEngine;

/***************************************************
 * Player.cs
 * 
 * Player prefab root script.
 * Registers with PlayerSession and persists data when needed.
 * Does NOT use DontDestroyOnLoad.
 * Gleb
 * 01.27.2026
 ***************************************************/
public class Player : MonoBehaviour
{
    private PlayerSession session;

    void Awake()
    {
        session = FindFirstObjectByType<PlayerSession>(FindObjectsInactive.Include);
        if (session != null)
        {
            session.ApplyToPlayer(this);
        }
    }

    void OnDestroy()
    {
        if (session != null && !session.RestartFlag)
        {
            session.SaveFromPlayer(this);
        }
    }
}