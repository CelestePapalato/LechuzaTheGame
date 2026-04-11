using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(RaycastCollision))]
public class PlayerController : MonoBehaviour
{
    [Header("Walk")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private bool instantHorizontalMove;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpTimeToPeak = 0.36f;
    [SerializeField] private float jumpTimeToDescent = 0.35f;
    [SerializeField] private float maxFallSpeed = 22f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.12f;

    [Header("Impulse")]
    [SerializeField] private float impulseDecay = 12f;

    private Rigidbody2D rb;
    private RaycastCollision collision;

    private Vector2 walkVelocity;
    private Vector2 dashVelocity;
    private Vector2 impulseVelocity;

    private float dashTimeLeft;
    private float facing = 1f;

    private float inputHorizontal;
    private bool jumpQueued;
    private bool dashQueued;

    public void AddImpulse(Vector2 worldDeltaVelocity)
    {
        impulseVelocity += worldDeltaVelocity;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<RaycastCollision>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputHorizontal) > 0.01f)
            facing = Mathf.Sign(inputHorizontal);

        if (Input.GetButtonDown("Jump"))
            jumpQueued = true;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            dashQueued = true;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        bool grounded = collision.IsGrounded;

        if (impulseVelocity.sqrMagnitude > 1e-8f)
            impulseVelocity *= Mathf.Clamp01(1f - impulseDecay * dt);

        if (dashTimeLeft > 0f)
        {
            dashTimeLeft -= dt;
            dashVelocity = new Vector2(facing * dashSpeed, 0f);
            if (dashTimeLeft <= 0f)
                dashVelocity = Vector2.zero;
        }

        if (dashQueued && dashTimeLeft <= 0f)
        {
            dashTimeLeft = dashDuration;
            dashVelocity = new Vector2(facing * dashSpeed, 0f);
        }
        dashQueued = false;

        if (grounded && walkVelocity.y < 0f)
            walkVelocity.y = 0f;

        bool jumped = jumpQueued && grounded;
        if (jumped)
            walkVelocity.y = GetJumpVelocity();
        jumpQueued = false;

        if (!grounded && !jumped)
        {
            float g = GetGravityForVerticalVelocity(walkVelocity.y);
            walkVelocity.y -= g * dt;
            walkVelocity.y = Mathf.Max(walkVelocity.y, -maxFallSpeed);
        }

        if (Mathf.Abs(inputHorizontal) > 0.01f)
        {
            if (instantHorizontalMove)
                walkVelocity.x = inputHorizontal * maxSpeed;
            else
                walkVelocity.x += inputHorizontal * acceleration * dt;
        }
        else
            walkVelocity.x = Mathf.MoveTowards(walkVelocity.x, 0f, deceleration * dt);

        walkVelocity.x = Mathf.Clamp(walkVelocity.x, -maxSpeed, maxSpeed);

        Vector2 step = walkVelocity + dashVelocity + impulseVelocity;
        rb.MovePosition(rb.position + step * dt);
    }

    private float GetJumpVelocity()
    {
        return (2f * jumpHeight) / Mathf.Max(jumpTimeToPeak, 1e-4f);
    }

    private float GetGravityForVerticalVelocity(float verticalVelocity)
    {
        float h = Mathf.Max(jumpHeight, 1e-4f);
        float tp = Mathf.Max(jumpTimeToPeak, 1e-4f);
        float td = Mathf.Max(jumpTimeToDescent, 1e-4f);
        float riseGravity = (2f * h) / (tp * tp);
        float fallGravity = (2f * h) / (td * td);
        return verticalVelocity > 0f ? riseGravity : fallGravity;
    }
}
