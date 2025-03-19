using UnityEngine;

// This script should be attached to interactable objects (e.g., items on the ground)
public class AttachmentObject : MonoBehaviour
{
    public AttachmentsScrObj attachmentData; // Reference to attachment ScriptableObject

    private void Start()
    {
        // Change the sprite to the one set in the AttachmentScrObj
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && attachmentData != null)
        {
            spriteRenderer.sprite = attachmentData.Sprite;
        }
    }

    // Method to handle interaction
    public void Interact()
    {   
        PlayerController player = FindFirstObjectByType<PlayerController>(); // Find player in scene
        if (player != null)
        {
            WeaponsManager weaponsManager = FindFirstObjectByType<WeaponsManager>(); // Get WeaponsManager from player
            if (weaponsManager != null)
            {
                weaponsManager.AddAttachment(attachmentData, this); // Call WeaponsManager to handle adding the attachment
            }
        }
    }
}
