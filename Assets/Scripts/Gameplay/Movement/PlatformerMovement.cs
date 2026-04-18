using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerMovement : MonoBehaviour
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

    [Header("Ground")]
    [SerializeField] private CapsuleCollider2D groundCollider;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float raycastDistance = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.12f;

    [Header("Impulse")]
    [SerializeField] private float impulseDecay = 12f;

    private Rigidbody2D rb;

    private readonly Dictionary<int, ContactPoint2D[]> puntosDeContacto = new Dictionary<int, ContactPoint2D[]>();

    public bool OnFloor { get; private set; }

    private Vector2 walkVelocity;
    private Vector2 dashVelocity;
    private Vector2 impulseVelocity;

    private float dashTimeLeft;
    private float facing = 1f;

    private float inputHorizontal;
    private bool jumpQueued;
    private bool dashQueued;

    public CapsuleCollider2D GroundCollider => groundCollider;

    public void SetGroundCollider(CapsuleCollider2D collider)
    {
        groundCollider = collider;
        puntosDeContacto.Clear();
    }

    public void AddImpulse(Vector2 worldDeltaVelocity)
    {
        impulseVelocity += worldDeltaVelocity;
    }

    public void SetMoveInput(float horizontal, bool jumpPressedThisFrame, bool dashPressedThisFrame)
    {
        inputHorizontal = horizontal;
        if (jumpPressedThisFrame)
            jumpQueued = true;
        if (dashPressedThisFrame)
            dashQueued = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!groundCollider)
            groundCollider = GetComponent<CapsuleCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInFloorMask(collision.gameObject.layer))
            CacheContacts(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsInFloorMask(collision.gameObject.layer))
            CacheContacts(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        puntosDeContacto.Remove(collision.collider.GetInstanceID());
    }

    private void CacheContacts(Collision2D collision)
    {
        int n = collision.contactCount;
        if (n <= 0)
            return;
        ContactPoint2D[] contacts = new ContactPoint2D[n];
        collision.GetContacts(contacts);
        puntosDeContacto[collision.collider.GetInstanceID()] = contacts;
    }

    private bool IsInFloorMask(int layer)
    {
        return (floorLayer.value & (1 << layer)) != 0;
    }

    public void RefreshOnFloor()
    {
        if (!groundCollider)
        {
            OnFloor = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, floorLayer);
            return;
        }

        OnFloor = false;
        float cosMax = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

        foreach (ContactPoint2D[] contacts in puntosDeContacto.Values)
        {
            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].normal.y >= cosMax)
                {
                    OnFloor = true;
                    return;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        RefreshOnFloor();
        bool grounded = OnFloor;

        if (Mathf.Abs(inputHorizontal) > 0.01f)
            facing = Mathf.Sign(inputHorizontal);

        HandleImpulseVelocity(dt);
        HandleDashVelocity(dt);
        HandleJumpVelocity(dt, grounded);
        HandleInputVelocity(dt);

        Vector2 v = walkVelocity + dashVelocity + impulseVelocity;
        rb.linearVelocity = v;
    }

    private void HandleImpulseVelocity(float dt)
    {
        if (impulseVelocity.sqrMagnitude > 1e-8f)
            impulseVelocity *= Mathf.Clamp01(1f - impulseDecay * dt);
    }

    private void HandleDashVelocity(float dt)
    {
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
    }

    private void HandleJumpVelocity(float dt, bool grounded)
    {
        if (grounded && walkVelocity.y < 0f)
            walkVelocity.y = 0f;

        bool jumped = jumpQueued && grounded;
        if (jumped)
            walkVelocity.y = GetJumpVelocity();
        jumpQueued = false;

        float vyForGravity = walkVelocity.y + impulseVelocity.y;
        if (!grounded && !jumped)
        {
            float g = GetGravityForVerticalVelocity(vyForGravity);
            walkVelocity.y -= g * dt;
            walkVelocity.y = Mathf.Max(walkVelocity.y, -maxFallSpeed);
        }
    }

    private void HandleInputVelocity(float dt)
    {
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
