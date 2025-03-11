using UnityEngine;

public enum AttachmentCategory
{
    BarrelSlot,
    Sight,
    Underbarrel,
    Magazine,
    Misc
}

[CreateAssetMenu(fileName = "AttachmentsScrObj", menuName = "Scriptable Objects/AttachmentsScrObj")]
public class AttachmentsScrObj : ScriptableObject
{
    [Tooltip("How the attachment is called")]
    [SerializeField] private string attachmentName;
    [Tooltip("The attachment category so the player can only equip one attachment of each category")]
    [SerializeField] private AttachmentCategory category;

    [Space(20)]
    [Header("Weapon modifiers")]
    [Tooltip("Percentage of how much damage value is added to each bullet (it can be negative)")]
    [SerializeField] private float addedDamage;
    [Tooltip("Percentage of how much bullet spread value is increased (it can be negative)")]
    [SerializeField] private float addedSpread;
    [Tooltip("Percentage of how much bullet speed value is increased (it can be negative)")]
    [SerializeField] private float addedSpeed;
    [Tooltip("Percentage of how much reload time value is increased (it can be negative)")]
    [SerializeField] private float addedReloadTime;
    [Tooltip("Percentage of how much fire rate value is increased (it can be negative)")]
    [SerializeField] private float addedFireRate;
    [Tooltip("Multiplier applied to the number of bullets shot at once")]
    [SerializeField] private float addedMagazineSize;
    [Tooltip("Multiplier applied to the number of bullets shot at once")]
    [SerializeField] private float bulletCountMultiplier;
    [Tooltip("Percentage of how much damage value is subtracted when hitting a shield")]
    [SerializeField] private float subtractedBlockedDamage;

    [Space(20)]
    [Header("Player modifiers")]
    [Tooltip("Percentage of how much player speed value is increased (it can be negative)")]
    [SerializeField] private float addedPlayerSpeed;

    [Space(20)]
    [Header("Enablers")]
    [Tooltip("Check if the weapon can inflict damage by collision with enemies (ex. bayonet)")]
    [SerializeField] private bool damagesEnemiesByCollision;
    [Tooltip("Check to disable crossair (ex. Laser Sight)")]
    [SerializeField] private bool removesCrossair;
    [Tooltip("Check to enable crossair zoom (ex. 8x Scope)")]
    [SerializeField] private bool enablesCrossairZoom;
    [Tooltip("Check to enable a shader to highlight enemies (ex. Thermal Sight)")]
    [SerializeField] private bool highlightsEnemies;
    [Tooltip("Check to enable throwing grenades (ex. Underbarrel Grenade Launcher)")]
    [SerializeField] private bool enablesGrenades;
    [Tooltip("Check to enable a quick spread shot (ex. Underbarrel Shotgun)")]
    [SerializeField] private bool enablesSpreadShot;
    [Tooltip("Check to apply effects only when the player stands still (ex. Bipod)")]
    [SerializeField] private bool bipodBehaviour;
    [Tooltip("Check to enable explosive splashes that inflict +20% damage")]
    [SerializeField] private bool explosiveRounds;
    [Tooltip("Check to let bullets penetrate enemies")]
    [SerializeField] private bool penetratesEnemies;

    [Space(20)]
    [Header("Weapon type change")]
    [Tooltip("Check to enable a weapon type change. If not checked, the weapon type value below is ignored")]
    [SerializeField] private bool changesWeaponType;
    [SerializeField] private WeaponType weaponType;

    [Space(20)]
    [Header("Bullet color change")]
    [Tooltip("Check to change bulletColor")]
    [SerializeField] private Color bulletColor;

    public AttachmentCategory Category => category;
    public float AddedDamage => addedDamage;
    public float AddedSpread => addedSpread;
    public float AddedSpeed => addedSpeed;
    public float AddedReloadTime => addedReloadTime;
    public float AddedFireRate => addedFireRate;
    public float AddedMagazineSize => addedMagazineSize;
    public float BulletCountMultiplier => bulletCountMultiplier;
    public float SubtractedBlockedDamage => subtractedBlockedDamage;
    public float AddedPlayerSpeed => addedPlayerSpeed;
    public bool DamagesEnemiesByCollision => damagesEnemiesByCollision;
    public bool RemovesCrossair => removesCrossair;
    public bool EnablesCrossairZoom => enablesCrossairZoom;
    public bool HighlightsEnemies => highlightsEnemies;
    public bool EnablesGrenades => enablesGrenades;
    public bool EnablesSpreadShot => enablesSpreadShot;
    public bool BipodBehaviour => bipodBehaviour;
    public bool ExplosiveRounds => explosiveRounds;
    public bool PenetratesEnemies => penetratesEnemies;
    public bool ChangesWeaponType => changesWeaponType;
    public WeaponType WeaponType => weaponType;
    public Color BulletColor => bulletColor;
}
