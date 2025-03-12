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

    [Header("Other Settings")]
    [SerializeField] float originalBlockedDamage; 
    private float blockedDamage = 0; 

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
            rb.linearVelocity = moveInput * (isAiming ? moveSpeed * aimMoveSpeedMultiplier : moveSpeed);
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
        blockedDamage = originalBlockedDamage;

        // Get the player's current weapon and update the player's stats based on the weapon's attachments.
        weaponsManager.WeaponCheck();
        moveSpeed += originalMoveSpeed *weaponsManager.addedPlayerSpeed /100;
        blockedDamage += originalBlockedDamage *weaponsManager.subtractedBlockedDamage /100;
    }
}