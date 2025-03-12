using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public WeaponsScrObj currentWeapon;
    //In this array, each item corresponds to an attachment category
    // 0 - BarrelSlot
    // 1 - Sight
    // 2 - Underbarrel
    // 3 - Magazine
    // 4 - Misc
    public AttachmentsScrObj[] currentAttachments = new AttachmentsScrObj[5];

    FireMode currentFireMode;
    float currentDamage;
    float currentFireRate;
    float currentReloadTime;
    int currentMagazineSize;
    float currentSpread;
    int currentProjectileCount;
    float currentSpeed;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    private float lastFireTime;

    public void weaponCheck()
    {
        RefreshAttachments();

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

    void RefreshAttachments()
    {
        currentDamage = currentWeapon.Damage;
        currentFireRate = currentWeapon.FireRate;
        currentReloadTime = currentWeapon.ReloadTime;
        currentMagazineSize = currentWeapon.MagazineSize;
        currentSpread = currentWeapon.Spread;
        currentProjectileCount = currentWeapon.BulletCount;
        currentSpeed = currentWeapon.BulletSpeed;
        currentFireMode = currentWeapon.FireMode;

        for (int i = 0; i < currentAttachments.Length; i++)
        {
            if (currentAttachments[i] != null)
            {
                currentDamage += currentWeapon.Damage * currentAttachments[i].AddedDamage / 100;
                currentFireRate += currentWeapon.FireRate * currentAttachments[i].AddedFireRate / 100;
                currentReloadTime += currentWeapon.ReloadTime * currentAttachments[i].AddedReloadTime / 100;
                currentMagazineSize += (int)(currentWeapon.MagazineSize * currentAttachments[i].AddedMagazineSize / 100);
                currentSpread += currentWeapon.Spread * currentAttachments[i].AddedSpread / 100;
                currentProjectileCount = (int)(currentWeapon.BulletCount * currentAttachments[i].BulletCountMultiplier);
                currentSpeed += currentWeapon.BulletSpeed * currentAttachments[i].AddedSpeed / 100;
                currentFireMode = currentAttachments[i].ChangesFireMode ? currentAttachments[i].FireMode : currentFireMode;
            }
        }
    }

    private bool semiShotFired = false;

    public void Fire(bool isHoldingFire)
    {
        if (isHoldingFire && Time.time >= lastFireTime + 1f / currentFireRate)
        {
            if (currentFireMode == FireMode.Auto || !semiShotFired)
            {
                lastFireTime = Time.time;
                FireProjectiles();
                semiShotFired = currentFireMode == FireMode.Semi;
            }
        }

        if (!isHoldingFire) semiShotFired = false;
    }

    private void FireProjectiles()
    {
        for (int i = 0; i < currentProjectileCount; i++)
        {
            float angleOffset = Random.Range(-currentSpread / 2f, currentSpread / 2f);
            Vector2 direction = firePoint.right; // Use the weapon's right direction
            direction = Quaternion.Euler(0, 0, angleOffset) * direction;

            // If the weapon is flipped, reverse the direction
            if (firePoint.lossyScale.x < 0)
            {
                direction = -direction;
            }

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.linearVelocity = direction * currentSpeed;

            // Rotate the projectile to face the direction it is being shot at
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = currentDamage;
            }
        }
    }
}
