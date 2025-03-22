using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float damage;
    private float radius;
    private float lifetime;
    public AudioClip explosionSound; // Add a reference to the impact sound
    private AudioSource audioSource; // Add a reference to the AudioSource component

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component
    }

    private void Start()
    {
        // Set the scale of the explosion based on the radius
        transform.localScale = new Vector3(radius, radius, 1f);

        // Destroy the explosion after the lifetime expires
        Destroy(gameObject, lifetime);

        // Play the impact sound
        if (audioSource != null && explosionSound != null)
        {
            audioSource.volume = 1f;
            audioSource.PlayOneShot(explosionSound);
        }
    }

    public void Initialize(float damage, float radius, float lifetime)
    {
        this.damage = damage;
        this.radius = radius;
        this.lifetime = lifetime;

        // Play the explosion animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("Explosion");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the explosion collided with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Get the Enemy component and apply damage
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {   
                enemy.TakeDamage(damage);
            }
        }
    }
}
