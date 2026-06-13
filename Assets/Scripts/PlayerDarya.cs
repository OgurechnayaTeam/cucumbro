using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDarya : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector3 startingScale;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Запоминаем текущий масштаб ИЗ РЕДАКТОРА
        startingScale = transform.localScale;

        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        if (moveAction?.action != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction?.action != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        // Безопасное чтение ввода
        if (moveAction?.action != null)
        {
            movement = moveAction.action.ReadValue<Vector2>();
        }

        // Разворот только если есть движение
        if (Mathf.Abs(movement.x) > 0.01f)
        {
            int newDirection = movement.x > 0 ? 1 : -1;
            if (newDirection != facingDirection)
            {
                facingDirection = newDirection;
                UpdateFacingDirection();
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    private void UpdateFacingDirection()
    {
        // Сохраняем оригинальный масштаб, меняем только знак X
        transform.localScale = new Vector3(
            Mathf.Abs(startingScale.x) * facingDirection,
            startingScale.y,
            startingScale.z
        );
    }

    public void AddHealth(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Здоровье восстановлено на {amount}. Текущее: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        Debug.Log($"Получен урон {damage}. Текущее: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Игрок погиб!");
        // TODO: Анимация смерти, перезагрузка сцены
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
