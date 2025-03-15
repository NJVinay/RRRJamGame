using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    public float moveSpeed = 2.0f;           // Speed at which the enemy moves toward the player
    public float attackRange = 0.5f;          // Distance within which the enemy can attack the player
    public float damageAmount = 10.0f;        // Amount of damage dealt to the player
    public float detectionRange = 5.0f;       // Range within which the enemy can detect the player

    private Transform player;                 // Reference to the player's transform
    private bool isPlayerInRange = false;     // Flag to check if the player is in attack range

    private void Start()
    {
        // Find the player GameObject by its tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (IsPlayerInDetectionRange())
        {
            MoveTowardsPlayer();
            if (isPlayerInRange)
            {
                AttackPlayer();
            }
        }
    }

    private bool IsPlayerInDetectionRange()
    {
        // Determine the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= attackRange;

        // Return true if the player is within detection range
        return distanceToPlayer <= detectionRange;
    }

    private void MoveTowardsPlayer()
    {
        if (!isPlayerInRange)
        {
            // Move towards the player smoothly
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void AttackPlayer()
    {
        // Call the TakeDamage method on the player to deal damage
        player.GetComponent<PlayerHealth>().TakeDamage(damageAmount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true; // Set the flag when the player is in range
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object exiting the trigger is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // Reset the flag when the player is out of range
        }
    }
}