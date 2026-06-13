using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 2f;
    public int maxHealth = 3;
    public float damageCooldown = 1f; // время неуязвимости после получения урона

    [Header("References")]
    public Transform player; // можно назначить вручную или найти автоматически

    private int currentHealth;
    private float lastDamageTime = -999f; // чтобы первый удар прошёл сразу
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Если игрок не назначен — найдём его по имени
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("Player not found! Assign it manually or ensure object is named 'Player'.");
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Движение к игроку
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // Поворот лицом к игроку (опционально)
        if (direction.x != 0)
        {
            float scaleX = transform.localScale.x;
            if ((direction.x > 0 && scaleX < 0) || (direction.x < 0 && scaleX > 0))
            {
                transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    /// <summary>
    /// Получить урон от пули/игрока
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead || Time.time - lastDamageTime < damageCooldown)
            return;

        currentHealth -= amount;
        lastDamageTime = Time.time;

        Debug.Log($"Enemy took {amount} damage. Health: {currentHealth}/{maxHealth}");

        // Визуальный эффект попадания (можно добавить позже)
        // GetComponent<SpriteRenderer>().color = Color.red;
        // Invoke(nameof(ResetColor), 0.1f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Enemy died!");

        // Опционально: анимация смерти, звук, дроп предметов
        // Destroy(gameObject, 0.5f); // удалить через полсекунды (для анимации)
        Destroy(gameObject); // удалить сразу
    }

    // Для визуального эффекта попадания (раскомментируйте, если нужно)
    /*
    void ResetColor()
    {
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().color = Color.white;
    }
    */
}