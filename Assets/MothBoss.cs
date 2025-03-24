using UnityEngine;
using Pathfinding;
using UnityEngine.Audio;

public class MothBoss : MonoBehaviour
{
    public float idealDistanceAbovePlayer = 5f; // Ideal distance to stay above the player
    public float strafeDistance = 5f; // Distance to strafe left and right
    public float strafeSpeed = 2f; // Speed of strafing
    public int maxHealth = 1500; // Maximum health of the boss
    [SerializeField] float currentHealth; // Current health of the boss
    private Transform player;
    private AIPath aiPath;
    private Vector3 targetPosition;
    private float nextPathUpdateTime = 0f; // Track the next path update time
    private Animator animator;
    private float nextAttackTime = 0f; // Track the next attack time

    public GameObject projectilePrefab; // Reference to the projectile prefab
    public float projectileSpeed = 10f; // Speed of the projectile
    private AudioSource audioSource; // Reference to the AudioSource component
    public AudioResource attackSound; // Reference to the attack sound clip
    public Transform firePoint; // Reference to the fire point transform
    public int projectileCount = 5; // Number of projectiles to fire
    public float spreadAngle = 65f; // Total spread angle
    public float projectileDamage = 15f; // Damage of the projectile

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        aiPath = GetComponent<AIPath>();
        targetPosition = player.position + Vector3.up * idealDistanceAbovePlayer;
        currentHealth = maxHealth; // Initialize current health
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        firePoint = transform.Find("FirePoint");
    }

    void Update()
    {
        if (player != null && Time.time >= nextPathUpdateTime)
        {
            // Calculate the target position at the ideal distance above the player
            targetPosition = player.position + Vector3.up * idealDistanceAbovePlayer;

            // Strafe randomly left or right
            float randomDirection = Random.Range(-1f, 1f);
            targetPosition += Vector3.right * randomDirection * strafeDistance;

            // Ensure the target position is reachable by pathfinding
            GraphNode targetNode = AstarPath.active.GetNearest(targetPosition).node;
            if (!PathUtilities.IsPathPossible(AstarPath.active.GetNearest(transform.position).node, targetNode))
            {
                targetPosition = GetClosestReachableNodePosition(transform.position);
            }

            // Set the AIPath destination
            aiPath.destination = targetPosition;

            // Update the next path update time
            nextPathUpdateTime = Time.time + 0.25f;
        }

        // Check if it's time to attack
        if (Time.time >= nextAttackTime)
        {
            // Stop the AI movement
            aiPath.canMove = false;

            // Trigger the attack animation
            animator.SetTrigger("Attack");

            // Update the next attack time
            nextAttackTime = Time.time + 5f;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            //PlayPainSound(); // Play pain sound when taking damage
        }
    }

    private void Die()
    {
        // Handle the boss's death (e.g., play animation, destroy object)
        Destroy(gameObject);
    }

    private Vector3 GetClosestReachableNodePosition(Vector3 position)
    {
        GraphNode closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (var node in PathUtilities.GetReachableNodes(AstarPath.active.GetNearest(position).node))
        {
            float distance = Vector3.Distance((Vector3)node.position, position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return (Vector3)closestNode.position;
    }

    // Function to be called by the animator to fire the attack
    public void Fire()
    {
        if (firePoint == null)
        {
            Debug.LogError("FirePoint not found!");
            return;
        }

        // Play the attack sound
        if (audioSource != null && attackSound != null)
        {
            audioSource.resource = attackSound;
            audioSource.Play();
        }

        float angleStep = spreadAngle / (projectileCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < projectileCount; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 direction = (player.position - firePoint.position).normalized;
            float angle = startAngle + (angleStep * i);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, angle) * direction;
            rb.linearVelocity = spreadDirection * projectileSpeed;

            projectile.GetComponent<Projectile>().damage = projectileDamage;
            
            // Rotate the projectile to face the direction it is being shot at.
            float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
        }
    }

    // Function to be called by the animator to resume AI movement
    public void ResumeAI()
    {
        aiPath.canMove = true;
    }
}
