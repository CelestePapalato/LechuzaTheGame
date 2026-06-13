using UnityEngine;
using CustomInputSystem;

public class PlayerController : MonoBehaviour, IAbilityUser
{
    [Header("References")]
    [SerializeField]
    private PlayerInputSO playerInputSO;
    [SerializeField]
    private PlatformerMovement movement;
    [SerializeField]
    private Transform lightAnchorPivot;
    [SerializeField]
    private LightReservoir lightReservoir;

    [Header("Abilities")]
    [SerializeField]
    private LightAnchorProjectile anchorPrefab;

    [Header("Cooldowns")]
    [SerializeField]
    float dashCooldownLength = .7f;
    [SerializeField]
    float anchorCooldownLength = 1f;

    Health health;
    public Health Health => health;

    private AbilityHandler abilityHandler;

    public Transform LightAnchorPivot => lightAnchorPivot;
    public LightAnchorProjectile AnchorPrefab => anchorPrefab;
    public LightReservoir LightReservoir => lightReservoir;
    public PlatformerMovement Movement => movement;

    private void Awake()
    {
        if(!movement)
            movement = GetComponentInChildren<PlatformerMovement>();
        health = GetComponentInChildren<Health>();

        abilityHandler = new AbilityHandler(this, this, dashCooldownLength, anchorCooldownLength);
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        playerInputSO.OnDash += HandleDash;
        playerInputSO.OnAnchor += HandleAnchor;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        playerInputSO.OnDash -= HandleDash;
        playerInputSO.OnAnchor -= HandleAnchor;
        abilityHandler.Cleanup();
        StopAllCoroutines();
    }

    private void Update()
    {
        if (PlayerState.isDead) return;

        movement.SetMoveInput(
            playerInputSO.MovementInput.x,
            playerInputSO.JumpInput);
    }

    private void HandleDeath()
    {
        movement.SetMoveInput(0, false);
    }

    private void HandleDash(bool isPressed)
    {
        if (PlayerState.isDead || !isPressed) return;
        abilityHandler.HandleDash();
    }

    private void HandleAnchor(bool isPressed)
    {
        if (PlayerState.isDead || !isPressed) return;
        abilityHandler.HandleAnchor();
    }
}
