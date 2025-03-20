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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component
    }

    private void Start()
    {
        // Destroy the projectile after the lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the projectile collided with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Get the Enemy component and apply damage
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if(audioSource != null && enemyImpactSound != null)
                {
                    audioSource.volume = 0.3f;
                    audioSource.PlayOneShot(enemyImpactSound);
                }
            }
        }

        // Play the impact sound
        if (audioSource != null && impactSound != null && !collision.gameObject.CompareTag("Enemy"))
        {
            audioSource.volume = 0.1f;
            audioSource.PlayOneShot(impactSound);
        }

        // Disable the sprite and stop any movement
        spriteRenderer.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Destroy the object after 5 seconds
        Destroy(gameObject, 5f);
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
