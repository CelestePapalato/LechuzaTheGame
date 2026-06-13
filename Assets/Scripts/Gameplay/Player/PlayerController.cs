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
    private LightReservoir lightReservoir;
    [Header("Cooldowns")]
    [SerializeField]
    float dashCooldownLength = .7f;

    Health health;
    public Health Health => health;

    private AbilityHandler abilityHandler;

    public LightReservoir LightReservoir { get => lightReservoir; }
    public PlatformerMovement Movement { get => movement; }

    private void Awake()
    {
        if(!movement)
            movement = GetComponentInChildren<PlatformerMovement>();
        health = GetComponentInChildren<Health>();

        abilityHandler = new AbilityHandler(this, this, dashCooldownLength);
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        playerInputSO.OnDash += HandleDash;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        playerInputSO.OnDash -= HandleDash;
        StopAllCoroutines();
    }

    private void Update()
    {
        if (PlayerState.isDead) return;

        movement.SetMoveInput(
            playerInputSO.MovementInput.x,
            playerInputSO.JumpInput
            );
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
}
