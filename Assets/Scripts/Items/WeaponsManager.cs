using UnityEngine;

// This class manages the player's weapon, including its attachments and firing mechanics.
public class WeaponsManager : MonoBehaviour
{
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
    public FireMode currentFireMode {get; private set;}
    public float currentDamage {get; private set;}
    public float currentFireRate {get; private set;}
    public float currentReloadTime {get; private set;}
    public int currentMagazineSize {get; private set;}
    public float currentSpread {get; private set;}
    public int currentProjectileCount {get; private set;}
    public float currentSpeed {get; private set;}

    // Variables affecting player
    public float addedPlayerSpeed {get; private set;}
    public float subtractedBlockedDamage {get; private set;}  
    
    // Variables to store attachment effects.
    public bool damagesEnemiesByCollision {get; private set;}
    public bool removesCrosshair {get; private set;}
    public bool enablesCrosshairZoom {get; private set;}
    public bool highlightsEnemies {get; private set;}
    public bool enablesGrenades {get; private set;}
    public bool enablesSpreadShot {get; private set;}
    public bool bipodBehaviour {get; private set;}
    public bool explosiveRounds {get; private set;}
    public bool penetratesEnemies {get; private set;}

    [SerializeField] GameObject projectilePrefab; // Prefab for the projectile to be fired.
    [SerializeField] Transform firePoint; // The point from which projectiles are fired.
    private float lastFireTime; // Tracks the last time a shot was fired.

    // Method to check and log the current weapon and its attachments.
    public void WeaponCheck()
    {
        RefreshAttachments(); // Update weapon stats based on attachments.

        // Log the current weapon and attachment details.
        Debug.Log("Weapon: " + currentWeapon.WeaponName);
        Debug.Log("Barrel Attachment: " + (currentAttachments[0] != null ? currentAttachments[0].attachmentName : "None"));
        Debug.Log("Sight Attachment: " + (currentAttachments[1] != null ? currentAttachments[1].attachmentName : "None"));
        Debug.Log("Underbarrel Attachment: " + (currentAttachments[2] != null ? currentAttachments[2].attachmentName : "None"));
        Debug.Log("Magazine Attachment: " + (currentAttachments[3] != null ? currentAttachments[3].attachmentName : "None"));
        Debug.Log("Misc Attachment: " + (currentAttachments[4] != null ? currentAttachments[4].attachmentName : "None"));
        Debug.Log("Damage: " + currentDamage);
        Debug.Log("Fire Rate: " + currentFireRate);
        Debug.Log("Reload Time: " + currentReloadTime);
        Debug.Log("Magazine Size: " + currentMagazineSize);
        Debug.Log("Spread: " + currentSpread);
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
                    // Insert here the logic to change bullet color
                }
            }
        }
    }

    private bool semiShotFired = false; // Tracks if a shot has been fired in semi-automatic mode.

    // Method to handle firing logic based on input.
    public void Fire(bool isHoldingFire)
    {
        // Check if the fire button is held and enough time has passed since the last shot.
        if (isHoldingFire && Time.time >= lastFireTime + 1f / currentFireRate)
        {
            // Fire in automatic mode or if a semi-automatic shot hasn't been fired yet.
            if (currentFireMode == FireMode.Auto || !semiShotFired)
            {
                lastFireTime = Time.time; // Update the last fire time.
                FireProjectiles(); // Fire the projectiles.
                semiShotFired = currentFireMode == FireMode.Semi; // Set semi-shot fired flag if in semi mode.
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

            // Set the damage for the projectile if it has a Projectile script.
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = currentDamage;
            }
        }
    }
}