using UnityEngine;

public class BayonetCollision : MonoBehaviour
{
    private WeaponsManager weaponsManager;
    private float lastCollisionDamageTime; // Tracks the last time collision damage was applied.
    public AudioClip impactSound; // Sound to play on impact.
    private AudioSource audioSource; // Audio source to play the sound.

    private void Awake()
    {
        weaponsManager = GetComponentInParent<WeaponsManager>();
        lastCollisionDamageTime = -0.5f; // Initialize last collision damage time.
        audioSource = GetComponent<AudioSource>(); // Initialize the audio source.
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (weaponsManager != null && weaponsManager.damagesEnemiesByCollision && Time.time >= lastCollisionDamageTime + 0.5f)
        {
            // Check if the collided object has an Enemy component.
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(40);
                
                lastCollisionDamageTime = Time.time; // Update the last collision damage time.
                
                // Play the impact sound.
                if (impactSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(impactSound);
                }
            }
        }
    }
}
