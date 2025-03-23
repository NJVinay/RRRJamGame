using UnityEngine;

public class slashAttack : MonoBehaviour
{
    private Animator animator;
    private float animationLength;
    public float slashDamage = 10;
    public AudioClip hitSound; // Add a public variable for the hit sound
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
        if (animator != null)
        {
            animator.Play("Attack");
            // Get the length of the current animation clip
            animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            Destroy(gameObject, animationLength);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(slashDamage);
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound); // Play the hit sound
                }
            }
        }
    }
}
