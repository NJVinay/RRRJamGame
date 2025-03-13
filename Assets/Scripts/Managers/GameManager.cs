using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ✅ Singleton Instance for global access
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGamePaused = false;
    public bool isGameOver = false;

    [Header("Player Stats (Optional/Future)")]
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
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isGamePaused) ResumeGame();
            else PauseGame();
        }

        // Press 'R' to restart game/map (test purposes)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    // ✅ Score Handling
    public void AddScore(int points)
    {
        playerScore += points;
        Debug.Log("Score: " + playerScore);
        // Future: Call UI update here
    }

    // ✅ Lives Handling (Optional for future)
    public void PlayerDied()
    {
        playerLives--;
        Debug.Log("Player Lives Left: " + playerLives);

        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Optional: Respawn player, regenerate map, etc.
        }
    }

    // ✅ Game Over Logic
    public void GameOver()
    {
        isGameOver = true;
        PauseGame(); // Freeze the game
        Debug.Log("Game Over!");
        // Future: Trigger Game Over UI
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

        // Regenerate map if assigned
        if (mapGenerator != null)
        {
            mapGenerator.SendMessage("GenerateMap"); // Call map generation
        }

        // Optional: Scene reload alternative (if using fixed levels)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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