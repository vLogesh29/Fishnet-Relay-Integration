using Alter.Runtime.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cinemachine
{
    /// <summary>
    /// This is an add-on to override the legacy input system and read input using the
    /// UnityEngine.Input package API.  Add this behaviour to any CinemachineVirtualCamera 
    /// or FreeLook that requires user input, and drag in the the desired actions.
    /// If the Input System Package is not installed, then this behaviour does nothing.
    /// </summary>
    public class CinemachineInputFeeder : MonoBehaviour, AxisState.IInputAxisProvider
    {
        /// <summary>If set, Input Actions will be auto-enabled at start</summary>
        [Tooltip("If set, Input Actions will be auto-enabled at start")]
        public bool AutoEnableInputs = true;

        private void Start()
        {
            if (AutoEnableInputs)
                EnableInput();
        }

        Vector2 lookInput = Vector2.zero;
        void OnLookInput(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        void OnLookInputCanceled(InputAction.CallbackContext context)
        {
            lookInput = Vector2.zero;
        }

        public void EnableInput()
        {
            AlterPlayerInputs.Instance.PlayerInput.PlayerControls.Look.performed += OnLookInput;
            AlterPlayerInputs.Instance.PlayerInput.PlayerControls.Look.canceled += OnLookInputCanceled;
        }
        public void DisableInput()
        {
            AlterPlayerInputs.Instance.PlayerInput.PlayerControls.Look.performed -= OnLookInput;
            AlterPlayerInputs.Instance.PlayerInput.PlayerControls.Look.canceled -= OnLookInputCanceled;
            lookInput = Vector2.zero;
        }
        private void OnDisable()
        {
            DisableInput();
        }

        /// <summary>
        /// Implementation of AxisState.IInputAxisProvider.GetAxisValue().
        /// Axis index ranges from 0...2 for X, Y, and Z.
        /// Reads the action associated with the axis.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns>The current axis value</returns>
        public virtual float GetAxisValue(int axis)
        {
            if (enabled)
            {
                switch (axis)
                {
                    case 0:
                        {
                            //if (lookInput.x <= 2 && lookInput.x >= -2)
                            //    return 0;
                            //else
                                return ProcessInput("X", lookInput.x);
                        }
                    case 1: return ProcessInput("X", lookInput.y);
                }
            }
            return 0;
        }

        [SerializeField] float touchXAxis_Sensitivity = 1f, touchYAxis_Sensitivity = 1f;
        float ProcessInput(string axis, float value)
        {
            if (Application.isMobilePlatform)
            {
                return value * ((axis == "X") ? touchXAxis_Sensitivity : touchYAxis_Sensitivity);
            }
            else
                return value;
        }
    }
}