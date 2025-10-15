using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryFailureManager : MonoBehaviour
{
    [SerializeField] GameObject Door;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;
    public int playerHealth = 100;
    private int lastLoggedHealth;
    public static VictoryFailureManager Instance;
    private bool isGameOver = false;
    private bool isVictory = false;
    //TESTING FIELDS
    //private float enemyDecreaseTimer = 0f;
    //private float enemyDecreaseInterval = 1f;
    //private float healthDecreaseTimer = 0f;
    //private float healthDecreaseInterval = 1f;

    private void Start()
    {
        GameObject.Find("Door").tag = "Locked"; // door starts locked
        playerHealth = 100;
        lastLoggedHealth = playerHealth;
        Debug.Log("Start health:" + playerHealth);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (victoryUI) victoryUI.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(false);

    }

    private void Update() // called every frame
    {

        if (playerHealth <= 0)
        {
            isGameOver = true;
            GameOver();
        }

        if (playerHealth != lastLoggedHealth)
        {
            Debug.Log("Player health = " + playerHealth);
            lastLoggedHealth = playerHealth;
        }

        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            isVictory = true;
            LevelComplete();
        }
//<<<<<<< Updated upstream
//    }
//=======
    }
        void GameOver() // level lost
        {
            if (!isGameOver == false)
            {
                isGameOver = true;
                if (gameOverUI == false)
                {
                    gameOverUI.SetActive(true);
                }

            gameOverUI.SetActive(true);
            Time.timeScale = 0f;
            RestartLevel();
            }
        }

        private void LevelComplete() // level completed
        {
            if (!isVictory == false)
            {
                isVictory = true;

                if (isVictory == true)
                {
                    Door.tag = "Completed";
                }
                if (victoryUI == false)
                {
                    victoryUI.SetActive(true);
                }
                victoryUI.SetActive(true);
                if (Door.CompareTag("Completed"))
                {
                    Debug.Log("Door open");
                }

                Destroy(Door);
            }
            else
            {
                Debug.Log("Door is not responding");
            }

        }

        public void RestartLevel()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Restarted");
        }
    }

//>>>>>>> Stashed changes
