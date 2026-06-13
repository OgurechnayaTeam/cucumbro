using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Основной скрипт управления игроком (Дарья).
/// Отвечает за движение, здоровье, урон и активные баффы (щит, усиление).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDarya : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Настройки урона")]
    [Tooltip("Базовый урон игрока без баффов")]
    [SerializeField] private float baseDamage = 10f;

    [Tooltip("Текущий множитель урона (изменяется баффами)")]
    [SerializeField] private float damageMultiplier = 1f;

    [Tooltip("Длительность баффа урона по умолчанию")]
    [SerializeField] private float defaultBuffDuration = 10f;

    [Header("Настройки щита")]
    [Tooltip("Статус активности щита (автоматически управляется системой)")]
    [SerializeField] private bool isShieldActive = false;

    [Tooltip("Длительность щита по умолчанию")]
    [SerializeField] private float defaultShieldDuration = 5f;

    // Приватные переменные состояния
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector3 startingScale;
    private int facingDirection = 1;

    #region Public Properties
    /// <summary> Итоговый урон с учетом всех модификаторов </summary>
    public float CurrentDamage => baseDamage * damageMultiplier;

    /// <summary> Базовый урон (без баффов) </summary>
    public float BaseDamage => baseDamage;

    /// <summary> Текущий множитель урона </summary>
    public float DamageMultiplier => damageMultiplier;

    /// <summary> Активен ли сейчас щит неуязвимости </summary>
    public bool IsShieldActive => isShieldActive;

    /// <summary> Текущее здоровье </summary>
    public int GetCurrentHealth() => currentHealth;

    /// <summary> Максимальное здоровье </summary>
    public int GetMaxHealth() => maxHealth;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // Чтение ввода
        if (moveAction?.action != null)
            movement = moveAction.action.ReadValue<Vector2>();

        // Разворот персонажа при движении
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
        transform.localScale = new Vector3(
            Mathf.Abs(startingScale.x) * facingDirection,
            startingScale.y,
            startingScale.z
        );
    }

    #region Health Methods

    public void AddHealth(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[HP] Восстановлено {amount}. Текущее: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        // Щит полностью блокирует урон
        if (isShieldActive)
        {
            Debug.Log("[Shield] Урон заблокирован щитом!");
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        Debug.Log($"[HP] Получен урон {damage}. Текущее: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("[Player] Игрок погиб!");

    }

    #endregion


    #region Shield Methods

    /// <summary>
    /// Вызывается из ShieldPickup для активации защиты
    /// </summary>
    public void ActivateShield(float duration = 0f)
    {
        if (duration <= 0)
            duration = defaultShieldDuration;

        isShieldActive = true;

        // Сброс предыдущего таймера для корректного продления эффекта
        CancelInvoke(nameof(DeactivateShield));
        Invoke(nameof(DeactivateShield), duration);

        Debug.Log($"[Shield] Активирован на {duration:F1}с!");
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        Debug.Log("[Shield] Действие закончилось.");
    }

    #endregion


    #region Damage Methods

    /// <summary>
    /// Вызывается из DamageBoostPickup
    /// </summary>
    public void ApplyDamageBoost(float multiplier, float duration = 0f)
    {
        if (duration <= 0)
            duration = defaultBuffDuration;

        damageMultiplier = multiplier;

        Debug.Log($"[Buff] Урон x{multiplier:F2} на {duration:F1}с!");

        if (duration > 0)
        {
            CancelInvoke(nameof(ResetDamageBoost));
            Invoke(nameof(ResetDamageBoost), duration);
        }
    }

    private void ResetDamageBoost()
    {
        damageMultiplier = 1f;
        Debug.Log("[Buff] Множитель урона сброшен.");
    }

    /// <summary> Для чит-кодов или тестов </summary>
    public void SetPermanentDamageMultiplier(float multiplier)
    {
        CancelInvoke(nameof(ResetDamageBoost));
        damageMultiplier = multiplier;
        Debug.Log($"[Buff] Постоянный множитель: x{multiplier:F2}");
    }

    #endregion
}
