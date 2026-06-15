using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDarya : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 2f;
    public int maxHealth = 3;       // Увеличьте это значение в Инспекторе для теста
    public float damageCooldown = 0.2f; // Снизил кулдаун, чтобы пули успевали наносить урон
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float contactDamageCooldown = 1f;

    [Header("References")]
    public Transform player;

    private int currentHealth;
    private float lastDamageTime = -999f;
    private float lastPlayerDamageTime = -999f;
    private bool isDead = false;
    private bool canMove = true;
    private Rigidbody2D rb;         // Обязательно кэшируем Rigidbody

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null)
            enemyCollider = gameObject.AddComponent<BoxCollider2D>();

        enemyCollider.isTrigger = false;

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    // ВАЖНО: Движение перенесено в FixedUpdate для корректной физики
    void FixedUpdate()
    {
        if (isDead || player == null || rb == null || !canMove)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        // Используем физику вместо transform.position
        rb.linearVelocity = direction * moveSpeed;

        // Поворот спрайта
        if (direction.x != 0)
        {
            float scaleX = transform.localScale.x;
            if ((direction.x > 0 && scaleX < 0) || (direction.x < 0 && scaleX > 0))
                transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!canMove && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        // Проверка кулдауна
        if (Time.time - lastDamageTime < damageCooldown) return;

        currentHealth -= amount;
        lastDamageTime = Time.time;

        Debug.Log($"[EnemyDarya] HP: {currentHealth}/{maxHealth}");

        // Визуальный отклик
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (currentHealth <= 0) Die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || contactDamage <= 0 || Time.time - lastPlayerDamageTime < contactDamageCooldown)
            return;

        PlayerDarya playerDarya = other.GetComponentInParent<PlayerDarya>();
        if (playerDarya == null)
            return;

        playerDarya.TakeDamage(contactDamage);
        lastPlayerDamageTime = Time.time;
    }

    void Die()
    {
        isDead = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false; // Отключаем коллайдер сразу

        Debug.Log("[EnemyDarya] DIED!");
        Destroy(gameObject, 0.1f);
    }

    void ResetColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }
}
