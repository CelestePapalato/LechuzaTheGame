using UnityEngine;
using CustomInputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSO playerInputSO;
    [SerializeField]
    private PlatformerMovement movement;

    private bool dashWasHeldLastFrame = false;

    Health health;
    public Health Health => health;

    private void Awake()
    {
        if(!movement)
            movement = GetComponentInChildren<PlatformerMovement>();
        health = GetComponentInChildren<Health>();
    }

    private void Update()
    {
        bool dashIsHeld = playerInputSO.DashInput;
        bool dashPressedThisFrame = dashIsHeld && !dashWasHeldLastFrame;
        dashWasHeldLastFrame = dashIsHeld;

        movement.SetMoveInput(
            playerInputSO.MovementInput.x,
            playerInputSO.JumpInput,
            dashPressedThisFrame);
    }
}
