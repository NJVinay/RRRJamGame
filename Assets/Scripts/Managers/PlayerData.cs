// PlayerData.cs
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    // Persistent Data
    public WeaponsScrObj selectedWeapon; // Weapon chosen in spawn room
    public float health;
    public int keys;
    public bool hasBossKey;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            Debug.Log("PlayerData initialized and persisted.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetData()
    {
        health = 100;
        keys = 0;
        hasBossKey = false;
        selectedWeapon = null;
    }
}