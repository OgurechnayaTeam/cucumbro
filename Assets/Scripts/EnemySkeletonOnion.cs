using System.Collections;
using UnityEngine;

public class EnemySkeletonOnionEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float noticeRange = 5f;
    public float attackRange = 1.5f;

    [Header("Combat")]
    public int maxHealth = 50;
    public int currentHealth;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    [Header("Death Effect")]
    public float deathFadeDuration = 1.5f;
    public float hitFlashDuration = 0.15f;

    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDead = false;

    void Awake()
    {
        // Добавляем только Rigidbody2D (без коллайдера!)
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Kinematic; // KINEMATIC - лучший вариант для простого движения
            Debug.Log("Added Rigidbody2D (Kinematic)");
        }
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Анимации
        if (animator != null)
        {
            animator.SetFloat("distanceToPlayer", distance);
            bool notice = distance <= noticeRange;
            animator.SetBool("isNotice", notice);
        }

        // Атака
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking && !isDead)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool notice = distance <= noticeRange;

        if (notice && !isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // ДЛЯ KINEMATIC Rigidbody2D:
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // ИЛИ для DYNAMIC Rigidbody2D (раскомментировать):
            // rb.linearVelocity = direction * moveSpeed;

            // Поворот спрайта
            if (direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (direction.x > 0 ? 1 : -1);
                transform.localScale = scale;
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        if (animator != null) animator.SetBool("isAttack", true);
        lastAttackTime = Time.time;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);

        // Урон игроку
        //if (player != null)
        //{
        //    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //    if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
        //}

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        if (animator != null) animator.SetBool("isAttack", false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = original;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null) rb.simulated = false;
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / deathFadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
