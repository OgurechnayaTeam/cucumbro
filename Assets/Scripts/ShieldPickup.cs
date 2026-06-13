using UnityEngine;

/// <summary>
/// Предмет, дающий временную неуязвимость (щит)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShieldPickup : MonoBehaviour
{
    [Header("Настройки щита")]
    [Tooltip("Длительность действия щита в секундах")]
    [SerializeField] private float shieldDuration = 5f;

    [Header("Визуальные эффекты")]
    [Tooltip("Цвет свечения (обычно синий или голубой)")]
    [SerializeField] private Color glowColor = new Color(0.2f, 0.6f, 1f, 1f);

    [Tooltip("Эффект частиц при подборе")]
    [SerializeField] private GameObject pickupEffect;

    private SpriteRenderer spriteRenderer;
    private Vector3 fixedPosition;
    private float startY;
    private float baseRotationZ;

    [Header("Анимация")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float swingSpeed = 2.5f;
    [SerializeField] private float swingAngle = 10f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
        }
    }

    private void Start()
    {
        fixedPosition = transform.position;
        startY = transform.position.y;
        baseRotationZ = transform.eulerAngles.z;

        if (spriteRenderer != null)
            spriteRenderer.color = glowColor;
    }

    private void Update()
    {
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(fixedPosition.x, newY, fixedPosition.z);

        float currentSwing = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseRotationZ + currentSwing);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ищем скрипт игрока напрямую через коллайдер
        PlayerDarya player = other.GetComponent<PlayerDarya>();

        // Если на объекте нет скрипта PlayerDarya — выходим
        if (player == null) return;

        // Активируем щит
        player.ActivateShield(shieldDuration);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
        Debug.Log($"[ShieldPickup] Щит подобран! Неуязвимость на {shieldDuration:F1}с");
    }
}
