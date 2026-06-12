using UnityEngine;
using System.Collections.Generic;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform katanaPivot; // Точка, вокруг которой вращается меч

    [Header("Combat")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visuals & Physics")]
    [SerializeField] private Animator anim;
    [SerializeField] private Collider2D hitCollider;

    private float timeSinceLastAttack;
    private bool isAttacking;
    private HashSet<EnemyDarya> damagedEnemies = new HashSet<EnemyDarya>();

    private void Start()
    {
        // Инициализация коллайдера
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            if (!hitCollider.isTrigger)
            {
                Debug.LogWarning("[Katana] HitCollider should be a Trigger! Fixing...");
                hitCollider.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("[Katana] HitCollider NOT assigned! Add a Collider2D with IsTrigger.");
        }

        timeSinceLastAttack = attackCooldown;

        // Авто-поиск игрока
        if (playerTransform == null && transform.parent != null)
            playerTransform = transform.parent;

        // Авто-поиск пивота
        if (katanaPivot == null)
            katanaPivot = transform;
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        // Вращаемся к врагу только если НЕ атакуем
        if (!isAttacking)
        {
            RotateTowardsNearestEnemy();

            // Проверяем, можно ли атаковать
            if (timeSinceLastAttack >= attackCooldown && HasEnemyInRange())
            {
                Attack();
            }
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
        // Ищем врагов в увеличенном радиусе, чтобы меч "смотрел" на них заранее
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

        if (closest != null && katanaPivot != null)
        {
            Vector2 dir = (closest.position - katanaPivot.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Вращаем именно пивот, а не сам спрайт меча (если они разделены)
            katanaPivot.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else if (katanaPivot != null)
        {
            // Возврат в нейтральное положение
            katanaPivot.localRotation = Quaternion.identity;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;
        damagedEnemies.Clear();

        if (anim != null)
            anim.SetTrigger("Attack");

        // Включаем хитбокс на короткое время
        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            Invoke(nameof(DisableHitbox), 0.2f); // Время активной фазы удара
        }

        // Сброс состояния атаки
        Invoke(nameof(ResetAttackState), 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking) return;

        // Проверка слоя врага
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

    // Визуализация радиуса атаки в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange * 2);
    }
}
