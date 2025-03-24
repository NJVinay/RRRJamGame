using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

// This class manages the player's weapon, including its attachments and firing mechanics.
public class WeaponsManager : MonoBehaviour
{
    public AudioManager audioManager;
    public WeaponsScrObj currentWeapon; // The current weapon being used by the player.

    // Array to hold the current attachments for the weapon.
    // Each index corresponds to a specific attachment category:
    // 0 - BarrelSlot, 
    // 1 - Sight, 
    // 2 - Underbarrel, 
    // 3 - Magazine, 
    // 4 - Misc
    public AttachmentsScrObj[] currentAttachments = new AttachmentsScrObj[5];

    // Variables to store the current weapon's properties.
    public FireMode currentFireMode { get; private set; }
    public float currentDamage { get; private set; }
    public float currentFireRate { get; set; }
    public float currentReloadTime { get; private set; }
    public int currentMagazineSize { get; private set; }
    public float currentSpread { get; set; }
    public int currentProjectileCount { get; private set; }
    public float currentSpeed { get; private set; }
    public int currentAmmo { get; private set; } // Current ammo in the magazine.

    // Variables affecting player
    public float addedPlayerSpeed { get; private set; }
    public float subtractedBlockedDamage { get; private set; }

    // Variables to store attachment effects.
    public bool damagesEnemiesByCollision { get; private set; }
    public bool enablesCrosshairZoom { get; private set; }
    public bool highlightsEnemies { get; private set; }
    public bool enablesGrenades { get; private set; }
    public bool enablesSpreadShot { get; private set; }
    public bool bipodBehaviour { get; private set; }
    public bool explosiveRounds { get; private set; }
    public bool penetratesEnemies { get; private set; }

    [SerializeField] GameObject currentProjectilePrefab; // Prefab for the projectile to be fired.
    [SerializeField] GameObject BulletPrefab; // Default bullet projectile prefab.
    [SerializeField] GameObject RocketPrefab; // Default rocket projectile prefab.
    [SerializeField] Transform firePoint; // The point from which projectiles are fired.
    [SerializeField] private GameObject barrelLocation; // Reference to the BarrelLocation object.
    [SerializeField] private GameObject sightLocation; // Reference to the SightLocation object.
    [SerializeField] private GameObject underbarrelLocation; // Reference to the UnderbarrelLocation object.
    [SerializeField] private GameObject magazineLocation; // Reference to the MagazineLocation object.
    [SerializeField] private GameObject miscLocation; // Reference to the MiscLocation object.
    private float lastFireTime; // Tracks the last time a shot was fired.
    private float lastAudioPlayTime; // Tracks the last time an audio clip was played.

    private AudioSource audioSource;
    private ReloadManager reloadManager;
    private PlayerController playerController;

    private Color currentBulletColor = Color.yellow; // Default bullet color.
    private Camera mainCamera; // Reference to the main camera.
    private CameraFollow cameraFollow; // Reference to the CameraFollow script.
    private float shakeDuration = 0.03f; // Duration of the camera shake.
    private float shakeMagnitude = 0.06f; // Magnitude of the camera shake.

