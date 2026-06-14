using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemySkeletonOnionEnemy : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 2f;
    public int maxHealth = 3;
    public float damageCooldown = 0.5f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    [Header("Death Effect")]
    public float flashSpeed = 6f;
    public int flashCount = 5;
    public float deathDelay = 0.3f;
    public Color glowColor = new Color(1f, 0.95f, 0.8f, 1f);

    [Header("References")]
    public Transform player;
    public LayerMask playerLayer;

    private int currentHealth;
    private float lastDamageTime = -999f;
    private float lastAttackTime = -999f;
    private bool isDead = false;
    private bool isNoticed = false;
    private bool isAttacking = false;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private float distanceToPlayer;
    private Color originalColor;
    private Coroutine damageFlashCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        // ОТКЛЮЧАЕМ ГРАВИТАЦИЮ чтобы враг не падал
        rb.gravityScale = 0;

        // Опционально: замораживаем вращение
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Сохраняем оригинальный цвет
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
                playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || rb == null) return;

        // Вычисляем расстояние до игрока
        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Проверяем, видит ли игрок врага
        isNoticed = distanceToPlayer <= detectionRange;

        if (isNoticed)
        {
            // Движение к игроку
            Vector2 direction = (player.position - transform.position).normalized;

            // Проверяем, можно ли атаковать
            if (distanceToPlayer <= attackRange)
            {
                // Останавливаем движение когда близко к игроку
                rb.linearVelocity = Vector2.zero;

                // Атакуем с кулдауном
                if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
                {
                    StartCoroutine(AttackSequence());
                }
            }
            else
            {
                // Движение к игроку
                isAttacking = false;
                rb.linearVelocity = direction * moveSpeed;

                // Поворот лицом к игроку (только если направление значимое)
                if (Mathf.Abs(direction.x) > 0.1f) // Порог для предотвращения дрожания
                {
                    float scaleX = transform.localScale.x;
                    if ((direction.x > 0 && scaleX < 0) || (direction.x < 0 && scaleX > 0))
                    {
                        transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
                    }
                }
            }
        }
        else
        {
            // Игрок вне зоны видимости
            isAttacking = false;
            rb.linearVelocity = Vector2.zero;
        }

        // Обновляем параметры анимации
        UpdateAnimatorParameters();
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Наносим урон игроку
        AttackPlayer();

        // Ждем пока анимация не пройдет до конца
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        float startTime = stateInfo.normalizedTime;

        while (stateInfo.normalizedTime < 1.0f && stateInfo.IsName("SkeletonOnionEnemyAttack"))
        {
            yield return null;
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }

        // Ждем еще немного после завершения анимации
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    void UpdateAnimatorParameters()
    {
        if (anim == null) return;

        // Устанавливаем параметры для Animator
        anim.SetFloat("moveSpeed", isNoticed && !isAttacking ? 1f : 0f);
        anim.SetBool("isNotice", isNoticed);
        anim.SetBool("isAttack", isAttacking);
        anim.SetFloat("distanceToPlayer", distanceToPlayer);
    }

    void AttackPlayer()
    {
        if (player == null) return;

        // Ищем компонент PlayerDarya на игроке
        PlayerDarya playerDarya = player.GetComponent<PlayerDarya>();
        if (playerDarya != null)
        {
            playerDarya.TakeDamage(attackDamage);
            Debug.Log($"SkeletonOnionEnemy attacked Player for {attackDamage} damage!");
        }

        // Альтернативно ищем другие скрипты игрока
        if (playerDarya == null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                // Если у Player нет метода TakeDamage, можно добавить
                Debug.Log($"SkeletonOnionEnemy attacked Player!");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || Time.time - lastDamageTime < damageCooldown)
            return;

        currentHealth -= amount;
        lastDamageTime = Time.time;

        Debug.Log($"[SkeletonOnionEnemy] HP: {currentHealth}/{maxHealth}");

        // Визуальный эффект получения урона - КРАСНЫЙ НА 0.5 СЕКУНДЫ
        if (spriteRenderer != null && !isDead)
        {
            // Останавливаем предыдущую корутину если она запущена
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);

            damageFlashCoroutine = StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        // Устанавливаем красный цвет
        spriteRenderer.color = Color.red;

        // Ждем 0.5 секунды
        yield return new WaitForSeconds(0.5f);

        // Возвращаем оригинальный цвет
        spriteRenderer.color = originalColor;

        damageFlashCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[SkeletonOnionEnemy] DIED!");

        // Отключаем коллайдер и физику
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Останавливаем все анимации и возвращаемся в Idle
        if (anim != null)
        {
            anim.SetBool("isAttack", false);
            anim.SetBool("isNotice", false);
            anim.SetFloat("moveSpeed", 0);
            anim.SetFloat("distanceToPlayer", 999);
        }

        // Запускаем корутину смерти
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Небольшая пауза перед началом эффекта
        yield return new WaitForSeconds(deathDelay);

        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        float flashDuration = 1f / flashSpeed;

        // Эффект свечения и исчезновения
        for (int i = 0; i < flashCount; i++)
        {
            if (spriteRenderer != null)
            {
                // Свечение золотистым
                spriteRenderer.color = glowColor;
                yield return new WaitForSeconds(flashDuration);

                // Полное исчезновение
                spriteRenderer.color = Color.clear;
                yield return new WaitForSeconds(flashDuration);
            }
        }

        // Финальное исчезновение
        Destroy(gameObject);
    }

    // Отрисовка радиусов в редакторе
    private void OnDrawGizmosSelected()
    {
        // Радиус обнаружения
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Радиус атаки
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
