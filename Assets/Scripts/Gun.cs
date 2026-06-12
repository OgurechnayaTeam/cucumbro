using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject muzzleEffect; // Эффект выстрела
    [SerializeField] private Transform muzzlePoint;   // Точка вылета пули
    [SerializeField] private GameObject projectilePrefab;

    [Header("Config")]
    [SerializeField] private float fireDistance = 10f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 15f; // Скорость пули (отдельно от дальности)

    [Header("Positioning")]
    [Tooltip("Смещение оружия относительно точки крепления (руки)")]
    [SerializeField] private Vector2 weaponOffset = new Vector2(0.3f, 0.15f);

    private Transform handSlot;
    private Animator anim;
    private Transform closestEnemy;
    private float timeSinceLastShot;

    private void Start()
    {
        anim = GetComponent<Animator>();
        timeSinceLastShot = fireRate;

        // Автоматически находим руку (родителя), если не назначена вручную
        if (handSlot == null && transform.parent != null)
        {
            handSlot = transform.parent;
            Debug.Log($"[Gun] HandSlot assigned automatically: {handSlot.name}");
        }
        else if (handSlot == null)
        {
            Debug.LogError("[Gun] No parent found! Assign HandSlot manually in Inspector.");
        }
    }

    private void Update()
    {
        // 1. Позиционирование: используем localPosition, чтобы оружие "прилипло" к руке
        // и учитывало масштаб игрока/руки
        if (handSlot != null)
        {
            transform.localPosition = weaponOffset;
        }

        // 2. Логика боя
        FindClosestEnemy();
        AimAtEnemy();
        HandleShooting();
    }

    void HandleShooting()
    {
        // Стреляем только если есть цель и она в радиусе
        if (closestEnemy == null ||
            Vector2.Distance(transform.position, closestEnemy.position) > fireDistance)
            return;

        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= fireRate)
        {
            Shoot();
            timeSinceLastShot = 0f;
        }
    }

    void FindClosestEnemy()
    {
        closestEnemy = null;
        float closestDistSqr = Mathf.Infinity;

        // Для Unity 6 лучше использовать FindObjectsByType
        EnemyDarya[] enemies = FindObjectsByType<EnemyDarya>(FindObjectsSortMode.None);

        foreach (EnemyDarya enemy in enemies)
        {
            // Используем sqrMagnitude для оптимизации (избегаем корня)
            float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;

            if (distSqr <= fireDistance * fireDistance && distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestEnemy = enemy.transform;
            }
        }
    }

    void AimAtEnemy()
    {
        if (closestEnemy != null)
        {
            Vector3 direction = closestEnemy.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Если нет врагов, можно вернуть в исходное положение или оставить как есть
            // transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
    }

    void Shoot()
    {
        // Создаем эффект дульной вспышки
        if (muzzleEffect != null && muzzlePoint != null)
        {
            var muzzleGo = Instantiate(muzzleEffect, muzzlePoint.position, transform.rotation);
            Destroy(muzzleGo, 0.1f);
        }

        // Создаем снаряд
        if (projectilePrefab != null && muzzlePoint != null)
        {
            var projectileGo = Instantiate(projectilePrefab, muzzlePoint.position, transform.rotation);

            Rigidbody2D rb = projectileGo.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Важно: используем bulletSpeed, а не fireDistance (fireDistance - это радиус обзора)
                rb.linearVelocity = transform.right * bulletSpeed;
            }

            Destroy(projectileGo, 3f);
        }

        // Анимация выстрела
        if (anim != null)
        {
            anim.SetTrigger("Fire");
        }
    }
}
