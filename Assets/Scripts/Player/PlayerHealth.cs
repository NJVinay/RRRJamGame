using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public Image healthBar;      // Reference to the health bar UI element
    public float maxHealth = 100.0f; // Maximum health of the player
    private Vector3 startPosition;
    private WeaponsManager weaponsManager; // Reference to the WeaponsManager for handling damage reduction
    [SerializeField] AudioClip damageSound; // Sound effect for taking damage.
    [SerializeField] AudioClip deathSound; // Sound effect for taking damage.
    AudioSource audioSource; // Reference to the AudioSource component.

    private void Awake()
    {
        PlayerData.Instance.health = maxHealth; // Set current health to maximum at start
        UpdateHealthBar(); // Initialize health bar
        startPosition = transform.position; // Store the initial position of the player
        weaponsManager = FindFirstObjectByType<WeaponsManager>(); // Initialize the WeaponsManager reference
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component.
    }

    private void Start()
    {
        // Initialize PlayerData health if needed
        if (PlayerData.Instance.health <= 0)
        {
            PlayerData.Instance.health = maxHealth;
        }
        weaponsManager = FindFirstObjectByType<WeaponsManager>(); // Initialize WeaponsManager
        UpdateHealthBar(); // Initialize health bar
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
        float finalDamage = damageAmount; // Initialize reduced damage variable
        if (weaponsManager != null)
        {
            float damageReduction = weaponsManager.subtractedBlockedDamage / 100.0f; // Calculate damage reduction percentage
            finalDamage = damageAmount * (1 - damageReduction);
        }

        PlayerData.Instance.health -= finalDamage; // Decrease current health
        PlayerData.Instance.health = Mathf.Clamp(PlayerData.Instance.health, 0, maxHealth); // Ensure health doesn't drop below 0
        UpdateHealthBar(); // Update health bar UI

        if (PlayerData.Instance.health <= 0)
        {
            audioSource.PlayOneShot(deathSound); // Play the damage sound effect.
            Die(); // Call die function if player is dead
        }
        else
        {
            audioSource.PlayOneShot(damageSound); // Play the damage sound effect.
        }
    }

    public void Heal(float healingAmount)
    {
        PlayerData.Instance.health += healingAmount; // Increase current health
        PlayerData.Instance.health = Mathf.Clamp(PlayerData.Instance.health, 0, maxHealth); // Ensure health doesn't exceed max health
        UpdateHealthBar(); // Update health bar UI
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = PlayerData.Instance.health / maxHealth; // Update the fill amount of the health bar
        }
    }

    private void Die()
    {
        Debug.Log("Player has died! Resetting position...");
        transform.position = startPosition;     // Reset the player's position to the starting position
        PlayerData.Instance.health = maxHealth;               // Restore health
        UpdateHealthBar();                       // Update health bar UI
    }
}