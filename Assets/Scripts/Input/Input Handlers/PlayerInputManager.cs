using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomInputSystem
{
    public class PlayerInputManager : MonoBehaviour
    {
        [SerializeField]
        PlayerInputSO playerInputSO;

        public static PlayerInputSO PlayerInputSOInstance { get; private set; }

        private void Awake()
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            PlayerInputSOInstance = playerInputSO;
        }

        private void OnMove(InputValue inputValue)
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            playerInputSO.MovementInput = inputValue.Get<Vector2>();
        }

        void OnDash(InputValue inputValue)
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            playerInputSO.DashInput = inputValue.isPressed;
        }

        void OnJump(InputValue inputValue)
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            playerInputSO.JumpInput = inputValue.isPressed;
        }

        void OnInteract(InputValue inputValue)
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            playerInputSO.InteractInput = inputValue.isPressed;
        }

        void OnAnchor(InputValue inputValue)
        {
            if (!playerInputSO) { Debug.LogError("PlayerInputSO is null. Can't interact."); return; }
            playerInputSO.AnchorInput = inputValue.isPressed;
        }
    }
}