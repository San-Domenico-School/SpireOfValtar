using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 100;

    [SerializeField] RestartManager restartManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        Debug.Log("Health = 100");
    }

    // Player takes (x) amount of damage from enemy
    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Player has taken {amount} damage, remaining health = {health}");
        if (health <= 1)
        {
            restartManager.RestartLevel();
            Debug.Log("Player has died");
        }
    }
}
