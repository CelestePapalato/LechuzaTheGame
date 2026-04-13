using UnityEngine;
using CustomInputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSO playerInputSO;
    [SerializeField]
    private PlatformerMovement movement;

    private void Awake()
    {
        if(!movement)
            movement = GetComponentInChildren<PlatformerMovement>();
    }

    private void Update()
    {
        movement.SetMoveInput(
            playerInputSO.MovementInput.x,
            playerInputSO.JumpInput,
            playerInputSO.DashInput);
    }
}
