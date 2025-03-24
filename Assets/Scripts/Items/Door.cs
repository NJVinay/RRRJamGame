using UnityEngine;

public class Door : MonoBehaviour
{
    public bool locked = false;
    public bool bossLocked = false;
    private Collider2D doorCollider;
    private SpriteRenderer doorSpriteRenderer;
    private PlayerController player;
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite; 
    private AudioSource audioSource; // Reference to the AudioSource component.
    public AudioClip doorOpenSound; // Sound effect for opening the door.
    public AudioClip doorLockedSound; // Sound effect for closing the door.

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        doorSpriteRenderer = GetComponent<SpriteRenderer>();
        player = FindFirstObjectByType<PlayerController>();
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component.
    }

    public void Interact()
    {
        if (!locked && !bossLocked)
        {
            // Disable collision
            doorCollider.enabled = false;

            // Swap sprites
            doorSpriteRenderer.sprite = openDoorSprite;

            // Play the door open sound effect
            audioSource.PlayOneShot(doorOpenSound);
        }

        else if(locked && PlayerData.Instance.keys > 0)
        {
            PlayerData.Instance.keys -= 1;

            // Disable collision
            doorCollider.enabled = false;

            // Swap sprites
            doorSpriteRenderer.sprite = openDoorSprite;

            // Play the door open sound effect
            audioSource.PlayOneShot(doorOpenSound);
        }

        else if(bossLocked && PlayerData.Instance.hasBossKey)
        {
            PlayerData.Instance.hasBossKey = false;

            // Disable collision
            doorCollider.enabled = false;

            // Swap sprites
            doorSpriteRenderer.sprite = openDoorSprite;

            // Play the door open sound effect
            audioSource.PlayOneShot(doorOpenSound);
        }
        else
        {
            // Play the door locked sound effect
            audioSource.PlayOneShot(doorLockedSound);
        }
    }
}
