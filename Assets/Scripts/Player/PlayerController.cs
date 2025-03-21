using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// This class controls the player's movement and shooting mechanics.
public class PlayerController : MonoBehaviour
{
    // Header for organizing movement-related settings in the Unity Inspector.
    [Header("Movement Settings")]
    [SerializeField] float originalMoveSpeed = 5f; // Normal movement speed of the player.
    [SerializeField] float aimMoveSpeedMultiplier = 0.5f; // Speed multiplier when aiming.
    [SerializeField] float dashSpeed = 25f; // Speed during a dash.
    [SerializeField] float dashTime = 0.1f; // Duration of the dash.
    [SerializeField] float dashCooldown = 2f; // Cooldown period between dashes.
    private float lastDashTime; // Tracks the last time a dash was performed.
    private Vector2 moveInput; // Stores the player's movement input.
    private float moveSpeed; // Current movement speed of the player.
    private Rigidbody2D rb; // Reference to the player's Rigidbody2D component.
    private bool isDashing = false; // Indicates if the player is currently dashing.

    // Header for organizing shooting-related settings in the Unity Inspector.
    [Header("Shooting Settings")]
    public bool isAiming = false; // Indicates if the player is currently aiming.
    public bool isHoldingFire = false; // Indicates if the fire button is being held down.
    private WeaponsManager weaponsManager; // Reference to the WeaponsManager for handling shooting.
    public Transform WeaponPosition; // Reference to the WeaponPosition transform.
    private Underbarrel underbarrel;
    bool bipodActive = false;

    [Header("Interaction Settings")]
    [SerializeField] float interactionRadius = 2f; // Radius within which the player can interact with objects.
    [SerializeField] Material highlightMaterial; // Material to apply when the player is in range.
    private Material originalMaterial; // Original material of the interactable object.
    private GameObject currentInteractable; // Reference to the current interactable object.
    [SerializeField] GameObject descriptionBox; // Reference to the description box GameObject.
    [SerializeField] TMPro.TextMeshProUGUI descriptionText; // Reference to the TextMeshProUGUI component for the description text.

    [Header("Crosshair Settings")]
    private Transform weaponObject;
    private Transform playerObject; // Reference to the player object
    private Transform crosshairObject; // Reference to the crosshair object
    [SerializeField] private GameObject sniperCrosshairObject; // Reference to the sniper crosshair object

    // Called when the script instance is being loaded.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component.
        weaponsManager = WeaponPosition.GetComponentInChildren<WeaponsManager>(); // Find the WeaponsManager component in the child object of WeaponPosition.
        descriptionBox.SetActive(false); // Ensure the description box is initially disabled.

        weaponObject = GameObject.FindWithTag("PlayerWeapon").transform;
        playerObject = GameObject.FindWithTag("Player").transform;
        crosshairObject = GameObject.FindWithTag("Crosshair").transform;
        sniperCrosshairObject = GameObject.FindWithTag("SniperCrosshair");
        sniperCrosshairObject.SetActive(false); // Ensure the sniper crosshair is initially disabled.
        
