using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public Transform crosshair;

    [Header("Camera Settings")]
    [Range(0f, 1f)] public float playerFocus = 0.7f;
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (player == null || crosshair == null) return;

        // Calculate the target position closer to the player
        Vector3 targetPosition = Vector3.Lerp(player.position, crosshair.position, playerFocus);
        targetPosition.z = transform.position.z; // Maintain camera's z position

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
