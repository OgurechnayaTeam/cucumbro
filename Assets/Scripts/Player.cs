using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionReference moveAction;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector3 startingScale;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        startingScale = transform.localScale;
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

    private void Update()
    {
        if (moveAction != null)
            movement = Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);

        if (Mathf.Abs(movement.x) > 0.01f)
        {
            facingDirection = movement.x > 0 ? 1 : -1;
            UpdateFacingDirection();
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
            startingScale.z);
    }
}
