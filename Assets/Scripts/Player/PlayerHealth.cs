using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public Image healthBar;      // Reference to the health bar UI element
    public float maxHealth = 100.0f; // Maximum health of the player
    private float currentHealth;  // Current health of the player

    private Vector3 startPosition;

    private void Start()
    {
        currentHealth = maxHealth; // Set current health to maximum at start
        UpdateHealthBar();          // Initialize health bar
        startPosition = transform.position; // Store the initial position of the player
    }

    private void Update()
    {
        // For testing: Press Enter to take damage, and Space to heal
        if (Input.GetKeyDown(KeyCode.I))
        {
            TakeDamage(10.0f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Heal(5.0f);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; // Decrease current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't drop below 0
        UpdateHealthBar(); // Update health bar UI

        if (currentHealth <= 0)
        {
            Die(); // Call die function if player is dead
        }
    }

    public void Heal(float healingAmount)
    {
        currentHealth += healingAmount; // Increase current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't exceed max health
        UpdateHealthBar(); // Update health bar UI
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth; // Update the fill amount of the health bar
        }
    }

    private void Die()
    {
        //Debug.Log("Player has died!"); // Handle player death (e.g., restart level or show game over screen)
        // Add further logic here, such as disabling player controls, showing a game over screen, etc.


        //RESTART SCENE
        // Debug.Log("Player has died! Restarting level...");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);


        //Respawn at starting position
        Debug.Log("Player has died! Resetting position...");
        transform.position = startPosition;     // Reset the player's position to the starting position
        currentHealth = maxHealth;               // Restore health
        UpdateHealthBar();                       // Update health bar UI
    }
}