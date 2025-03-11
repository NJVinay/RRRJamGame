using UnityEngine;

public class CrosshairFollow : MonoBehaviour
{
    public Transform weaponObject;
    public Transform playerObject; // Reference to the player object

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

        // Rotate the specified object to face the crosshair
        Vector3 direction = mousePosition - weaponObject.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the angle if the player is facing left
        if (mousePosition.x < playerObject.position.x)
        {
            playerObject.localScale = new Vector3(-1, 1, 1); // Flip the player to face left
            angle += 180f; // Adjust the angle by 180 degrees
        }
        else
        {
            playerObject.localScale = new Vector3(1, 1, 1); // Flip the player to face right
        }

        weaponObject.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
