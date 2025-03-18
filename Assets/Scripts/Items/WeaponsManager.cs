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
    public float currentFireRate { get; private set; }
    public float currentReloadTime { get; private set; }
    public int currentMagazineSize { get; private set; }
    public float currentSpread { get; private set; }
    public int currentProjectileCount { get; private set; }
    public float currentSpeed { get; private set; }
    public int currentAmmo { get; private set; } // Current ammo in the magazine.

    // Variables affecting player
    public float addedPlayerSpeed { get; private set; }
    public float subtractedBlockedDamage { get; private set; }

    // Variables to store attachment effects.
    public bool damagesEnemiesByCollision { get; private set; }
    public bool removesCrosshair { get; private set; }
    public bool enablesCrosshairZoom { get; private set; }
    public bool highlightsEnemies { get; private set; }
    public bool enablesGrenades { get; private set; }
    public bool enablesSpreadShot { get; private set; }
    public bool bipodBehaviour { get; private set; }
    public bool explosiveRounds { get; private set; }
    public bool penetratesEnemies { get; private set; }

    [SerializeField] GameObject projectilePrefab; // Prefab for the projectile to be fired.
    [SerializeField] Transform firePoint; // The point from which projectiles are fired.
    private float lastFireTime; // Tracks the last time a shot was fired.
    private float lastAudioPlayTime; // Tracks the last time an audio clip was played.

    private AudioSource audioSource;
    private ReloadManager reloadManager;

    private Color currentBulletColor = Color.white; // Default bullet color.

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        reloadManager = GetComponent<ReloadManager>();
        currentAmmo = currentMagazineSize; // Initialize current ammo.
        lastAudioPlayTime = -0.1f; // Initialize last audio play time.
    }

    // Method to check and log the current weapon and its attachments.
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
        Debug.Log("Removes Crosshair: " + removesCrosshair);
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
    void RefreshAttachments()
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
        removesCrosshair = false;
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
                currentProjectileCount = (int)(currentWeapon.BulletCount * currentAttachments[i].BulletCountMultiplier);
                currentSpeed += currentWeapon.BulletSpeed * currentAttachments[i].AddedSpeed / 100;

                //Apply player modifiers
                addedPlayerSpeed += currentAttachments[i].AddedPlayerSpeed;
                subtractedBlockedDamage += currentAttachments[i].SubtractedBlockedDamage;

                // Apply attachment effects
                // If multiple attachments have the same effect (they shouldn't), the effect is enabled
                damagesEnemiesByCollision |= currentAttachments[i].DamagesEnemiesByCollision;
                removesCrosshair |= currentAttachments[i].RemovesCrosshair;
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
            }
        }

        audioManager.UpdateDynamicMusic();
    }

    // Method to drop the previous attachment at the player's location.
    private void DropAttachment(AttachmentsScrObj previousAttachment)
    {
        if (previousAttachment != null)
        {
            Vector3 playerPosition = transform.position; // Get the player's current position.
            Debug.Log("Dropping attachment: " + previousAttachment.name + " at position: " + playerPosition);
            // Add functionality to instantiate and drop the attachment in the game world.
        }
    }

    // Adds the picked up attachment to the correct slot based on its type
    public void AddAttachment(AttachmentsScrObj attachment)
    {
        AttachmentsScrObj previousAttachment = null;
        Debug.Log("Adding attachment: " + attachment.name);
        switch (attachment.Category)
        {
            case AttachmentCategory.BarrelSlot:
                previousAttachment = currentAttachments[0];
                currentAttachments[0] = attachment;
                Debug.Log("BarrelSlot");
                break;
            case AttachmentCategory.Magazine:
                previousAttachment = currentAttachments[3];
                currentAttachments[3] = attachment;
                Debug.Log("Magazine");
                break;
            case AttachmentCategory.Sight:
                previousAttachment = currentAttachments[1];
                currentAttachments[1] = attachment;
                Debug.Log("Sight");
                break;
            case AttachmentCategory.Underbarrel:
                previousAttachment = currentAttachments[2];
                currentAttachments[2] = attachment;
                Debug.Log("Underbarrel");
                break;
            case AttachmentCategory.Misc:
                previousAttachment = currentAttachments[4];
                currentAttachments[4] = attachment;
                Debug.Log("Misc");
                break;
            default:
                Debug.LogWarning("Unknown attachment type.");
                break;
        }

        DropAttachment(previousAttachment); // Drop the previous attachment.
        WeaponCheck();
        Debug.Log("Attachment added: " + attachment.name); // Confirmation log
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
                    semiShotFired = true; // Set semi-shot fired flag if in semi mode.

                    // Play the weapon's firing sound
                    if (currentWeapon.AudioClip != null && Time.time >= lastAudioPlayTime + 0.08f)
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
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
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
            }
        }
    }
}