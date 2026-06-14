using UnityEngine;

/// <summary>
/// Префаб для увеличения урона игрока.
/// Подбираемый предмет, дающий временный бафф на урон.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageBoostPickup : MonoBehaviour
{
    [Header("Настройки баффа")]
    [Tooltip("Множитель урона (1.5 = +50% урона, 2.0 = удвоенный урон)")]
    [SerializeField] private float damageMultiplier = 1.5f;

    [Tooltip("Длительность эффекта в секундах (0 = использовать значение по умолчанию из PlayerDarya)")]
    [SerializeField] private float duration = 0f;

    [Header("Визуальные эффекты")]
    [Tooltip("Цвет свечения предмета")]
    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0f, 1f);

    [Tooltip("Эффект частиц при подборе")]
    [SerializeField] private GameObject pickupEffect;

    [Tooltip("Звук при подборе")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Анимация")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float swingSpeed = 3f;
    [SerializeField] private float swingAngle = 15f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector3 fixedPosition;
    private float startY;
    private float baseRotationZ;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

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
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = glowColor;
    }

    private void Update()
    {
        AnimatePickup();
    }

    private void AnimatePickup()
    {
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(fixedPosition.x, newY, fixedPosition.z);

        float currentSwing = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseRotationZ + currentSwing);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ПРЯМОЙ ПОИСК СКРИПТА ИГРОКА (без проверки тега)
        PlayerDarya player = other.GetComponent<PlayerDarya>();
        if (player == null) return;

        player.ApplyDamageBoost(damageMultiplier, duration);
        PlayPickupEffects();
        Destroy(gameObject);

        Debug.Log($"[DamageBoostPickup] Подобран бафф урона x{damageMultiplier:F2}!");
    }

    private void PlayPickupEffects()
    {
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = glowColor;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds bounds = col.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
