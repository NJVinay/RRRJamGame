using UnityEngine;
using UnityEngine.Audio;

public enum FireMode
{
    Auto,
    Semi
}
public enum DefaultWeaponProjectile
{
    Bullet,
    Rocket
}

[CreateAssetMenu(fileName = "WeaponsScrObj", menuName = "Scriptable Objects/WeaponsScrObj")]
public class WeaponsScrObj : ScriptableObject
{
    [Header("Weapon info")]
    [Tooltip("Literally how the weapon is called ahah")]
    [SerializeField] private string weaponName;
    [Tooltip("The firemode of the weapon")]
    [SerializeField] private FireMode fireMode;
    [Tooltip("The default projectile of the weapon")]
    [SerializeField] private DefaultWeaponProjectile defaultWeaponProjectile;
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
    [Tooltip("The clip for firing the weapon")]
    [SerializeField] private AudioResource audioClip;
    [Tooltip("The clip for emptying the magazine of the weapon.")]
    [SerializeField] private AudioResource emptyClip;
    [Tooltip("The clip for reloading the weapon")]
    [SerializeField] private AudioResource reloadClip;
    [Tooltip("The clip for finishing a reload of the weapon.")]
    [SerializeField] private AudioResource reloadFinishClip;
    [Tooltip("The prefab for the weapon")]
    [SerializeField] private GameObject prefab;

    [Space(20)]
    [Header("Index for AudioManager")]
    [SerializeField] private int index;

    public string WeaponName => weaponName;
    public FireMode FireMode => fireMode;
    public DefaultWeaponProjectile DefaultWeaponProjectile => defaultWeaponProjectile;
    public float Damage => damage;
    public float FireRate => fireRate;
    public float ReloadTime => reloadTime;
    public int MagazineSize => magazineSize;
    public float Spread => spread;
    public int BulletCount => bulletCount;
    public float BulletSpeed => bulletSpeed;
    public AudioResource AudioClip => audioClip;
    public AudioResource EmptyClip => emptyClip;
    public AudioResource ReloadClip => reloadClip;
    public AudioResource ReloadFinishClip => reloadFinishClip;
    public GameObject Prefab => prefab;
    public int Index => index;
    public AttachmentsScrObj barrel;
    public AttachmentsScrObj sight;
    public AttachmentsScrObj underbarrel;
    public AttachmentsScrObj magazine;
    public AttachmentsScrObj misc;
}
