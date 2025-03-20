using Pathfinding;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    AIDestinationSetter enemyTargetScript;

    public float health = 500; // Add health property

    private void Awake()
    {
        enemyTargetScript = GetComponent<AIDestinationSetter>();
        enemyTargetScript.target = GameObject.FindGameObjectWithTag("Player").transform;
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
