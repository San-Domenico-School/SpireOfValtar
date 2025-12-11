using UnityEngine;

public class TeleporterActivater : MonoBehaviour

{
    [SerializeField] public int enemyKillsNeeded = 5;
    [SerializeField] public int enemiesKilled = 0;

    public GameObject objectToActivate;

    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log("+1 Enemy Killed");

        if(enemiesKilled >= enemyKillsNeeded)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Door opened!");
        }
    }
}
