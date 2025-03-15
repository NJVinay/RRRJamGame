using UnityEngine;

public class WandAim : MonoBehaviour
{
    public Transform player; // Drag your player GameObject here in the inspector

    void Update()
    {
        if (player != null)
        {
            // Calculate the direction to the player
            Vector2 direction = (player.position - transform.position).normalized;

            // Calculate the angle in degrees
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set the rotation of the wand
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }
}