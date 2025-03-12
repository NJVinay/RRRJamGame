using UnityEngine;

public enum FireMode
{
    Auto,
    Semi
}

[CreateAssetMenu(fileName = "WeaponsScrObj", menuName = "Scriptable Objects/WeaponsScrObj")]
public class WeaponsScrObj : ScriptableObject
{
    [Header("Weapon info")]
    [Tooltip("Literally how the weapon is called ahah")]
    [SerializeField] private string weaponName;
    [Tooltip("The firemode of the weapon")]
    [SerializeField] private FireMode fireMode;
    [Tooltip("How much damage a single bullet delivers")]
    [SerializeField] private float damage;
    [Tooltip("How many bullets are being shot in one second")]
    [SerializeField] private float fireRate;
    [Tooltip("How much seconds it takes to reload the weapon")]
    [SerializeField] private float reloadTime;
    [Tooltip("How many bullets can be shot before reloading")]
    [SerializeField] private int magazineSize;
    [Tooltip("How wide and imprecise is the shot (in degrees)")]
    [SerializeField] private float spread;
    
    [Space(20)]
    [Header("Bullet info")]
    [Tooltip("How many bullets get shot at once")]
    [SerializeField] private int bulletCount;
    [Tooltip("How many units the bullet traverses in a second")]
    [SerializeField] private float bulletSpeed;

    [Space(20)]
    [Header("Asset data")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Sprite sprite;

    public string WeaponName => weaponName;
    public FireMode FireMode => fireMode;
    public float Damage => damage;
    public float FireRate => fireRate;
    public float ReloadTime => reloadTime;
    public int MagazineSize => magazineSize;
    public float Spread => spread;
    public int BulletCount => bulletCount;
    public float BulletSpeed => bulletSpeed;
    public AudioClip AudioClip => audioClip;
    public Sprite Sprite => sprite;
}
