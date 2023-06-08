using UnityEngine;
namespace Alter.Runtime.Character
{
    [System.Serializable]
    public class CharacterMotionData : ICharacterMotionData
    {
        private Vector3 moveDirection;
        private Vector3 movePosition;
        private Quaternion moveRotation;

        private float stopThreshold;

        private float followMinDistance;
        private float followMaxDistance;

        private Character.MovementType movementType;
        private float linearSpeed;
        private float angularSpeed;

        private AnimFloat standLevel;

        public Vector3 MoveDirection { get => moveDirection; set => moveDirection = value; }
        public Vector3 MovePosition { get => movePosition; set => movePosition = value; }
        public Quaternion MoveRotation { get => moveRotation; set => moveRotation = value; }

        public float StopThreshold => stopThreshold;
        public float FollowMinDistance => followMinDistance;
        public float FollowMaxDistance => followMaxDistance;
        public Character.MovementType MovementType { get => movementType; set => movementType = value; }

        public float LinearSpeed { get => linearSpeed; set => linearSpeed = value; }
        public float AngularSpeed { get => angularSpeed; set => angularSpeed = value; }

        public AnimFloat StandLevel { get => standLevel; set => standLevel = value; }

        [SerializeField] private float walkSpeed = 4;
        [SerializeField] private bool canRun = false;
        [SerializeField] private float runSpeed = 6;
        [SerializeField] private float rotationSpeed = 5;

        [Space(20)]
        [SerializeField] private float mass = 80;
        [SerializeField] private float height = 1.8f;
        [SerializeField] private float radius = 0.25f;

        [Space(20)]
        [SerializeField] private bool useAcceleration = false;
        [SerializeField] private float acceleration, deceleration;

        [Space(20)]
        [SerializeField] private bool canJump = true;
        [SerializeField] private int airJump = 0;
        private bool isJumping = false;
        [SerializeField] private float jumpForce = 5, jumpCoolDown = 0.5f;
        [SerializeField] private float gravityUpwards = -9.81f, gravityDownwards = -9.81f;
        [SerializeField] private float terminalVelocity = -53;

        [Space(20)]
        [SerializeField] private bool canRotate = true;

