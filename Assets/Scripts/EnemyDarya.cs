using UnityEngine;

public class EnemyDarya : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 2f;
    public int maxHealth = 3;       // Увеличьте это значение в Инспекторе для теста
    public float damageCooldown = 0.2f; // Снизил кулдаун, чтобы пули успевали наносить урон

    [Header("References")]
    public Transform player;

    private int currentHealth;
    private float lastDamageTime = -999f;
    private bool isDead = false;
    private Rigidbody2D rb;         // Обязательно кэшируем Rigidbody

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }


    void FixedUpdate()
    {
        if (isDead || player == null || rb == null) return;

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
