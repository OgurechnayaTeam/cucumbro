using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 3f;

    // Ссылка на того, кто выстрелил (чтобы не убить себя)
    private Transform shooterTransform;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // Запоминаем, кто выстрелил (родитель пули - это Gun, а его родитель - Player)
        shooterTransform = transform.parent?.parent;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ИГНОРИРУЕМ ТРИГГЕРЫ (иначе пуля может застрять в зоне обнаружения)
        if (collision.collider.isTrigger) return;

        // 2. НЕ СТРЕЛЯЕМ В САМИХ СЕБЯ
        if (collision.transform.root == shooterTransform) return;

        // 3. НАНОСИМ УРОН ВРАГУ
        EnemyDarya enemy = collision.gameObject.GetComponent<EnemyDarya>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // 4. УНИЧТОЖАЕМ ПУЛЮ ПРИ ПОПАДАНИИ В ЛЮБОЙ ТВЕРДЫЙ ОБЪЕКТ
        Destroy(gameObject);
    }
}
