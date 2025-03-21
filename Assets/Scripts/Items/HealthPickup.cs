using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healingAmount = 10.0f; // Amount of health to heal


    public void Interact()
    {
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>(); // Find player in scene
        if (player != null)
        {
            player.Heal(healingAmount); // Call the Heal function in PlayerController
            Destroy(gameObject); // Destroy the health pickup object
        }
    }
}