        public float WalkSpeed { get => walkSpeed; set => walkSpeed = value; }
        public bool CanRun { get => canRun; set => canRun = value; }
        public float RunSpeed { get => runSpeed; set => runSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        public float Mass { get => mass; set => mass = value; }
        public float Height { get => height; set => height = value; }
        public float Radius { get => radius; set => radius = value; }

        public bool UseAcceleration => useAcceleration;
        public float Acceleration { get => acceleration; set => acceleration = value; }
        public float Deceleration { get => deceleration; set => deceleration = value; }
        public bool CanJump { get => canJump; set => canJump = value; }
        public int AirJumps { get => airJump; set => airJump = value; }

        public bool IsJumping { get => isJumping; set => isJumping = value; }
        public float JumpForce { get => jumpForce; set => jumpForce = value; }
        public float JumpCooldown { get => jumpCoolDown; set => jumpCoolDown = value; }
        public float GravityUpwards { get => gravityUpwards; set => gravityUpwards = value; }
        public float GravityDownwards { get => gravityDownwards; set => gravityDownwards = value; }

        public bool CanRotate { get => canRotate; set => canRotate = value; }
        public float TerminalVelocity { get => terminalVelocity; set => terminalVelocity = value; }

        ICharacter character;
        public void OnStartup(ICharacter _character)
        {
            character = _character;
            moveRotation = character.transform.rotation;
        }

        public void AfterStartup(ICharacter _character)
        {
            throw new System.NotImplementedException();
        }

        public void OnDispose(ICharacter _character)
        {
            throw new System.NotImplementedException();
        }

        public void OnEnable()
        {
            throw new System.NotImplementedException();
        }

        public void OnDisable()
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate()
        {
            var input = new Vector3(character.InputsManager.currentMovementInput.x, 0, character.InputsManager.currentMovementInput.y);
            var movement = input;

            var cam = character.CameraData.GetMainCamera();

            //Movement
            moveDirection = updateDirection(cam, movement);
            if (moveDirection == Vector3.zero)
                movementType = Character.MovementType.None;
            else
            {
                movementType = Character.MovementType.MoveToDirection;
            }
            //Rotation
            moveRotation = updateRotation(cam, movement);
        }
        Vector3 updateDirection(Camera cam, Vector3 moveVelocity)
        {
            if (cam)
            {
                moveVelocity = cam.transform.TransformDirection(moveVelocity);
                moveVelocity.y = 0f;
                moveVelocity.Normalize();
            }
            moveVelocity = CalculatePlayerSpeed(moveVelocity);
            moveVelocity = CalculateAcceleration(moveVelocity);
            return moveVelocity;
        }
        Quaternion updateRotation(Camera cam, Vector3 movement)
        {
            if (CanRotate)
            {
                if (movement.sqrMagnitude > 0.01f)
                {
                    Vector3 positionToLook = movement;
                    if (cam)
                    {
                        positionToLook = cam.transform.TransformDirection(positionToLook);
                        positionToLook.y = 0f;
                        positionToLook.Normalize();
                    }

                    Quaternion targetRotation = Quaternion.LookRotation(positionToLook);
                    targetRotation.z = 0;
                    targetRotation.x = 0;
                    return targetRotation;
                }
                else
                    return moveRotation;
            }
            else
            {
                if (cam)
                {
                    return Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up);
                }
                else
                {
                    Debug.LogError("Camera Hook not found");
                    return Quaternion.LookRotation(movement);
                }
            }
        }
        public Vector3 CalculateSpeed(Vector3 tarDirection)
        {
            LinearSpeed = (CanRun) ? RunSpeed : WalkSpeed;
            return tarDirection * LinearSpeed;
        }
        public Vector3 CalculatePlayerSpeed(Vector3 tarDirection)
        {
            if (character.IsPlayer)
            {
                LinearSpeed = (CanRun && character.InputsManager.isRunning) ? RunSpeed : WalkSpeed;
            }
            else
                LinearSpeed = (CanRun) ? RunSpeed : WalkSpeed;

            return tarDirection * LinearSpeed;
        }

        public Vector3 CalculateAcceleration(Vector3 tarDirection)
        {
            if (!this.UseAcceleration) return tarDirection;

            Vector3 curDirection = this.MoveDirection;

            if (tarDirection.sqrMagnitude < 0.01f) tarDirection = Vector3.zero;
            bool isIncreasing = curDirection.sqrMagnitude < tarDirection.sqrMagnitude;

            float acceleration = isIncreasing
                ? this.Acceleration
                : this.Deceleration;

            curDirection = Vector3.Lerp(
                curDirection,
                tarDirection,
                acceleration * this.character.Time.DeltaTime
            );

            if (isIncreasing)
            {

                Vector3 projection = Vector3.Project(curDirection, tarDirection);
                curDirection = projection.sqrMagnitude < tarDirection.sqrMagnitude
                    ? curDirection
                    : tarDirection;
            }
            else
            {
                float curMagnitude = curDirection.sqrMagnitude;
                float tarMagnitude = tarDirection.sqrMagnitude;
                curDirection = Mathf.Abs(curMagnitude) > 0.01f || Mathf.Abs(tarMagnitude) > 0.01f
                    ? curDirection
                    : Vector3.zero;

            }
            return curDirection;
        }
        public void OnFixedUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void ResetMovementData()
        {
            this.MoveDirection = Vector3.zero;
            this.MovePosition = Vector3.zero;
            this.MovementType = Character.MovementType.None;
        }

        public void ResetRotationData()
        {
            this.moveRotation = Quaternion.identity;
        }
    }
}