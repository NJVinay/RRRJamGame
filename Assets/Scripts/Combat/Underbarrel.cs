using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Underbarrel : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab; // Prefab for the grenade projectile.
    [SerializeField] private GameObject bulletPrefab; // Prefab for the shotgun projectile.
    private Transform firePoint; // The point from which projectiles are fired.
    private WeaponsManager weaponsManager; // Reference to the WeaponsManager component.
    private CameraFollow cameraFollow; // Reference to the CameraFollow component.
    private float grenadeDamage = 150f; // Damage of the grenade projectile.
    private float grenadeSpread = 15f; // Spread angle of the grenade.
    private float grenadeSpeed = 15f; // Speed of the grenade projectile.
    private float grenadeCooldown = 5f; // Cooldown between grenade shots.

    private float shotgunDamage = 12f; // Damage of the shotgun projectile.
    private float shotgunSpread = 15f; // Spread angle of the shotgun.
    private float shotgunSpeed = 25f; // Speed of the shotgun projectile.
    private float shotgunCooldown = 2f; // Cooldown between shotgun shots.
    private float shotgunProjectileCount = 5; // Number of projectiles fired by the shotgun.
    private float lastFireTime; // Time when the weapon was last fired.
    private bool isReloading = false; // Flag to check if the weapon is currently reloading.
    private Image progressBar; // Reference to the progress bar UI element.
    private AudioSource audioSource; // Reference to the AudioSource component.
    public AudioClip reloadSound; // Reference to the reload sound.
    public AudioResource grenadeSound; // Reference to the grenade sound.
    public AudioResource shotgunSound; // Reference to the grenade sound.


    public void Awake()
    {
        weaponsManager = GetComponentInParent<WeaponsManager>();
        audioSource = GetComponent<AudioSource>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        progressBar = GameObject.FindGameObjectWithTag("ProgressBar").GetComponent<Image>();
        firePoint = transform;
    }

    public void Fire()
    {
        if (weaponsManager != null)
        {
            if (weaponsManager.enablesGrenades && !isReloading)
            {
                FireGrenade();
                audioSource.Stop(); // Stop any currently playing audio clip.
                audioSource.resource = grenadeSound; // Set the audio clip to the grenade sound.
                audioSource.Play();
                lastFireTime = Time.time; // Update the last fire time.
                StartCoroutine(UnderbarrelReloadCoroutine(grenadeCooldown));
                cameraFollow.ShakeCamera(0.03f, 0.06f);
            }

            if (weaponsManager.enablesSpreadShot && !isReloading)
            {
                FireShotgun();
                audioSource.Stop(); // Stop any currently playing audio clip.
                audioSource.resource = shotgunSound; // Set the audio clip to the grenade sound.
                audioSource.Play();
                lastFireTime = Time.time; // Update the last fire time.
                StartCoroutine(UnderbarrelReloadCoroutine(shotgunCooldown));
                cameraFollow.ShakeCamera(0.03f, 0.06f);
            }
        }
    }

    private IEnumerator UnderbarrelReloadCoroutine(float currentReloadTime)
    {
        isReloading = true; // Set reloading status to true.
        float elapsedTime = 0f; // Track the elapsed time.

        while (elapsedTime < currentReloadTime)
        {
            elapsedTime += Time.deltaTime;
            progressBar.fillAmount = elapsedTime / currentReloadTime; // Update the radial wheel fill amount.
            yield return null; // Wait for the next frame.
        }

        // Play the reload finish sound.
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        progressBar.fillAmount = 0f; // Reset the radial wheel fill amount.
        isReloading = false; // Reset reloading status.
    }

    private void FireGrenade()
    {
        // Calculate a random angle offset for spread.
        float angleOffset = Random.Range(-grenadeSpread / 2f, grenadeSpread / 2f);
        Vector2 direction = firePoint.right; // Use the weapon's right direction.
        direction = Quaternion.Euler(0, 0, angleOffset) * direction; // Apply the angle offset.

        // Reverse direction if the weapon is flipped.
        if (firePoint.lossyScale.x < 0)
        {
            direction = -direction;
        }
        // Instantiate the projectile and set its velocity.
        GameObject projectile = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.linearVelocity = direction * grenadeSpeed;

        // Rotate the projectile to face the direction it is being shot at.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Set the damage for the projectile if it has an Explosive script.
        Explosive explosiveScript = projectile.GetComponent<Explosive>();
        if (explosiveScript != null)
        {
            explosiveScript.damage = grenadeDamage;
        }
    }

    private void FireShotgun()
    {
        for (int i = 0; i < shotgunProjectileCount; i++)
        {
            // Calculate a random angle offset for spread.
            float angleOffset = Random.Range(-shotgunSpread / 2f, shotgunSpread / 2f);
            Vector2 direction = firePoint.right; // Use the weapon's right direction.
            direction = Quaternion.Euler(0, 0, angleOffset) * direction; // Apply the angle offset.

            // Reverse direction if the weapon is flipped.
            if (firePoint.lossyScale.x < 0)
            {
                direction = -direction;
            }

            // Instantiate the projectile and set its velocity.
            GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.linearVelocity = direction * shotgunSpeed;

            // Rotate the projectile to face the direction it is being shot at.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // Set the damage for the projectile if it has a Projectile script.
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = shotgunDamage;
            }
        }
    }
}
