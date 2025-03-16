using UnityEngine;

public class FireEnemy : MonoBehaviour
{
    public GameObject fireballPrefab;       // Reference to the fireball prefab
    public float fireRate = 2f;             // Time between fireball shots
    public float detectionRange = 5f;       // Range to detect the player
    public float attackRange = 2f;          // Distance to attack the player

    private Transform player;               // Reference to the player's transform
    private float nextFireTime = 0f;       // Timer for firing projectiles

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find player by tag
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position); // Distance to player

        if (distanceToPlayer <= detectionRange) // Check if player is within detection range
        {
            if (distanceToPlayer <= attackRange && Time.time >= nextFireTime) // Check if within attack range
            {
                FireFireball(); // Fire a fireball
                nextFireTime = Time.time + fireRate; // Reset next fire time
            }
        }
    }

    private void FireFireball()
    {
        // Instantiate the fireball at FireEnemy's position with no rotation
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);

        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Set the fireball's direction and rotate it to face the player
        fireball.transform.up = direction; // Set the fireball's "up" direction to point towards the player

        Debug.Log("Firing fireball towards: " + direction);
    }
}