using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f; // Скорость полёта
    [SerializeField] float lifetime = 3f; // Время жизни

    private Vector2 direction;

    void Start()
    {
        // Удаляем пулю через lifetime секунд
        Destroy(gameObject, lifetime);

        // Направление берём из текущего поворота объекта
        direction = transform.right; // Если пуля смотрит вправо по умолчанию
    }

    void Update()
    {
        // Двигаем пулю в направлении её поворота
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // Если нужно наносить урон при столкновении
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy!");
            // Здесь можно вызвать метод нанесения урона у врага
            // other.GetComponent<Enemy>().TakeDamage(10);
            Destroy(gameObject); // Уничтожаем пулю при попадании
        }
    }
}