using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Настройки еды")]
    [SerializeField] private int healthRestore = 20; // Количество восстанавливаемого здоровья
    [SerializeField] private bool destroyOnConsume = true; // Уничтожать ли еду после использования

    [Header("Визуальные эффекты")]
    [SerializeField] private ParticleSystem healEffect; // Эффект при подборе (опционально)
    [SerializeField] private AudioClip pickupSound; // Звук при подборе (опционально)

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        PlayerDarya player = other.GetComponentInParent<PlayerDarya>();

        if (player != null)
        {
            ConsumeFood(player);
        }
    }

    private void ConsumeFood(PlayerDarya player)
    {
        // Воспроизводим эффект лечения
        PlayHealEffects();

        // Увеличиваем здоровье игрока
        player.AddHealth(healthRestore);

        // Уничтожаем еду если нужно
        if (destroyOnConsume)
        {
            Destroy(gameObject);
        }
    }

    private void PlayHealEffects()
    {
        // Воспроизводим визуальный эффект
        if (healEffect != null)
        {
            Instantiate(healEffect, transform.position, Quaternion.identity);
        }

        // Воспроизводим звук
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    // Метод для настройки количества восстанавливаемого здоровья извне
    public void SetHealthRestore(int amount)
    {
        healthRestore = Mathf.Max(1, amount); // Минимум 1 единица здоровья
    }
}
