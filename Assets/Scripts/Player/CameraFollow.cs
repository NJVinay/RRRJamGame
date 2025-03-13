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

    private float currentPlayerFocus;

    private void LateUpdate()
    {
        if (player == null || crosshair == null) return;

        // Smoothly interpolate the playerFocus value
        float targetPlayerFocus = PC != null && PC.isAiming ? playerFocus * 1.5f : playerFocus;
        currentPlayerFocus = Mathf.Lerp(currentPlayerFocus, targetPlayerFocus, Time.deltaTime * smoothSpeed);

        // Calculates a camera follow point, with a bias towards the player
        Vector3 targetPosition = Vector3.Lerp(player.position, crosshair.position, currentPlayerFocus);
        targetPosition.z = transform.position.z; // Maintain camera's z position

        // Directly set the camera to the target position
        transform.position = targetPosition;
    }
}