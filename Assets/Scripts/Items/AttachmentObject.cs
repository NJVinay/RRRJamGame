using UnityEngine;

// This script should be attached to interactable objects (e.g., items on the ground)
public class AttachmentObject : MonoBehaviour
{
    public AttachmentsScrObj attachmentData; // Reference to attachment ScriptableObject

    // Method to handle interaction
    public void Interact()
    {   
        PlayerController player = FindFirstObjectByType<PlayerController>(); // Find player in scene
        if (player != null)
        {
            WeaponsManager weaponsManager = FindFirstObjectByType<WeaponsManager>(); // Get WeaponsManager from player
            if (weaponsManager != null)
            {
                weaponsManager.AddAttachment(attachmentData); // Call WeaponsManager to handle adding the attachment
                Destroy(gameObject); // Destroy this pickup object after picking up
            }
        }
    }
}
