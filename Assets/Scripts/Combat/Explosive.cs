using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float damage;
    public float lifetime = 30f; // Lifetime in seconds
    private SpriteRenderer spriteRenderer; 
    private Rigidbody2D rb; // Add a reference to the Rigidbody2D component
    public GameObject explosionPrefab; // Add a reference to the explosion prefab
    public float explosionRadius; // Add a reference to the explosion radius
    public float explosionLifetime; // Add a reference to the explosion lifetime
    public GameObject explosionLocation; // Add a reference to the explosion location object
    public bool explosiveRound; // Adds a boolean to check if the round is from Explosive Rounds instead of a normal explosive 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component
    }

    private void Start()
    {
        // Destroy the explosive after the lifetime expires
        Destroy(gameObject, lifetime);

        if (explosiveRound)
        {
            explosionRadius =  0.5f;
            explosionLifetime = 0.5f;
            transform.localScale *= 0.5f; // Halve the scale if explosiveRound is true
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Instantiate the explosion prefab at the explosionLocation's position
        if (explosionPrefab != null && explosionLocation != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, explosionLocation.transform.position, Quaternion.identity);
            Explosion explosionScript = explosion.GetComponent<Explosion>();
            if (explosionScript != null)
            {
                explosionScript.Initialize(damage, explosionRadius, explosionLifetime);
            }
            Destroy(explosion, explosionLifetime);
        }

        // Disable the sprite and stop any movement
        spriteRenderer.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Destroy(gameObject);
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
