using Pathfinding;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    AIDestinationSetter enemyTargetScript;
    public Material defaultMaterial;
    public Material outlineMaterial;
    private WeaponsManager weaponsManager;
    private SpriteRenderer spriteRenderer;

    public float health = 500; // Add health property

    private void Awake()
    {
        weaponsManager = FindFirstObjectByType<WeaponsManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyTargetScript = GetComponent<AIDestinationSetter>();
        enemyTargetScript.target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        UpdateMaterial();
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
    }

    private void Die()
    {
        // Add logic for enemy death, e.g., play animation, destroy object, etc.
        Destroy(gameObject);
    }
}