    private void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        reloadManager = GetComponent<ReloadManager>();
        audioManager = FindFirstObjectByType<AudioManager>(); // Fetch AudioManager
        currentAmmo = currentMagazineSize; // Initialize current ammo.
        lastAudioPlayTime = -0.1f; // Initialize last audio play time.
        mainCamera = Camera.main; // Fetch the main camera.
        cameraFollow = mainCamera.GetComponent<CameraFollow>(); // Fetch the CameraFollow script.
        Initialize(); // Call the new Initialize method
    }

    // Method to initialize the WeaponsManager
    public void Initialize()
    {
        if(playerController != null) playerController.FetchManagers();
        if(audioManager != null) audioManager.FetchManagers();
    }

    public void Start()
    {
        WeaponCheck(); // Log the current weapon and its attachments.
        if (PlayerData.Instance.selectedWeapon != null)
        {
            currentWeapon = PlayerData.Instance.selectedWeapon;
            RefreshAttachments(); // Refresh all stats based on attachments
        }
    }
    // Method to check and log the current weapon and its attachments.
    public void WeaponCheck()
    {
        // Refresh all stats based on attachments
        RefreshAttachments();

        // Log current weapon and its attachments for debugging
        Debug.Log("----- Weapon Status -----");
        Debug.Log("Weapon: " + currentWeapon.WeaponName);
        Debug.Log("Barrel Attachment: " + (currentAttachments[0] != null ? currentAttachments[0].attachmentName : "None"));
        Debug.Log("Sight Attachment: " + (currentAttachments[1] != null ? currentAttachments[1].attachmentName : "None"));
        Debug.Log("Underbarrel Attachment: " + (currentAttachments[2] != null ? currentAttachments[2].attachmentName : "None"));
        Debug.Log("Magazine Attachment: " + (currentAttachments[3] != null ? currentAttachments[3].attachmentName : "None"));
        Debug.Log("Misc Attachment: " + (currentAttachments[4] != null ? currentAttachments[4].attachmentName : "None"));

        // Log weapon stats
        Debug.Log("Damage: " + currentDamage);
        Debug.Log("Fire Rate: " + currentFireRate);
        Debug.Log("Reload Time: " + currentReloadTime);
        Debug.Log("Magazine Size: " + currentMagazineSize);
        Debug.Log("Spread: " + currentSpread);
        Debug.Log("Projectile Count: " + currentProjectileCount);
        Debug.Log("Projectile Speed: " + currentSpeed);
        Debug.Log("Fire Mode: " + currentFireMode);

        // Log player effects
        Debug.Log("Player Speed Bonus: " + addedPlayerSpeed);
        Debug.Log("Blocked Damage Reduction: " + subtractedBlockedDamage);

        // Log attachment effects (booleans)
        Debug.Log("Damages Enemies By Collision: " + damagesEnemiesByCollision);
        Debug.Log("Enables Crosshair Zoom: " + enablesCrosshairZoom);
        Debug.Log("Highlights Enemies: " + highlightsEnemies);
        Debug.Log("Enables Grenades: " + enablesGrenades);
        Debug.Log("Enables Spread Shot: " + enablesSpreadShot);
        Debug.Log("Bipod Behaviour: " + bipodBehaviour);
        Debug.Log("Explosive Rounds: " + explosiveRounds);
        Debug.Log("Penetrates Enemies: " + penetratesEnemies);
        Debug.Log("--------------------------");
    }


    // Method to refresh and update weapon stats based on current attachments.
    public void RefreshAttachments()
    {
        // Initialize weapon stats from the base weapon.
        currentDamage = currentWeapon.Damage;
        currentFireRate = currentWeapon.FireRate;
        currentReloadTime = currentWeapon.ReloadTime;
        currentMagazineSize = currentWeapon.MagazineSize;
        currentSpread = currentWeapon.Spread;
        currentProjectileCount = currentWeapon.BulletCount;
        currentSpeed = currentWeapon.BulletSpeed;
        currentFireMode = currentWeapon.FireMode;

        //Player stats
        addedPlayerSpeed = 0;
        subtractedBlockedDamage = 0;

        // Disable every attachment effect
        damagesEnemiesByCollision = false;
        enablesCrosshairZoom = false;
        highlightsEnemies = false;
        enablesGrenades = false;
        enablesSpreadShot = false;
        bipodBehaviour = false;
        explosiveRounds = false;
        penetratesEnemies = false;

        // Reset bullet color to default.
        currentBulletColor = Color.yellow;

        // Iterate over each attachment and modify weapon stats accordingly.
        for (int i = 0; i < currentAttachments.Length; i++)
        {
            if (currentAttachments[i] != null)
            {
                // Apply attachment modifiers
                currentDamage += currentWeapon.Damage * currentAttachments[i].AddedDamage / 100;
                currentFireRate += currentWeapon.FireRate * currentAttachments[i].AddedFireRate / 100;
                currentReloadTime += currentWeapon.ReloadTime * currentAttachments[i].AddedReloadTime / 100;
                currentMagazineSize += (int)(currentWeapon.MagazineSize * currentAttachments[i].AddedMagazineSize / 100);
                currentSpread += currentWeapon.Spread * currentAttachments[i].AddedSpread / 100;
                currentSpeed += currentWeapon.BulletSpeed * currentAttachments[i].AddedSpeed / 100;

                //Apply player modifiers
                addedPlayerSpeed += currentAttachments[i].AddedPlayerSpeed;
                subtractedBlockedDamage += currentAttachments[i].SubtractedBlockedDamage;

                // Apply attachment effects
                // If multiple attachments have the same effect (they shouldn't), the effect is enabled
                damagesEnemiesByCollision |= currentAttachments[i].DamagesEnemiesByCollision;
                enablesCrosshairZoom |= currentAttachments[i].EnablesCrosshairZoom;
                highlightsEnemies |= currentAttachments[i].HighlightsEnemies;
                enablesGrenades |= currentAttachments[i].EnablesGrenades;
                enablesSpreadShot |= currentAttachments[i].EnablesSpreadShot;
                bipodBehaviour |= currentAttachments[i].BipodBehaviour;
                explosiveRounds |= currentAttachments[i].ExplosiveRounds;
                penetratesEnemies |= currentAttachments[i].PenetratesEnemies;

                // Update fire mode if the attachment changes it
                currentFireMode = currentAttachments[i].ChangesFireMode ? currentAttachments[i].FireMode : currentFireMode;

                // Change bullet color if the attachment changes it
                if (currentAttachments[i].ChangesColor)
                {
                    currentBulletColor = currentAttachments[i].BulletColor;
                }

                if (currentAttachments[i].ChangesProjectileCount)
                {
                    currentProjectileCount = (int)(currentWeapon.BulletCount * currentAttachments[i].BulletCountMultiplier);
                }
            }
        }

        // Update playerFocus on CameraFollow based on enablesCrosshairZoom
        if (cameraFollow != null)
        {
            cameraFollow.UpdatePlayerFocus(enablesCrosshairZoom ? 0.28f : 0.2f);
        }

        UpdateEnemyMaterial();
        audioManager.UpdateDynamicMusic();
        playerController.UpdateWeaponsAndPlayerStats();
    }

    // Method to replace the picked-up object with the attachment.
    private void DropAttachment(AttachmentObject attachmentObject, AttachmentsScrObj previousAttachment)
    {
        if (previousAttachment != null)
        {
            attachmentObject.attachmentData = previousAttachment; // Replace the attachment data.
            SpriteRenderer spriteRenderer = attachmentObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = previousAttachment.Sprite; // Update the sprite.
            }
            Debug.Log("Replaced attachment with: " + previousAttachment.name);
        }
        else
        {
            Destroy(attachmentObject.gameObject); // Destroy the attachment object if there was no previous attachment.
        }
    }

    // Adds the picked-up attachment to the correct slot based on its type
    public AttachmentsScrObj AddAttachment(AttachmentsScrObj attachment, AttachmentObject attachmentObject)
    {
        AttachmentsScrObj previousAttachment = null;
        Debug.Log("Adding attachment: " + attachment.name);
        switch (attachment.Category)
        {
            case AttachmentCategory.BarrelSlot:
                previousAttachment = currentAttachments[0];
                currentAttachments[0] = attachment;
                Debug.Log("BarrelSlot");

                // Update the sprite of the BarrelLocation object
                if (barrelLocation != null)
                {
                    SpriteRenderer barrelSpriteRenderer = barrelLocation.GetComponent<SpriteRenderer>();
                    if (barrelSpriteRenderer != null)
                    {
                        barrelSpriteRenderer.sprite = attachment.Sprite;
                    }
                }
                break;
            case AttachmentCategory.Magazine:
                previousAttachment = currentAttachments[3];
                currentAttachments[3] = attachment;
                Debug.Log("Magazine");

                // Update the sprite of the MagazineLocation object
                if (magazineLocation != null)
                {
                    SpriteRenderer magazineSpriteRenderer = magazineLocation.GetComponent<SpriteRenderer>();
                    if (magazineSpriteRenderer != null)
                    {
                        magazineSpriteRenderer.sprite = attachment.Sprite;
                    }
                }
                break;
            case AttachmentCategory.Sight:
                previousAttachment = currentAttachments[1];
                currentAttachments[1] = attachment;
                Debug.Log("Sight");

                // Update the sprite of the SightLocation object
                if (sightLocation != null)
                {
                    SpriteRenderer sightSpriteRenderer = sightLocation.GetComponent<SpriteRenderer>();
                    if (sightSpriteRenderer != null)
                    {
                        sightSpriteRenderer.sprite = attachment.Sprite;
                    }
                }
                break;
            case AttachmentCategory.Underbarrel:
                previousAttachment = currentAttachments[2];
                currentAttachments[2] = attachment;
                Debug.Log("Underbarrel");

                // Update the sprite of the UnderbarrelLocation object
                if (underbarrelLocation != null)
                {
                    SpriteRenderer underbarrelSpriteRenderer = underbarrelLocation.GetComponent<SpriteRenderer>();
                    if (underbarrelSpriteRenderer != null)
                    {
                        underbarrelSpriteRenderer.sprite = attachment.Sprite;
                    }
                }
                break;
            case AttachmentCategory.Misc:
                previousAttachment = currentAttachments[4];
                currentAttachments[4] = attachment;
                Debug.Log("Misc");

                // Update the sprite of the MiscLocation object
                if (miscLocation != null)
                {
                    SpriteRenderer miscSpriteRenderer = miscLocation.GetComponent<SpriteRenderer>();
                    if (miscSpriteRenderer != null)
                    {
                        miscSpriteRenderer.sprite = attachment.Sprite;
                    }
                }
                break;
            default:
                Debug.LogWarning("Unknown attachment type.");
                break;
        }

        DropAttachment(attachmentObject, previousAttachment); // Replace the previous attachment with the new one.
        WeaponCheck();
        Debug.Log("Attachment added: " + attachment.name); // Confirmation log
        return previousAttachment; // Return the previous attachment
    }

    private bool semiShotFired = false; // Tracks if a shot has been fired in semi-automatic mode.

    // Method to handle the reloading process.
    public void Reload()
    {
        if (currentAmmo == currentMagazineSize) return; // Do not reload if the magazine is full.
        reloadManager.Reload(currentWeapon, currentReloadTime);
        currentAmmo = currentMagazineSize; // Refill ammo after reloading.
    }

    // Method to handle firing logic based on input.
    public void Fire(bool isHoldingFire)
    {
        // Prevent firing if the weapon is reloading.
        if (reloadManager.IsReloading()) return;

        // Check if the fire button is held and enough time has passed since the last shot.
        if (isHoldingFire && Time.time >= lastFireTime + 1f / currentFireRate)
        {
            // Check if there is ammo left to fire.
            if (currentAmmo > 0)
            {
                // Fire in automatic mode or if a semi-automatic shot hasn't been fired yet.
                if (currentFireMode == FireMode.Auto || !semiShotFired)
                {
                    lastFireTime = Time.time; // Update the last fire time.
                    FireProjectiles(); // Fire the projectiles.
                    cameraFollow.ShakeCamera(shakeDuration, shakeMagnitude); // Shake the camera.
                    semiShotFired = true; // Set semi-shot fired flag if in semi mode.

                    // Play the weapon's firing sound
                    if (currentWeapon.AudioClip != null && Time.time >= lastAudioPlayTime + 0.06f)
                    {
                        audioSource.Stop(); // Stop any currently playing audio clip.
                        audioSource.resource = currentWeapon.AudioClip;
                        audioSource.Play();
                        lastAudioPlayTime = Time.time; // Update the last audio play time.
                    }

                    currentAmmo--; // Decrease ammo count.
                }
            }
            else
            {
                if (currentWeapon.EmptyClip != null && Time.time >= lastAudioPlayTime + 0.3f)
                {
                    audioSource.Stop(); // Stop any currently playing audio clip.
                    audioSource.resource = currentWeapon.EmptyClip;
                    audioSource.Play();
                    lastAudioPlayTime = Time.time; // Update the last audio play time.
                    semiShotFired = currentFireMode == FireMode.Semi;
                }
            }
        }

        // Reset the semi-shot fired flag if the fire button is released.
        if (!isHoldingFire) semiShotFired = false;
    }

    // Method to instantiate and fire projectiles.
    private void FireProjectiles()
    {
        // Determine the default projectile prefab based on defaultWeaponProjectile
        switch (currentWeapon.DefaultWeaponProjectile)
        {
            case DefaultWeaponProjectile.Rocket:
                currentProjectilePrefab = RocketPrefab;
                break;
            case DefaultWeaponProjectile.Bullet:
            default:
                currentProjectilePrefab = BulletPrefab;
                break;
        }

        // Override the projectile prefab if explosiveRounds is true
        if (explosiveRounds)
        {
            currentProjectilePrefab = RocketPrefab;
        }

        // Loop to fire multiple projectiles if needed.
        for (int i = 0; i < currentProjectileCount; i++)
        {
            // Calculate a random angle offset for spread.
            float angleOffset = Random.Range(-currentSpread / 2f, currentSpread / 2f);
            Vector2 direction = firePoint.right; // Use the weapon's right direction.
            direction = Quaternion.Euler(0, 0, angleOffset) * direction; // Apply the angle offset.

            // Reverse direction if the weapon is flipped.
            if (firePoint.lossyScale.x < 0)
            {
                direction = -direction;
            }

            // Instantiate the projectile and set its velocity.
            GameObject projectile = Instantiate(currentProjectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.linearVelocity = direction * currentSpeed;

            // Rotate the projectile to face the direction it is being shot at.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // Set the damage and color for the projectile if it has a Projectile script.
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = currentDamage;
                projectileScript.SetColor(currentBulletColor); // Assuming Projectile has a SetColor method.
                if (penetratesEnemies)
                {
                    projectileScript.penetratesEnemies = true;
                }
            }

            // Set the damage and color for the projectile if it has a Projectile script.
            Explosive explosiveScript = projectile.GetComponent<Explosive>();
            if (explosiveScript != null)
            {
                explosiveScript.damage = currentDamage;
                explosiveScript.SetColor(currentBulletColor); // Assuming Projectile has a SetColor method.
                explosiveScript.explosiveRound = explosiveRounds;
                if (penetratesEnemies)
                {
                    explosiveScript.penetratesEnemies = true;
                }
            }
        }
    }

    public void UpdateEnemyMaterial()
    {
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            enemy.UpdateMaterial();
        }
    }
}