using UnityEngine;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Ссылка на игрока
    [SerializeField] private Vector2 defaultOffset = new Vector2(1f, 0f); // Смещение относительно игрока

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f; // Радиус удара
    [SerializeField] private int damage = 10;          // Урон за удар
    [SerializeField] private float attackCooldown = 0.8f; // Перезарядка между ударами
    [SerializeField] private LayerMask enemyLayer;     // Слой врагов (настраивается в Inspector)

    [Header("Visuals")]
    [SerializeField] private Animator anim;            // Аниматор меча (если есть)
    [SerializeField] private Collider2D hitCollider;   // Коллайдер зоны удара (должен быть выключен по умолчанию!)

    private float timeSinceLastAttack;
    private bool isAttacking = false;

    private void Start()
    {
        if (hitCollider != null)
            hitCollider.enabled = false; // Выключаем хитбокс по умолчанию

        timeSinceLastAttack = attackCooldown;

        // Проверка: если слой врагов не выбран, предупреждаем
        if (enemyLayer == 0)
            Debug.LogWarning("[Katana] Enemy Layer is not set in Inspector! Attacks may not detect enemies.");
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 1. Следование за игроком
        if (!isAttacking)
        {
            transform.position = (Vector2)playerTransform.position + defaultOffset;
            RotateTowardsNearestEnemy();
        }

        // 2. Логика авто-атаки
        if (HasEnemyInRange() && timeSinceLastAttack >= attackCooldown)
        {
            Attack();
        }

        timeSinceLastAttack += Time.deltaTime;
    }

    /// <summary>
    /// Проверка наличия врага в радиусе (использует LayerMask вместо тегов)
    /// </summary>
    private bool HasEnemyInRange()
    {
        // Ищем коллайдеры только на слое enemyLayer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            // Проверяем, что на объекте есть скрипт Enemy (это надежнее тега)
            if (hit.GetComponent<Enemy>() != null)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Поворот меча в сторону ближайшего врага (использует LayerMask)
    /// </summary>
    private void RotateTowardsNearestEnemy()
    {
        // Ищем врагов в расширенном радиусе для поворота
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 2, enemyLayer);
        Transform closestEnemy = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // Опять же, проверяем наличие скрипта Enemy
            if (hit.GetComponent<Enemy>() == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestEnemy = hit.transform;
            }
        }

        if (closestEnemy != null)
        {
            Vector2 dir = (closestEnemy.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Сброс в исходное положение (смотрит вправо)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// Выполнение атаки
    /// </summary>
    private void Attack()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;

        if (anim != null)
            anim.SetTrigger("Slash");

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            Invoke(nameof(DisableHitbox), 0.2f); // Активен 0.2 сек
        }

        Invoke(nameof(ResetAttackState), 0.4f); // Длительность анимации
    }

    private void DisableHitbox()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }

    /// <summary>
    /// Обработка попадания (использует LayerMask и компонент Enemy)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, находится ли объект на слое врагов
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Проверяем наличие скрипта Enemy
            Enemy enemyScript = other.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                Debug.Log($"Katana hit {other.name} for {damage} damage!");

                // Опционально: создать эффект попадания здесь
                // SpawnHitEffect(other.transform.position);окол
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}