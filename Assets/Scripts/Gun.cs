using UnityEngine;

public class Gun : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField] GameObject muzzle;
    [SerializeField] Transform muzzlePosition;
    [SerializeField] GameObject projectile;

    [Header("Config")]
    [SerializeField] float fireDistance = 10;
    [SerializeField] float fireRate = 0.5f;

    Transform player;
    Vector2 offset;

    private float timeSinceLastShot = 0f;
    Transform closestEnemy;
    Animator anim;


    private void Start()
    {
        anim = GetComponent<Animator>();
        timeSinceLastShot = fireRate;
        player = GameObject.Find("Player").transform;

        SetOffset(new Vector2(1, 0.5f));

    }

    private void Update()
    {

        transform.localPosition = offset;

        FindClosestEnemy();
        AimAtEnemy();
        Shooting();
    }

    void Shooting()
    {
        // Если врага нет или он слишком далеко — выходим
        if (closestEnemy == null || Vector2.Distance(transform.position, closestEnemy.position) > fireDistance)
            return;

        // Таймер перезарядки
        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= fireRate)
        {
            Shoot(); 
            timeSinceLastShot = 0f;
        }
    }

    void FindClosestEnemy()
    {
        closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        EnemyDarya[] enemies = FindObjectsOfType<EnemyDarya>();

        foreach (EnemyDarya enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= fireDistance)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }
    }

    void AimAtEnemy()
    {
        if (closestEnemy != null)
        {
            Vector3 direction = closestEnemy.position - transform.position;
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }


    void Shoot()
    {
        // Эффект вспышки
        var muzzleGo = Instantiate(muzzle, muzzlePosition.position, transform.rotation);
        muzzleGo.transform.SetParent(transform);
        Destroy(muzzleGo, 0.1f);

        // Создание снаряда
        var projectileGo = Instantiate(projectile, muzzlePosition.position, transform.rotation);

        Rigidbody2D bulletRb = projectileGo.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.right * fireDistance;
        }

        Destroy(projectileGo, 3f);

        if (anim != null)
        {
            anim.SetTrigger("Fire"); 
        }
    }

    public void SetOffset(Vector2 o)
    {
        offset = o;
    }

}
