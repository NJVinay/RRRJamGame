using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Add this using directive for scene management

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
    private TrailRenderer trailRenderer; // Reference to the TrailRenderer component

    // Header for organizing shooting-related settings in the Unity Inspector.
    [Header("Shooting Settings")]
    public bool isAiming = false; // Indicates if the player is currently aiming.
    public bool isHoldingFire = false; // Indicates if the fire button is being held down.
    public WeaponsManager weaponsManager; // Reference to the WeaponsManager for handling shooting.
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
    public bool hasBossKey = false; // Indicates if the player has the boss key.
    public int keys = 0; // Number of keys collected by the player.

    [Header("Crosshair Settings")]
    private Transform weaponObject;
    private Transform playerObject; // Reference to the player object
    private Transform crosshairObject; // Reference to the crosshair object
    [SerializeField] private GameObject sniperCrosshairObject; // Reference to the sniper crosshair object

    [Header("Audio Settings")]
    [SerializeField] AudioClip dashSound; // Sound effect for dashing.
    [SerializeField] AudioClip pickupSound; // Sound effect for interacting with a weapon/attachment.
    private AudioSource audioSource; // Reference to the AudioSource component.

    private bool isGamePaused = false; // Indicates if the game is currently paused.
    private PlayerInput playerInput; // Reference to the PlayerInput component.

    // Called when the script instance is being loaded.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Initialize the Rigidbody2D component.
        playerInput = GetComponent<PlayerInput>(); // Initialize the PlayerInput component.
        descriptionBox.SetActive(false); // Ensure the description box is initially disabled.
        playerObject = GameObject.FindWithTag("Player").transform;
        crosshairObject = GameObject.FindWithTag("Crosshair").transform;
        sniperCrosshairObject = GameObject.FindWithTag("SniperCrosshair");
        sniperCrosshairObject.SetActive(false); // Ensure the sniper crosshair is initially disabled.
        audioSource = GetComponent<AudioSource>(); // Initialize the AudioSource component.
        trailRenderer = GetComponent<TrailRenderer>(); // Initialize the TrailRenderer component.
        trailRenderer.emitting = false;
    }

    // private void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event.
    // }

    // private void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the sceneLoaded event.
    // }

    // Called when a new scene is loaded.
    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     if (scene.name != "MainMenu")
    //     {
    //         Time.timeScale = 1f; // Ensure the game is running.
    //         isGamePaused = false; // Reset the game paused flag.
    //         Cursor.visible = false; // Hide the cursor.
    //         Cursor.lockState = CursorLockMode.Confined; // Confine the cursor.
    //         rb.WakeUp(); // Ensure the Rigidbody2D is active.
    //         moveInput = Vector2.zero; // Reset movement input to ensure smooth resumption.
    //         UpdateWeaponsAndPlayerStats(); // Ensure moveSpeed is updated correctly.
    //         FetchManagers(); // Re-fetch managers to ensure all references are up-to-date.
    //         rb.linearVelocity = Vector2.zero; // Reset the player's velocity.
    //         playerInput.enabled = true; // Re-enable the PlayerInput component.
    //     }
    // }

    public void FetchManagers()
    {
        weaponsManager = FindFirstObjectByType<WeaponsManager>(); // Find the WeaponsManager component in the scene.
        if (weaponsManager != null)
        {
            weaponObject = weaponsManager?.transform; // Find the WeaponsManager component in the weapon object.
            underbarrel = weaponObject.GetComponentInChildren<Underbarrel>(); // Find the Underbarrel component in the child object of the player.
            Debug.Log("Ran fetch managers on player controller");
        }
        else
        {
            Debug.LogWarning("Weapon object not found. Ensure the weapon has the 'PlayerWeapon' tag.");
        }
    }

    // At the moment for debug purposes only
    private void Start()
    {
        UpdateWeaponsAndPlayerStats();

        // Hides the default cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        
        FetchManagers();
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
        if(weaponsManager != null && weaponsManager.enablesCrosshairZoom)
        {
            sniperCrosshairObject.SetActive(isAiming); // Enable/disable the sniper crosshair based on aiming status.
        }
        if(weaponsManager != null && weaponsManager.bipodBehaviour)
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
        if(weaponsManager != null)
        {
            weaponsManager.Fire(isHoldingFire); // Trigger the firing mechanism in the WeaponsManager.
        }
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
        if(weaponsManager != null)
        {
            weaponsManager.Reload();
        }
    }

    public void OnInteract (InputValue cc)
    {
        InteractWithObject();
    }

    // // Debug function to check the weapon status.
    // public void OnDebug01(InputValue cc)
    // {
    //     UpdateWeaponsAndPlayerStats();
    // }

    // // Called when the player presses the key to access the main menu.
    // public void OnMainMenu()
    // {
    //     ReturnToMainMenu();
    // }

    // Called at a fixed time interval, used for physics calculations.
    private void FixedUpdate()
    {
        // If not dashing, update the player's velocity based on movement input and aiming status.
        if (!isDashing)
        {
            if (isAiming && weaponsManager != null && weaponsManager.bipodBehaviour)
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
                if(hitCollider.GetComponent<AttachmentObject>() != null || hitCollider.GetComponent<WeaponObject>() != null)
                {
                    audioSource.PlayOneShot(pickupSound); // Play interact sound.
                }
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
                    AttachmentObject attachmentObject = currentInteractable.GetComponent<AttachmentObject>();
                    WeaponObject weaponObject = currentInteractable.GetComponent<WeaponObject>();
                    if (attachmentObject != null)
                    {
                        descriptionText.text = attachmentObject.attachmentData.description;
                        descriptionBox.SetActive(true);
                    }
                    else if (weaponObject != null)
                    {
                        descriptionText.text = weaponObject.weaponData.description;
                        descriptionBox.SetActive(true);
                    }
                }
                else
                {
                    // Update the description box if the same interactable object is still in range
                    AttachmentObject attachmentObject = currentInteractable.GetComponent<AttachmentObject>();
                    WeaponObject weaponObject = currentInteractable.GetComponent<WeaponObject>();
                    if (attachmentObject != null)
                    {
                        descriptionText.text = attachmentObject.attachmentData.description;
                    }
                    else if (weaponObject != null)
                    {
                        descriptionText.text = weaponObject.weaponData.description;
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
        //  if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     TogglePause();
        // }
        // If the fire button is being held, continue firing.
        if (isHoldingFire && weaponsManager != null)
        {
            weaponsManager.Fire(isHoldingFire);
        }

        DetectInteractableObjects(); // Detect nearby interactable objects

        // Check for the "M" key press to access the main menu
        // if (Keyboard.current.mKey.wasPressedThisFrame)
        // {
        //     if (isGamePaused)
        //     {
        //         ResumeGame(); // Resume the game if it is currently paused.
        //     }
        //     else
        //     {
        //         OnMainMenu(); // Access the main menu if the game is not paused.
        //     }
        // }

        // Update the crosshair position to follow the mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -9f; // Ensure the crosshair remains on the correct plane
        crosshairObject.position = mousePosition;

        // Rotate the weapon to face the crosshair
        Vector3 direction = mousePosition;
        if(weaponObject != null) direction = mousePosition - weaponObject.position;
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

        if(weaponObject != null) weaponObject.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }    

    // Method to toggle the pause state of the game.
    // private void TogglePause()
    // {
    //     if (isGamePaused)
    //     {
    //         ResumeGame();
    //     }
    //     else
    //     {
    //         Time.timeScale = 0f; // Pause the game.
    //         isGamePaused = true; // Set the game paused flag.
    //         Cursor.visible = true; // Show the cursor.
    //         Cursor.lockState = CursorLockMode.None; // Unlock the cursor.
    //     }
    // }
    
    // Coroutine to handle the dash mechanic.
    IEnumerator Dash()
    {
        isDashing = true; // Set dashing status to true.
        lastDashTime = Time.time; // Record the time the dash started.
        rb.linearVelocity = moveInput * dashSpeed; // Set the player's velocity to dash speed.
        audioSource.PlayOneShot(dashSound); // Play dash sound.
        trailRenderer.emitting = true; // Enable the TrailRenderer
        yield return new WaitForSeconds(dashTime); // Wait for the dash duration.
        isDashing = false; // Reset dashing status.
        trailRenderer.emitting = false; // Disable the TrailRenderer
    }

    public void UpdateWeaponsAndPlayerStats()
    {
        moveSpeed = originalMoveSpeed;
        if(weaponsManager != null)
        {
            moveSpeed += originalMoveSpeed * weaponsManager.addedPlayerSpeed / 100;
        }
    }

    // // Called when the player wants to return to the main menu.
    // public void ReturnToMainMenu()
    // {
    //     Time.timeScale = 0f; // Pause the game.
    //     isGamePaused = true; // Set the game paused flag.
    //     Cursor.visible = true; // Show the cursor.
    //     Cursor.lockState = CursorLockMode.None; // Unlock the cursor.
    //     SceneManager.LoadScene("MainMenu"); // Load the main menu scene.
    // }

    // // Called when the player wants to resume the game.
    // public void ResumeGame()
    // {
    //     Time.timeScale = 1f; // Resume the game.
    //     isGamePaused = false; // Reset the game paused flag.
    //     Cursor.visible = false; // Hide the cursor.
    //     Cursor.lockState = CursorLockMode.Confined; // Confine the cursor.
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the current level.
    // }

    // // Called when the player wants to quit the game.
    // public void QuitGame()
    // {
    //     Application.Quit(); // Quit the application.
    //     #if UNITY_EDITOR
    //     UnityEditor.EditorApplication.isPlaying = false; // Stop playing the game in the editor.
    //     #endif
    // }
}