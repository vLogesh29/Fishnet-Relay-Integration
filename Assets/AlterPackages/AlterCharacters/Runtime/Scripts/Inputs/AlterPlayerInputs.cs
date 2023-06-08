namespace Alter.Runtime.Inputs
{
    using UnityEngine.InputSystem;
    using UnityEngine;

    [DefaultExecutionOrder(-1)]
    public class AlterPlayerInputs : MonoBehaviour
    {
        public static AlterPlayerInputs Instance;
        bool isActive = false;

        [SerializeField] bool forceTouchControlsEditor = false;

        [Header("Options")]
        [SerializeField] bool activateOnStart = true;
        [SerializeField] bool canJump = true;
        public bool CanJump
        {
            get
            {
                return canJump;
            }
            set
            {
                canJump = value;
                if (isActive)
                    SetupJumpInput();
            }
        }

        [Space(20)]
        [Header("Input Data")]
        AlterPlayerInput playerInput;
        public AlterPlayerInput PlayerInput { get { return playerInput; } }
        public Vector2 currentMovementInput;
        public bool isMovementPressed;
        public bool isRunning = false;
        public bool isJumped = false;

        public Vector2 currentLook;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            playerInput = new AlterPlayerInput();
        }
        private void Start()
        {
            if (activateOnStart)
                ActivateInput();
            else
                DeactivateInput();
        }
        //private void OnEnable()
        //{
        //    if (!isActive)
        //        ActivateInput();
        //}
        //private void OnDisable()
        //{
        //    DeactivateInput();
        //}
        [ContextMenu("Activate Input")]
        public void ActivateInput()
        {
            if (isActive)
                return;
            playerInput?.Enable();

            playerInput.PlayerControls.Move.started += OnMovementInput;
            playerInput.PlayerControls.Look.performed += OnLookInput;
            playerInput.PlayerControls.Move.canceled += OnMovementInput;
            playerInput.PlayerControls.Move.performed += OnMovementInput;

            playerInput.PlayerControls.Run.started += OnRun;
            playerInput.PlayerControls.Run.canceled += OnRun;

            isActive = true;
            SetupMobileInputUI();
            SetupJumpInput();
        }
        void OnLookInput(InputAction.CallbackContext context)
        {
            var look = context.ReadValue<Vector2>();
            //Debug.Log("Look Input :: " + look);
            //Debug.Log("Movement Input value :: " + currentMovementInput.ToString());
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        }
        [ContextMenu("Deactivate Input")]
        public void DeactivateInput()
        {
            Debug.Log("Deactivating Input");

            playerInput?.Disable();

            playerInput.PlayerControls.Move.started -= OnMovementInput;
            playerInput.PlayerControls.Move.canceled -= OnMovementInput;
            playerInput.PlayerControls.Move.performed -= OnMovementInput;

            playerInput.PlayerControls.Run.started -= OnRun;
            playerInput.PlayerControls.Run.canceled -= OnRun;

            isActive = false;
            SetupMobileInputUI();
            SetupJumpInput();
        }

        void SetupMobileInputUI()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                EnableMobileInput(isActive);
            }
            else
            {
#if UNITY_EDITOR
                if (forceTouchControlsEditor)
                    EnableMobileInput(isActive);
#else
                EnableMobileInput(false);
#endif
            }
        }
        void SetupJumpInput()
        {
            if (CanJump && isActive)
            {
                playerInput.PlayerControls.Jump.performed += OnJump;
                playerInput.PlayerControls.Jump.canceled += OnJump;
            }
            else
            {
                playerInput.PlayerControls.Jump.performed -= OnJump;
                playerInput.PlayerControls.Jump.canceled -= OnJump;
            }
            if (touchStick.isActive)
                touchStick.SetActiveJumpUI(CanJump);
        }

        void OnMovementInput(InputAction.CallbackContext context)
        {
            currentMovementInput = context.ReadValue<Vector2>();
            //Debug.Log("Movement Input value :: " + currentMovementInput.ToString());
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        }
        void OnRun(InputAction.CallbackContext context)
        {
            isRunning = context.ReadValueAsButton();
        }
        void OnJump(InputAction.CallbackContext context)
        {
            isJumped = context.ReadValueAsButton();
        }


        [SerializeField] TouchStickManager touchStick;
        public void EnableMobileInput(bool value)
        {
            if (touchStick == null)
                touchStick = TouchStickManager.Instance;
            //if (!touchStick.gameObject.activeSelf)
            touchStick.gameObject.SetActive(value);
            touchStick.SetVisibility(value);
        }
    }
}
