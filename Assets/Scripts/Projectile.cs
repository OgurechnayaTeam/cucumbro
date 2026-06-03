using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10; // Урон пули
    [SerializeField] private float lifetime = 3f; // Время жизни

    void Start()
    {
        // Автоматическое удаление, если пуля никуда не попала
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем наличие скрипта врага напрямую, а не по тегу
        EnemyDarya enemy = collision.gameObject.GetComponent<EnemyDarya>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Hit Enemy! Damage: {damage}");
        }

        // Уничтожаем пулю при любом столкновении (враг или стена)
        Destroy(gameObject);
    }
}
