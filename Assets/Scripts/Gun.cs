using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private float fireRate = 0.30f;       // Частота выстрелов (меньше = быстрее)
    [SerializeField] private Transform muzzle;             // Точка вылета пули (дочерний объект)
    [SerializeField] private GameObject projectilePrefab;  // Префаб пули
    [SerializeField] private float bulletSpeed = 20f;      // Скорость полета пули

    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject muzzleFlashPrefab; // Префаб вспышки из ствола
    [SerializeField] private Animator gunAnimator;         // Аниматор пистолета (назначается в Inspector)
    [SerializeField] private string shootAnimTrigger = "Fire"; // Название триггера в аниматоре

    [Header("Поиск цели")]
    [SerializeField] private LayerMask enemyLayer;         // Слой врагов (Enemy)
    [SerializeField] private LayerMask wallLayer;          // Слой стен (Wall)
    [SerializeField] private float detectionRadius = 10f;  // Радиус обнаружения

    // Внутренние переменные
    private float nextFireTime;
    private Vector3 originalLocalMuzzleOffset; // Запоминаем правильное положение дула

    void Awake()
    {
        // При старте запоминаем, где находится дуло относительно центра пистолета
        if (muzzle != null)
        {
            originalLocalMuzzleOffset = muzzle.localPosition;
        }

        // Убрали автоматический поиск аниматора — он назначается только вручную в Inspector
    }

    void Update()
    {
        // Проверяем, можно ли стрелять прямо сейчас
        if (Time.time < nextFireTime) return;

        Collider2D closestEnemy = FindClosestVisibleEnemy();

        if (closestEnemy != null)
        {
            AimAt(closestEnemy.transform.position);
            Fire(closestEnemy.transform.position);
            PlayShootEffects(); // <-- Запускаем анимацию и вспышку

            // Обновляем таймер перезарядки
            nextFireTime = Time.time + fireRate;
        }
    }

    private Collider2D FindClosestVisibleEnemy()
    {
        Collider2D closestEnemy = null;
        float closestDist = Mathf.Infinity;
        Vector2 origin = muzzle != null ? muzzle.position : transform.position;

        var hits = Physics2D.OverlapCircleAll(origin, detectionRadius, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.isTrigger)
                continue;

            Vector2 target = hit.bounds.center;
            float dist = Vector2.Distance(origin, target);

            if (dist > detectionRadius || dist >= closestDist || !HasDirectVisibility(origin, target, dist))
                continue;

            closestDist = dist;
            closestEnemy = hit;
        }

        return closestEnemy;
    }

    private bool HasDirectVisibility(Vector2 origin, Vector2 target, float distance)
    {
        int effectiveWallLayer = wallLayer.value;
        if (effectiveWallLayer == 0)
        {
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            if (wallLayerIndex >= 0)
                effectiveWallLayer = 1 << wallLayerIndex;
        }

        if (effectiveWallLayer == 0)
            return true;

        Vector2 direction = (target - origin).normalized;
        return Physics2D.Raycast(origin, direction, distance, effectiveWallLayer).collider == null;
    }

    /// <summary>
    /// Поворачивает пистолет к цели и фиксирует позицию дула, 
    /// чтобы оно не смещалось из-за зеркального отражения родителя (игрока).
    /// </summary>
    private void AimAt(Vector2 targetPosition)
    {
        // 1. Вычисляем направление к цели
        Vector2 direction = targetPosition - (Vector2)transform.position;
        float idealAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. ОПРЕДЕЛЯЕМ БАЗОВОЕ НАПРАВЛЕНИЕ ИГРОКА
        Transform playerRoot = transform.root;
        bool playerFacingLeft = playerRoot.localScale.x < 0;

        // Базовый угол: 0° если вправо, 180° если влево
        float baseAngle = playerFacingLeft ? 180f : 0f;

        // 3. ОГРАНИЧИВАЕМ УГОЛ НАКЛОНА (±60 градусов от горизонтали)
        // Это ключевой момент! Пистолет может смотреть вверх/вниз, 
        // но никогда не выйдет за пределы "безопасной зоны" и не перевернется.
        // Можешь менять 60f на 45f или 70f по вкусу.
        float clampedAngle = Mathf.Clamp(idealAngle, baseAngle - 60f, baseAngle + 60f);

        // 4. Применяем безопасный поворот
        transform.localRotation = Quaternion.Euler(0, 0, clampedAngle);

        // 5. КОРРЕКЦИЯ СПРАЙТА (flipX)
        // Логика остается прежней: флипаем, если направления игрока и дула не совпадают
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bool gunFacingLeft = (clampedAngle > 90f && clampedAngle < 270f) ||
                                 (clampedAngle < -90f);

            bool shouldFlip = (playerFacingLeft != gunFacingLeft);
            sr.flipX = shouldFlip;
        }

        // 6. ФИКСАЦИЯ MUZZLE
        // При ограниченном угле дуло остается стабильным. 
        // Просто возвращаем его в исходную локальную позицию.
        if (muzzle != null)
        {
            muzzle.localPosition = originalLocalMuzzleOffset;
        }
    }

    private void Fire(Vector2 targetPosition)
    {
        if (projectilePrefab == null || muzzle == null) return;

        // 1. Вычисляем направление ОТ дула К цели
        Vector2 shootDirection = ((Vector2)targetPosition - (Vector2)muzzle.position).normalized;

        // 2. Создаем пулю СМЕЩЕННОЙ вперед на 0.1 единицы, чтобы она не появилась внутри пистолета
        Vector3 spawnPos = muzzle.position + (Vector3)(shootDirection * 0.1f);

        // 3. Поворот пули должен совпадать с направлением стрельбы
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion spawnRot = Quaternion.Euler(0, 0, angle);

        var bullet = Instantiate(projectilePrefab, spawnPos, spawnRot);

        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0)
                rb.gameObject.layer = projectileLayer;

            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // Для кинематики используем MovePosition или просто задаем velocity (работает и так)
            rb.linearVelocity = shootDirection * bulletSpeed;

            // ВАЖНО: Игнорируем коллизии с игроком и его частями
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
                Physics2D.IgnoreLayerCollision(rb.gameObject.layer, playerLayer, true);

        }
    }

    /// <summary>
    /// Запускает визуальные эффекты выстрела: анимацию отдачи и вспышку.
    /// </summary>
    private void PlayShootEffects()
    {
        // 1. Запуск анимации стрельбы (триггер "Fire")
        if (gunAnimator != null && !string.IsNullOrEmpty(shootAnimTrigger))
        {
            gunAnimator.SetTrigger(shootAnimTrigger);
        }

        // 2. Создание вспышки (Muzzle Flash)
        if (muzzleFlashPrefab != null && muzzle != null)
        {
            // Спавним вспышку в точке дула с тем же поворотом, что и у пистолета
            var flash = Instantiate(muzzleFlashPrefab, muzzle.position, transform.rotation);

            // Автоматически удаляем вспышку через короткое время (0.05 сек), чтобы она не висела в воздухе
            Destroy(flash, 0.05f);
        }
    }

    // Визуализация радиуса поиска в редакторе (желтый круг)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }


}
