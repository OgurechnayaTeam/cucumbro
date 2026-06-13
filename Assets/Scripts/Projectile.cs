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

        if (collision.collider.isTrigger) return;

        if (collision.transform.root == shooterTransform) return;


        EnemyDarya enemy = collision.gameObject.GetComponent<EnemyDarya>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }


        Destroy(gameObject);
    }
}
