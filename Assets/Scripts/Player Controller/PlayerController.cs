using UnityEngine;

[RequireComponent(typeof(PlatformerMovement))]
public class PlayerController : MonoBehaviour
{
    private PlatformerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlatformerMovement>();
    }

    private void Update()
    {
        movement.SetMoveInput(
            Input.GetAxisRaw("Horizontal"),
            Input.GetButtonDown("Jump"),
            Input.GetKeyDown(KeyCode.LeftShift));
    }
}
