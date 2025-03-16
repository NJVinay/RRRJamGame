using UnityEngine;

// This script should be attached to the pickup object (e.g., Bayonet on ground)
public class AttachmentPickup : MonoBehaviour
{
    public AttachmentsScrObj attachmentData; // Reference to attachment ScriptableObject
    private bool playerInRange = false; // To check if player is near

    private void Update()
    {
        // If player is in range and presses 'E', pick up the attachment
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PlayerController player = FindObjectOfType<PlayerController>(); // Find player in scene
            if (player != null)
            {
                player.PickupAttachment(attachmentData); // Call PlayerController's pickup method
                Destroy(gameObject); // Destroy this pickup object after picking up
            }
        }
    }

    // Check if player enters pickup range
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // Check if player leaves pickup range
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
