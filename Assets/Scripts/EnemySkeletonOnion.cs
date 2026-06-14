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

    [Header("Death Effect")]
    [Tooltip("Скорость мигания при смерти")]
    public float flashSpeed = 8f;
    [Tooltip("Количество миганий перед исчезновением")]
    public int flashCount = 6;
    [Tooltip("Время перед началом исчезновения")]
    public float deathDelay = 0.5f;
    [Tooltip("Цвет свечения при смерти")]
    public Color glowColor = new Color(1f, 0.9f, 0.7f, 1f);

    [Header("References")]
    public Transform player;

    private int currentHealth;
    private float lastDamageTime = -999f;
    private bool isDead = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || rb == null) return;

        // Движение к игроку
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Поворот лицом к игроку
        if (direction.x != 0)
        {
            float scaleX = transform.localScale.x;
            if ((direction.x > 0 && scaleX < 0) || (direction.x < 0 && scaleX > 0))
            {
                transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
            }
        }

        // Обновление анимации
        if (anim != null)
        {
            anim.SetFloat("moveSpeed", Mathf.Abs(direction.magnitude));
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || Time.time - lastDamageTime < damageCooldown)
            return;

        currentHealth -= amount;
        lastDamageTime = Time.time;

        Debug.Log($"SkeletonOnionEnemy took {amount} damage. Health: {currentHealth}/{maxHealth}");

        // Визуальный эффект получения урона
        if (spriteRenderer != null && !isDead)
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        // Отключаем коллайдер и физику
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Возвращаемся в Idle анимацию
        if (anim != null)
        {
            anim.SetFloat("moveSpeed", 0);
            // Убедимся, что триггеры сброшены
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Run");
        }

        // Запускаем корутину смерти
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Ждем немного перед началом эффекта
        yield return new WaitForSeconds(deathDelay);

        // Эффект мигания/свечения
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        float flashDuration = 1f / flashSpeed;

        for (int i = 0; i < flashCount; i++)
        {
            if (spriteRenderer != null)
            {
                // Свечение
                spriteRenderer.color = glowColor;
                yield return new WaitForSeconds(flashDuration);

                // Исчезновение
                spriteRenderer.color = Color.clear;
                yield return new WaitForSeconds(flashDuration);
            }
        }

        // Финальное исчезновение
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        // Уничтожаем объект
        Destroy(gameObject);
    }

    void ResetColor()
    {
        if (spriteRenderer != null && !isDead)
            spriteRenderer.color = Color.white;
    }

    // Визуальный эффект в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
