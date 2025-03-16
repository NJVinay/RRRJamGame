using UnityEngine;

// This script should be attached to the pickup object (e.g., Bayonet on ground)
public class AttachmentDetect : MonoBehaviour
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
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.nearbyAttachment = attachmentData; // Inform player which attachment is nearby
                Debug.Log("Attachment nearby: " + attachmentData.name);
            }
        }
    }

    // When the player leaves the area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.nearbyAttachment == attachmentData)
            {
                playerController.nearbyAttachment = null; // Clear the reference
                Debug.Log("Attachment out of range: " + attachmentData.name);
            }
        }
    }
}