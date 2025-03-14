using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float lifetime = 10f; // Lifetime in seconds
    private SpriteRenderer spriteRenderer; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Destroy the projectile after the lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject); // Destroy immediately on collision
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
