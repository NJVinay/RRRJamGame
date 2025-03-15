using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 10f;                // Speed of the fireball
    public float damageAmount = 10f;         // Damage dealt to the player
    public float lifespan = 3f;               // Time before the fireball is destroyed

    private void Start()
    {
        Destroy(gameObject, lifespan);       // Destroy the fireball after its lifespan
    }

    private void Update()
    {
        // Move the fireball in the current direction it is facing
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damageAmount); // Deal damage to the player
            Destroy(gameObject); // Destroy the fireball
        }
    }
}