        underbarrel = weaponObject.GetComponentInChildren<Underbarrel>(); // Find the Underbarrel component in the child object of the player.
    }

    // At the moment for debug purposes only
    private void Start()
    {
        UpdateWeaponsAndPlayerStats();

        // Hides the default cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Called when the player provides movement input.
    public void OnMove(InputValue cc)
    {
        moveInput = cc.Get<Vector2>(); // Update the movement input based on player input.
    }

    // Called when the player attempts to dash.
    public void OnDash(InputValue cc)
    {
        // Check if the player can dash (not already dashing, not aiming, and cooldown has passed).
        if (!isDashing && !isAiming && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash()); // Start the dash coroutine.
        }
    }

    // Called when the player provides aiming input.
    public void OnAim(InputValue cc)
    {
        isAiming = cc.Get<float>() > 0; // Update aiming status based on input.
        if(weaponsManager.enablesCrosshairZoom)
        {
            sniperCrosshairObject.SetActive(isAiming); // Enable/disable the sniper crosshair based on aiming status.
        }
        if(weaponsManager.bipodBehaviour)
        {
            if(isAiming)
            {
                if(!bipodActive)
                {
                    weaponsManager.currentSpread *= 0.2f; // Reduce spread by 80%
                    weaponsManager.currentFireRate *= 1.5f; // Increase fire rate by 50%
                    bipodActive = true;
                }
            }
            else
            {
                if(bipodActive)
                {
                    weaponsManager.currentSpread /= 0.2f; // Reset spread to original value
                    weaponsManager.currentFireRate /= 1.5f; // Reset fire rate to original value
                    bipodActive = false;
                }
            }
        }
    }

    // Called when the player provides fire input.
    public void OnFire(InputValue cc)
    {
        isHoldingFire = cc.Get<float>() > 0; // Update firing status based on input.
        weaponsManager.Fire(isHoldingFire); // Trigger the firing mechanism in the WeaponsManager.
    }

    public void OnUnderbarrel(InputValue cc)
    {
        if (underbarrel != null)
        {
            underbarrel.Fire();
        }
    }

    // Called when the player provides reload input.
    public void OnReload(InputValue cc)
    {
        weaponsManager.Reload();
    }

    public void OnInteract (InputValue cc)
    {
        InteractWithObject();
    }

    // Debug function to check the weapon status.
    public void OnDebug01(InputValue cc)
    {
        UpdateWeaponsAndPlayerStats();
    }

    // Called at a fixed time interval, used for physics calculations.
    private void FixedUpdate()
    {
        // If not dashing, update the player's velocity based on movement input and aiming status.
        if (!isDashing)
        {
            if (isAiming && weaponsManager.bipodBehaviour)
            {
                rb.linearVelocity = Vector2.zero; // Set movement speed to 0 when aiming with bipod
            }
            else
            {
                rb.linearVelocity = moveInput * (isAiming ? moveSpeed * aimMoveSpeedMultiplier : moveSpeed);
            }
        }
    }

    // Method to call when pressing "E" to interact with objects tagged as "Interactable"
    private void InteractWithObject()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Interactable"))
            {
                hitCollider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Interacted with: " + hitCollider.name);
                break;
            }
        }
    }

    // Method to detect nearby interactable objects and change their materials
    private void DetectInteractableObjects()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Interactable"))
            {
                if (currentInteractable != hitCollider.gameObject)
                {
                    if (currentInteractable != null)
                    {
                        // Revert material to original material
                        Renderer renderer = currentInteractable.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = originalMaterial;
                        }
                    }

                    currentInteractable = hitCollider.gameObject;
                    Renderer newRenderer = currentInteractable.GetComponent<Renderer>();
                    if (newRenderer != null)
                    {
                        originalMaterial = newRenderer.material;
                        newRenderer.material = highlightMaterial;
                    }

                    // Update and show the description box
                    AttachmentsScrObj attachment = currentInteractable.GetComponent<AttachmentObject>().attachmentData;
                    if (attachment != null)
                    {
                        descriptionText.text = attachment.description;
                        descriptionBox.SetActive(true);
                    }
                }
                else
                {
                    // Update the description box if the same interactable object is still in range
                    AttachmentsScrObj attachment = currentInteractable.GetComponent<AttachmentObject>().attachmentData;
                    if (attachment != null)
                    {
                        descriptionText.text = attachment.description;
                    }
                }
                return;
            }
        }

        // Revert material if no interactable object is nearby
        if (currentInteractable != null)
        {
            Renderer renderer = currentInteractable.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }
            currentInteractable = null;
            descriptionBox.SetActive(false); // Hide the description box
        }
    }

    // Called once per frame, used for regular updates.
    private void Update()
    {
        // If the fire button is being held, continue firing.
        if (isHoldingFire)
        {
            weaponsManager.Fire(isHoldingFire);
        }

        DetectInteractableObjects(); // Detect nearby interactable objects

        // Update the crosshair position to follow the mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -9f; // Ensure the crosshair remains on the correct plane
        crosshairObject.position = mousePosition;

        // Rotate the weapon to face the crosshair
        Vector3 direction = mousePosition - weaponObject.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the angle if the player is facing left
        if (mousePosition.x < playerObject.position.x)
        {
            playerObject.localScale = new Vector3(-1, 1, 1); // Flip the player to face left
            descriptionBox.transform.localScale = new Vector3(-1, 1, 1); // Flip the description box the other way to ensure text is not reversed
            angle += 180f; // Adjust the angle by 180 degrees
        }
        else
        {
            playerObject.localScale = new Vector3(1, 1, 1); // Flip the player to face right
            descriptionBox.transform.localScale = new Vector3(1, 1, 1); // Flip the description box the other way to ensure text is not reversed
        }

        weaponObject.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    // Coroutine to handle the dash mechanic.
    IEnumerator Dash()
    {
        isDashing = true; // Set dashing status to true.
        lastDashTime = Time.time; // Record the time the dash started.
        rb.linearVelocity = moveInput * dashSpeed; // Set the player's velocity to dash speed.
        yield return new WaitForSeconds(dashTime); // Wait for the dash duration.
        isDashing = false; // Reset dashing status.
    }

    public void UpdateWeaponsAndPlayerStats()
    {
        moveSpeed = originalMoveSpeed;
        moveSpeed += originalMoveSpeed * weaponsManager.addedPlayerSpeed / 100;
    }
}