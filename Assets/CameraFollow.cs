using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public Transform crosshair;
    [Header("Camera Settings")]
    [Range(0f, 1f)] public float playerFocus = 0.7f;
    public float smoothSpeed = 10f;
    public PlayerController PC;

    private void LateUpdate()
    {
        if (player == null || crosshair == null) return;

        // Calculates a camera follow point, with a bias towards the player. This gets increased by 1.5x whilst the player is aiming
        float currentPlayerFocus = PC != null && PC.isAiming ? playerFocus * 1.5f : playerFocus;
        Vector3 targetPosition = Vector3.Lerp(player.position, crosshair.position, currentPlayerFocus);
        targetPosition.z = transform.position.z; // Maintain camera's z position

        // Smoothly interpolate the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}