using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnn : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionReference moveAction;

    [SerializeField] private float animationSpeed = 0.5f; // Добавлен параметр для управления скоростью анимации (0.5 = 50% скорости)

    [SerializeField] private GameObject threeQuarterSprite; // SPTThreeOnFour и все его части
    [SerializeField] private GameObject frontSprite;       // SPFront и все его части
    [SerializeField] private GameObject backSprite;        // SPBack и все его части


    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator anim;
    private Vector3 threeQuarterOriginalScale;
    private int facingDirection = 1;
    private bool dead = false;

    private enum MovementDirection
    {
        Forward,     // Вниз (SPFront)
        Back,        // Вверх (SPBack)
        ThreeQuarter // Влево/вправо (SPTThreeOnFour)
    }

    private MovementDirection currentDirection = MovementDirection.ThreeQuarter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();
        if (threeQuarterSprite != null)
            threeQuarterOriginalScale = threeQuarterSprite.transform.localScale;

        if (anim != null)
            anim.speed = animationSpeed;
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Start()
    {
        // По умолчанию показываем только 3/4 группу
        UpdateSpriteVisibility();
    }

    private void Update()
    {
        if (dead)
        {
            movement = Vector2.zero;
            if (anim != null)
                anim.SetFloat("moveSpeed", 0);
            return;
        }

        // Получаем ввод
        if (moveAction != null)
        {
            movement = Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
        }
        else
        {
            // Fallback на старую систему ввода
            float moveHorizontal = Input.GetAxisRaw("Horizontal");
            float moveVertical = Input.GetAxisRaw("Vertical");
            movement = new Vector2(moveHorizontal, moveVertical).normalized;
        }

        if (anim != null)
            anim.SetFloat("moveSpeed", movement.magnitude);

        // Обновляем направление движения
        UpdateMovementDirection();

        // Обновляем горизонтальное направление для 3/4 спрайта
        if (Mathf.Abs(movement.x) > 0.01f)
        {
            facingDirection = movement.x > 0 ? 1 : -1;

            // Поворачиваем весь threeQuarterSprite, если он активен
            if (currentDirection == MovementDirection.ThreeQuarter && threeQuarterSprite != null)
            {
                FlipThreeQuarterSprite();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
            rb.linearVelocity = movement * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void UpdateMovementDirection()
    {
        MovementDirection newDirection = currentDirection;

        // Определяем направление на основе вектора движения
        if (movement.magnitude > 0.01f) // Если персонаж движется
        {
            Vector2 normalizedMovement = movement.normalized;

            // Определяем направление по вертикали
            if (normalizedMovement.y < -0.5f) // Движение вниз
                newDirection = MovementDirection.Forward;
            else if (normalizedMovement.y > 0.5f) // Движение вверх
                newDirection = MovementDirection.Back;
            else // Движение влево/вправо или диагональ
                newDirection = MovementDirection.ThreeQuarter;
        }

        // Если направление изменилось, обновляем видимость спрайтов
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            UpdateSpriteVisibility();

            // При переключении на 3/4 спрайт применяем текущее направление поворота
            if (currentDirection == MovementDirection.ThreeQuarter && threeQuarterSprite != null)
            {
                FlipThreeQuarterSprite();
            }
        }
    }

    private void FlipThreeQuarterSprite()
    {
        // Поворачиваем весь объект threeQuarterSprite по горизонтали
        Vector3 newScale = threeQuarterOriginalScale;
        newScale.x = Mathf.Abs(threeQuarterOriginalScale.x) * facingDirection;
        threeQuarterSprite.transform.localScale = newScale;
    }

    private void UpdateSpriteVisibility()
    {
        // Отключаем все родительские объекты спрайтов
        if (threeQuarterSprite != null) threeQuarterSprite.SetActive(false);
        if (frontSprite != null) frontSprite.SetActive(false);
        if (backSprite != null) backSprite.SetActive(false);

        // Включаем только нужный родительский объект
        // Дочерние элементы (части тела) активируются автоматически вместе с родителем
        switch (currentDirection)
        {
            case MovementDirection.Forward:
                if (frontSprite != null) frontSprite.SetActive(true);
                break;
            case MovementDirection.Back:
                if (backSprite != null) backSprite.SetActive(true);
                break;
            case MovementDirection.ThreeQuarter:
                if (threeQuarterSprite != null)
                {
                    threeQuarterSprite.SetActive(true);
                    // Применяем поворот при активации
                    FlipThreeQuarterSprite();
                }
                break;
        }
    }

    // Публичные методы
    public void SetDead(bool isDead)
    {
        dead = isDead;
        if (dead)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            // ИСПРАВЛЕНО: останавливаем анимацию при смерти
            if (anim != null) anim.SetFloat("moveSpeed", 0);
        }
    }

    public bool IsDead()
    {
        return dead;
    }

    public Vector2 GetMovement()
    {
        return movement;
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }
}
