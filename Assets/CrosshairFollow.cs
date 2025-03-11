using UnityEngine;

public class CrosshairFollow : MonoBehaviour
{
    private void Start()
    {
        // Hides the default cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        // Update the crosshair position to follow the mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -9f; // Ensure the crosshair remains on the correct plane
        transform.position = mousePosition;
    }
}
