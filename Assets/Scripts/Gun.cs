using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private GameObject muzzle;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private GameObject projectile;

    [Header("Config")]
    [SerializeField] private float fireDistance = 10f;
    [SerializeField] private float fireRate = 0.5f;

    [Header("Positioning")]
    [SerializeField] private Vector2 defaultOffset = new Vector2(1f, 0f);

    // Приватные поля
    private Vector2 currentOffset;
    private float timeSinceLastShot;
    private Transform closestEnemy;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Инициализация компонентов
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("[Gun] SpriteRenderer not found! Disabling script.");
            enabled = false;
            return;
        }

        // Проверка обязательной ссылки на игрока
        if (playerTransform == null)
        {
            Debug.LogError("[Gun] Player Transform is not assigned in Inspector! Please drag the Player object here.");
            enabled = false;
            return;
        }

        // Поиск точки выстрела
        if (muzzlePosition == null)
        {
            muzzlePosition = transform.Find("MuzzlePosition");
            if (muzzlePosition == null)
            {
                Debug.LogError("[Gun] MuzzlePosition not found! Create a child object named 'MuzzlePosition' at the gun's tip.");
                enabled = false;
                return;
            }
        }

        // Начальные настройки
        timeSinceLastShot = fireRate;
        currentOffset = defaultOffset;

        Debug.Log($"[Gun] Started successfully. Attached to player at offset: {currentOffset}");
    }

    private void Update()
    {
        if (playerTransform == null || !enabled) return;

        // Прикрепляем пушку к игроку с заданным смещением
        transform.position = (Vector2)playerTransform.position + currentOffset;

        // Ищем ближайшего врага в радиусе действия
        FindClosestEnemy();

        // Если есть цель — целимся и стреляем
        if (closestEnemy != null && Vector2.Distance(transform.position, closestEnemy.position) <= fireDistance)
        {
            AimAtEnemy();
            Shooting();
        }
        else
        {
            ResetToIdle();
        }
    }

    /// <summary>
    /// Находит ближайшего врага с тегом "Enemy" в радиусе fireDistance
    /// </summary>
    private void FindClosestEnemy()
    {
        closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= fireDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
    }

    /// <summary>
    /// Поворачивает пушку в сторону ближайшего врага
    /// </summary>
    private void AimAtEnemy()
    {
        if (closestEnemy == null) return;

        Vector2 direction = (closestEnemy.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Возвращает пушку в исходное положение (смотрит вправо)
    /// </summary>
    private void ResetToIdle()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Если используется аниматор — сбрасываем состояние выстрела
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
        {
            anim.Play("Idle");
        }
    }

    /// <summary>
    /// Логика стрельбы с учётом скорострельности
    /// </summary>
    private void Shooting()
    {
        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= fireRate)
        {
            Shoot();
            timeSinceLastShot = 0f;
        }
    }

    /// <summary>
    /// Создаёт визуальные эффекты и снаряд
    /// </summary>
    private void Shoot()
    {
        // Запуск анимации выстрела
        if (anim != null)
            anim.SetTrigger("Fire");

        // Создание эффекта дульного пламени
        if (muzzle != null && muzzlePosition != null)
        {
            GameObject muzzleEffect = Instantiate(muzzle, muzzlePosition.position, transform.rotation);
            muzzleEffect.transform.SetParent(transform);
            Destroy(muzzleEffect, 0.05f);
        }

        // Создание снаряда
        if (projectile != null && muzzlePosition != null)
        {
            GameObject bullet = Instantiate(projectile, muzzlePosition.position, transform.rotation);
            Destroy(bullet, 3f);
        }
    }

    /// <summary>
    /// Устанавливает новое смещение пушки относительно игрока
    /// </summary>
    public void SetOffset(Vector2 newOffset)
    {
        currentOffset = newOffset;
        if (playerTransform != null)
            transform.position = (Vector2)playerTransform.position + currentOffset;
    }

    /// <summary>
    /// Возвращает текущее смещение
    /// </summary>
    public Vector2 GetOffset()
    {
        return currentOffset;
    }
}