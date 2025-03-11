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

    WeaponType currentWeaponType;
    float currentDamage;
    float currentFireRate;
    float currentReloadTime;
    int currentMagazineSize;
    float currentSpread;

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            RefreshAttachments();

            Debug.Log("");
            Debug.Log("");
            Debug.Log("Damage: " + currentDamage);
            Debug.Log("Fire Rate: " + currentFireRate);
            Debug.Log("Reload Time: " + currentReloadTime);
            Debug.Log("Magazine Size: " + currentMagazineSize);
            Debug.Log("Spread: " + currentSpread);
        }
    }

    void RefreshAttachments()
    {
        currentDamage = currentWeapon.Damage;
        currentFireRate = currentWeapon.FireRate;
        currentReloadTime = currentWeapon.ReloadTime;
        currentMagazineSize = currentWeapon.MagazineSize;
        currentSpread = currentWeapon.Spread;

        for (int i = 0; i < currentAttachments.Length; i++)
        {
            if (currentAttachments[i] != null)
            {
                currentDamage += currentWeapon.Damage *currentAttachments[i].AddedDamage /100;
                currentFireRate += currentWeapon.FireRate *currentAttachments[i].AddedFireRate /100;
                currentReloadTime += currentWeapon.ReloadTime *currentAttachments[i].AddedReloadTime /100;
                currentMagazineSize += (int)(currentWeapon.MagazineSize *currentAttachments[i].AddedMagazineSize);
                currentSpread += currentWeapon.Spread *currentAttachments[i].AddedSpread /100;
            }
        }
    }
}
