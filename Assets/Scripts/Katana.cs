using UnityEngine;
using System.Collections.Generic;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Combat")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visuals")]
    [SerializeField] private Animator anim;
    [SerializeField] private Collider2D hitCollider;

    private float timeSinceLastAttack;
    private bool isAttacking = false;
    private HashSet<EnemyDarya> damagedEnemies = new HashSet<EnemyDarya>();

    private void Start()
    {
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            if (!hitCollider.isTrigger)
                Debug.LogError("[Katana] HitCollider MUST be a Trigger!");
        }
        else
        {
            Debug.LogError("[Katana] HitCollider NOT assigned! Add a Collider2D with IsTrigger and assign it.");
        }

        timeSinceLastAttack = attackCooldown;

        if (playerTransform == null && transform.parent != null)
            playerTransform = transform.parent;
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (!isAttacking)
            RotateTowardsNearestEnemy();

        if (!isAttacking && timeSinceLastAttack >= attackCooldown)
        {
            if (HasEnemyInRange())
                Attack();
        }
    }

    private bool HasEnemyInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<EnemyDarya>() != null) return true;
        }
        return false;
    }

    private void RotateTowardsNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 2, enemyLayer);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            EnemyDarya e = hit.GetComponent<EnemyDarya>();
            if (e == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        if (closest != null)
        {
            Vector2 dir = (closest.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;
        damagedEnemies.Clear();

        if (anim != null)
            anim.SetTrigger("Attack");
        else
            Debug.LogWarning("[Katana] Animator not assigned!");

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            Invoke(nameof(DisableHitbox), 0.2f);
        }

        Invoke(nameof(ResetAttackState), 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking) return;
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        EnemyDarya enemyScript = other.GetComponent<EnemyDarya>();
        if (enemyScript != null && !damagedEnemies.Contains(enemyScript))
        {
            enemyScript.TakeDamage(damage);
            damagedEnemies.Add(enemyScript);
            Debug.Log($"[Katana] HIT {other.name} for {damage} dmg!");
        }
    }

    private void DisableHitbox()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    private void ResetAttackState()
    {
        isAttacking = false;
        damagedEnemies.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
