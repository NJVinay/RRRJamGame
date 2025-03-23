using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ✅ Singleton Instance for global access
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGamePaused = false;
    public bool isGameOver = false;

    [Header("Player Stats")]
    [SerializeField] private int playerLives = 3; // Future use if you add lives system
    public int PlayerLives => playerLives;
    private int playerScore = 0;
    public int PlayerScore => playerScore;

    [Header("Map Generator (Optional)")]
    public MapGenerationScript mapGenerator; // Assign via Inspector if you want to regenerate maps

    // ✅ Ensure Singleton setup
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
    }

    // ✅ Handle Input (Pause/Restart)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && !isGameOver) // NOTE - This needs to be changed at some point to be in-line with the new input system
        {
            if (isGamePaused) ResumeGame();
            else PauseGame();
        }

        // Press 'P' to restart game/map (test purposes)
        if (Input.GetKeyDown(KeyCode.P))
        {
            RestartGame();
        }
    }

    // ✅ Lives Handling (Optional for future)
    public void PlayerDied()
    {
        RestartGame();
    }

    // ✅ Restart the game/map
    public void RestartGame()
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f; // Unpause if paused
        playerLives = 3; // Reset lives (optional, future)
        playerScore = 0; // Reset score
        Debug.Log("Game Restarted");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ✅ Pause/Resume Game
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
        // Future: Show Pause Menu UI
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
        // Future: Hide Pause Menu UI
    }
}