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
    public WeaponsManager weaponsManager; // Reference to the WeaponsManager for handling shooting.

    [Header("Interaction Settings")]
    [SerializeField] float interactionRadius = 2f; // Radius within which the player can interact with objects.
    [SerializeField] Material highlightMaterial; // Material to apply when the player is in range.
    private Material originalMaterial; // Original material of the interactable object.
    private GameObject currentInteractable; // Reference to the current interactable object.

    // Called when the script instance is being loaded.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component.
    }

    // At the moment for debug purposes only
    private void Start()
    {
        UpdateWeaponsAndPlayerStats();
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
    }

    // Called when the player provides fire input.
    public void OnFire(InputValue cc)
    {
        isHoldingFire = cc.Get<float>() > 0; // Update firing status based on input.
        weaponsManager.Fire(isHoldingFire); // Trigger the firing mechanism in the WeaponsManager.
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

    // This function lets the player receive and process the picked attachment
    public void PickupAttachment(AttachmentsScrObj attachment)
    {
        WeaponsManager weaponsManager = GetComponent<WeaponsManager>(); // Get WeaponsManager from player
        if (weaponsManager != null)
        {
            weaponsManager.AddAttachment(attachment); // Call WeaponsManager to handle adding the attachment
            Debug.Log("Picked up attachment: " + attachment.name); // Confirmation message
        }
    }

    // Called at a fixed time interval, used for physics calculations.
    private void FixedUpdate()
    {
        // If not dashing, update the player's velocity based on movement input and aiming status.
        if (!isDashing)
        {
            rb.linearVelocity = moveInput * (isAiming ? moveSpeed * aimMoveSpeedMultiplier : moveSpeed);
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

    private void UpdateWeaponsAndPlayerStats()
    {
        moveSpeed = originalMoveSpeed;

        // Get the player's current weapon and update the player's stats based on the weapon's attachments.
        weaponsManager.WeaponCheck();
        moveSpeed += originalMoveSpeed * weaponsManager.addedPlayerSpeed / 100;
    }
}