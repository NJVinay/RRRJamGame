using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float lifetime = 30f; // Lifetime in seconds
    private SpriteRenderer spriteRenderer; 
    private Rigidbody2D rb; // Add a reference to the Rigidbody2D component
    public AudioClip impactSound; // Add a reference to the impact sound
    public AudioClip enemyImpactSound; // Add a reference to the impact sound
    private AudioSource audioSource; // Add a reference to the AudioSource component
    public bool penetratesEnemies; // Add a boolean to check if the projectile penetrates enemies
    private TrailRenderer trailRenderer; // Add a reference to the TrailRenderer component
    public bool enemyProjectile; // Add a boolean to check if the projectile should hit players instead of enemies
    private bool hasHit = false; // Add a boolean to check if the projectile has hit an enemy

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component
        trailRenderer = GetComponent<TrailRenderer>(); // Initialize the TrailRenderer component
    }

    private void Start()
    {
        // Destroy the projectile after the lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyProjectile)
        {
            // Check if the projectile collided with a player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Get the Player component and apply damage
                PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
                if (player != null && !hasHit)
                {
                    player.TakeDamage(damage);
                    hasHit = true;
                    if(audioSource != null && enemyImpactSound != null)
                    {
                        audioSource.volume = 1.0f;
                        audioSource.PlayOneShot(enemyImpactSound);
                    }
                }
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                // Ignore collision with enemies
                return;
            }
        }
        else
        {
            // Check if the projectile collided with an enemy
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // Get the Enemy component and apply damage
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null && !hasHit)
                {
                    enemy.TakeDamage(damage);
                    hasHit = true;
                    if(audioSource != null && enemyImpactSound != null)
                    {
                        audioSource.volume = 0.6f;
                        audioSource.PlayOneShot(enemyImpactSound);
                    }
                }
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                // Ignore collision with players
                return;
            }
        }

        // Play the impact sound
        if (audioSource != null && impactSound != null && !collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag("Player"))
        {
            audioSource.volume = 0.1f;
            audioSource.PlayOneShot(impactSound);
        }

        if (penetratesEnemies && collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        // Disable the sprite and stop any movement
        spriteRenderer.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Destroy the object after 5 seconds
        Destroy(gameObject, 0.3f);
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
        }
    }
}
