using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReloadManager : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isReloading = false;
    public Image radialWheel; // Reference to the radial wheel UI element.

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Automatically fetch the Image component from the object tagged with "ReloadWheel".
        GameObject reloadWheelObject = GameObject.FindGameObjectWithTag("ReloadWheel");
        if (reloadWheelObject != null)
        {
            radialWheel = reloadWheelObject.GetComponent<Image>();
        }
    }

    // Method to handle the reloading process.
    public void Reload(WeaponsScrObj currentWeapon, float currentReloadTime)
    {
        if (!isReloading)
        {
            StartCoroutine(ReloadCoroutine(currentWeapon, currentReloadTime));
        }
    }

    // Coroutine to manage the reload timing and audio.
    private IEnumerator ReloadCoroutine(WeaponsScrObj currentWeapon, float currentReloadTime)
    {
        isReloading = true; // Set reloading status to true.
        float elapsedTime = 0f; // Track the elapsed time.

        // Play the reload start sound.
        if (currentWeapon.ReloadClip != null)
        {
            audioSource.resource = currentWeapon.ReloadClip;
            audioSource.Play();
        }

        while (elapsedTime < currentReloadTime)
        {
            elapsedTime += Time.deltaTime;
            radialWheel.fillAmount = elapsedTime / currentReloadTime; // Update the radial wheel fill amount.
            yield return null; // Wait for the next frame.
        }

        // Play the reload finish sound.
        if (currentWeapon.ReloadFinishClip != null)
        {
            audioSource.resource = currentWeapon.ReloadFinishClip;
            audioSource.Play();
        }

        radialWheel.fillAmount = 0f; // Reset the radial wheel fill amount.
        isReloading = false; // Reset reloading status.
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}
