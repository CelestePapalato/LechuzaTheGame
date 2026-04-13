using System;
using UnityEngine;

namespace CustomInputSystem
{
    [CreateAssetMenu(fileName = "Player Input", menuName = "Input Channel", order = 1)]
    public class PlayerInputSO : ScriptableObject
    {
        [SerializeField]
        private GameInputSettingsSO gameInputSettingsSO;

        // *** GAMEPLAY INPUT ***

        // AXIS
        private Vector2 _movementInput = Vector2.zero;
        // BUTTONS
        private bool _dashInput = false;
        private bool _jumpInput = false;
        private bool _interactInput = false;

        public Action<Vector2> OnMove;
        public Action<bool> OnDash;
        public Action<bool> OnJump;
        public Action<bool> OnInteract;

        public Vector2 MovementInput
        {
            get => _movementInput;
            set
            {
                _movementInput = value;
                OnMove?.Invoke(_movementInput);
            }
        }

        public bool DashInput
        {
            get => _dashInput;
            set
            {
                if (_dashInput != value)
                {
                    _dashInput = value;
                    OnDash?.Invoke(_dashInput);
                }
            }
        }

        public bool JumpInput
        {
            get => _jumpInput;
            set
            {
                if (_jumpInput != value)
                {
                    _jumpInput = value;
                    OnJump?.Invoke(_jumpInput);
                }
            }
        }

        public bool InteractInput
        {
            get => _interactInput;
            set
            {
                if (_interactInput != value)
                {
                    _interactInput = value;
                    OnInteract?.Invoke(_interactInput);
                }
            }
        }
    }
}