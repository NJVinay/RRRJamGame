using UnityEngine;
using System.IO;
using System.Linq;

// This script should be attached to interactable objects (e.g., items on the ground)
public class WeaponObject : MonoBehaviour
{
    public WeaponsScrObj weaponData; // Reference to weapon ScriptableObject
    public bool spawnRandomWeapon;

    private void Start()
    {
        if (spawnRandomWeapon)
        {
            string path = "Assets/ScriptableObjectsAssets/Weapons";
            string[] weaponFiles = Directory.GetFiles(path, "*.asset");
            string randomWeaponFile = weaponFiles[Random.Range(0, weaponFiles.Length)];
            weaponData = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponsScrObj>(randomWeaponFile);
        }

        // Change the sprite to the one set in the WeaponScrObj
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && weaponData != null)
        {
            spriteRenderer.sprite = weaponData.Sprite;
        }
    }

    // Method to handle interaction
    public void Interact()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>(); // Find player in scene
        if (player != null)
        {
            // Instantiate the weapon prefab as a child of WeaponLocation
            GameObject weaponLocation = GameObject.FindWithTag("WeaponLocation");
            if (weaponLocation != null && weaponData.Prefab != null)
            {
                Instantiate(weaponData.Prefab, weaponLocation.transform);
                Destroy(gameObject);
            }
        }
    }
}
