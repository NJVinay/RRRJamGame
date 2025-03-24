using Pathfinding;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class Enemy : MonoBehaviour
{
    AIDestinationSetter enemyTargetScript;
    public Material defaultMaterial;
    public Material outlineMaterial;
    private WeaponsManager weaponsManager;
    private SpriteRenderer spriteRenderer;

    public float health = 500; // Add health property
    private Vector3 lastPosition;

    public GameObject slashAttackPrefab;
    public GameObject projectilePrefab;

    
    [Header("Melee Settings")]
    public bool meleeEnemy;
    public float meleeRange;
    public float meleeAttackRange;
    public float meleeAttackDamage;

    [Header("Ranged Settings")]
    public bool rangedEnemy;
    public float rangedDamage;
    public float rangedBulletCount;
    public float rangedBulletSpeed;
    public float rangedBulletSpread;
    public float engagementDistance; // Add rangedRadius variable
    public float wanderDistance = 5f; // Add wanderDistance variable
    public float rangedPlayerDistance = 2f; // Add rangedPlayerDistance variable
    private Vector3 wanderPoint; // Track the current wander point
    private float nextWanderTime = 0f; // Track the next wander time
    private AIPath aiPath; // Reference to AIPath component
    private Seeker seeker; // Reference to Seeker component
    private int maxAttempts = 10; // Maximum attempts to find a valid wander point

    [Header("Attack Settings")]
    public float attackCooldown = 2.0f; // Add attackCooldown variable for melee
    private float lastAttackTime = 0f; // Track the last attack time
    public float fireRate = 1.0f; // Add fireRate variable
    private float lastFireTime = 0f; // Track the last fire time
    public float attackPause;
    private bool isAttacking;

    public AudioResource rangedAttackSound; // Add rangedAttackSound variable
    public AudioResource painSound; // Add painSound variable
    private float lastPainSoundTime = 0f; // Track the last pain sound time
    private AudioSource audioSource; // Add AudioSource component
    private Rigidbody2D rb;

    public float speed; // Add speed variable

    private void Awake()
    {
        weaponsManager = FindFirstObjectByType<WeaponsManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyTargetScript = GetComponent<AIDestinationSetter>();
        enemyTargetScript.target = GameObject.FindGameObjectWithTag("Player").transform;
        lastPosition = transform.position;
        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();
        audioSource = GetComponent<AudioSource>(); // Initialize AudioSource
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        UpdateMaterial();
        speed += Random.Range(-0.5f, 0.5f); // Randomize speed based on itself by 0.5f
        aiPath.maxSpeed = speed; // Set AIPath max speed to the randomized speed
    }

    private void Update()
    {
        FlipSprite();
        if (meleeEnemy) CheckMeleeRange();
        if (rangedEnemy) StrafeAroundTarget();
        lastPosition = transform.position;
    }

    public void UpdateMaterial()
    {
        if (weaponsManager != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.material = weaponsManager.highlightsEnemies ? outlineMaterial : defaultMaterial;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            PlayPainSound(); // Play pain sound when taking damage
        }
    }

    private void PlayPainSound()
    {
        if (audioSource != null && painSound != null && Time.time >= lastPainSoundTime + 0.25f)
        {
            audioSource.Stop(); // Stop any currently playing audio
            audioSource.resource = painSound;
            audioSource.Play();
            lastPainSoundTime = Time.time; // Update the last pain sound time
        }
    }
 
    private void FlipSprite()
    {
        if (meleeEnemy)
        {
            Vector3 movementDirection = transform.position - lastPosition;
            if (movementDirection.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (movementDirection.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
        else if (rangedEnemy)
        {
            if (enemyTargetScript.target != null)
            {
                Vector3 directionToTarget = enemyTargetScript.target.position - transform.position;
                if (directionToTarget.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
                else if (directionToTarget.x > 0)
                {
                    spriteRenderer.flipX = false;
                }
            }
        }
    }

    private IEnumerator StopMovementForSeconds(float seconds)
    {
        isAttacking = true;
        aiPath.canMove = false;
        rb.linearVelocity = Vector2.zero;
        Animator animator = GetComponent<Animator>();
        if (meleeEnemy && animator != null)
        {
            animator.speed = 0;
        }
        yield return new WaitForSeconds(seconds);
        rb.linearVelocity = Vector2.zero;
        if (meleeEnemy && animator != null)
        {
            animator.speed = 1;
        }
        isAttacking = false;
        aiPath.canMove = true;
    }

    private void CheckMeleeRange()
    {
        if (enemyTargetScript.target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, enemyTargetScript.target.position);
            if (distanceToTarget <= meleeRange)
            {
                aiPath.destination = enemyTargetScript.target.position; // Ensure the player is the target
                GetComponent<AIPath>().canMove = false;

                // Check if the cooldown period has passed
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    // Instantiate Slash Attack
                    Vector3 directionToTarget = (enemyTargetScript.target.position - transform.position).normalized;
                    GameObject slashAttack = Instantiate(slashAttackPrefab, transform.position, Quaternion.identity);
                    slashAttack.GetComponent<slashAttack>().slashDamage = meleeAttackDamage;
                    slashAttack.transform.right = directionToTarget;
                    slashAttack.transform.position = transform.position + directionToTarget * Mathf.Min(meleeAttackRange, distanceToTarget);

                    // Stop movement for a brief moment
                    StartCoroutine(StopMovementForSeconds(attackPause));

                    // Update the last attack time
                    lastAttackTime = Time.time;
                }
            }
            else if (!isAttacking)
            {
                aiPath.destination = enemyTargetScript.target.position; // Ensure the player is the target
                GetComponent<AIPath>().canMove = true;
            }
        }
    }

    private void StrafeAroundTarget()
    {
        if (enemyTargetScript.target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, enemyTargetScript.target.position);
            if (distanceToTarget <= engagementDistance)
            {
                if (Vector3.Distance(transform.position, wanderPoint) <= 1f || wanderPoint == Vector3.zero || Time.time >= nextWanderTime)
                {
                    int attempts = 0;
                    do
                    {
                        wanderPoint = (Vector3)Random.insideUnitCircle * wanderDistance + transform.position;
                        attempts++;
                    } while ((Vector3.Distance(wanderPoint, enemyTargetScript.target.position) > engagementDistance || Vector3.Distance(wanderPoint, enemyTargetScript.target.position) < rangedPlayerDistance || !IsPathfindable(wanderPoint)) && attempts < maxAttempts);

                    if (attempts < maxAttempts)
                    {
                        aiPath.destination = wanderPoint;
                    }
                    else
                    {
                        aiPath.destination = GetFallbackWanderPoint(); // Fallback to a valid wander point
                    }

                    nextWanderTime = Time.time + 1f; // Set the next wander time to 1 second later
                }

                // Check if the enemy can fire at the player
                if (Time.time >= lastFireTime + fireRate)
                {
                    // Trigger Shoot() in the animator
                    GetComponent<Animator>().SetTrigger("Shoot");

                    // Stop movement for a brief moment
                    StartCoroutine(StopMovementForSeconds(attackPause));

                    lastFireTime = Time.time;
                }
            }
            else
            {
                aiPath.destination = enemyTargetScript.target.position;
                wanderPoint = Vector3.zero; // Reset wander point
            }
        }
    }

    void ShootProjectile()
    {
        Transform firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError("FirePoint not found!");
            return;
        }

        for (int i = 0; i < rangedBulletCount; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 direction = (enemyTargetScript.target.position - firePoint.position).normalized;
            float spread = Random.Range(-rangedBulletSpread, rangedBulletSpread);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, spread) * direction;
            rb.linearVelocity = spreadDirection * rangedBulletSpeed;

            projectile.GetComponent<Projectile>().damage = rangedDamage;
            
            // Rotate the projectile to face the direction it is being shot at.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        // Play ranged attack sound
        if (audioSource != null && rangedAttackSound != null)
        {
            audioSource.resource = rangedAttackSound;
            audioSource.Play();
        }
    }

    private Vector3 GetFallbackWanderPoint()
    {
        Vector3 fallbackPoint;
        int attempts = 0;
        do
        {
            fallbackPoint = (Vector3)Random.insideUnitCircle * wanderDistance + transform.position;
            attempts++;
        } while ((Vector3.Distance(fallbackPoint, enemyTargetScript.target.position) < rangedPlayerDistance || !IsPathfindable(fallbackPoint)) && attempts < maxAttempts);

        return fallbackPoint;
    }

    private bool IsPathfindable(Vector3 point)
    {
        // Check if the point is reachable by pathfinding
        GraphNode startNode = AstarPath.active.GetNearest(transform.position).node;
        GraphNode targetNode = AstarPath.active.GetNearest(point).node;
        
        return PathUtilities.IsPathPossible(startNode, targetNode);
    }

    private void Die()
    {
        // Add logic for enemy death, e.g., play animation, destroy object, etc.
        Destroy(gameObject);
    }
}
