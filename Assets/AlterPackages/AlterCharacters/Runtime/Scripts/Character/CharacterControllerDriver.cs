using System;
using System.Collections.Generic;
using UnityEngine;
namespace Alter.Runtime.Character
{
    [Serializable]
    public class CharacterControllerDriver : ICharacterDriver
    {
        protected const float COYOTE_TIME = 0.3f;
        protected const float GROUND_TIME = 0.1f;
        protected const int COYOTE_FRAMES = 2;

        public ICharacter character => m_character;
        private ICharacter m_character;
        public ICharacterMotionData MotionData { get => m_motionData; }
        private ICharacterMotionData m_motionData;

        public Transform Transform => transform;
        private Transform transform;
        public Vector3 WorldMoveDirection => this.m_Controller.velocity;

        public Vector3 LocalMoveDirection => this.Transform.InverseTransformDirection(
            this.WorldMoveDirection
            );

        public float SkinWidth => skinWidth;
        [SerializeField] private float skinWidth = 0.08f;

        public bool IsGrounded => m_Controller.isGrounded;

        //public Vector3 FloorNormal => throw new System.NotImplementedException();

        public bool Collision { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        // Members
        [NonSerialized] protected CharacterController m_Controller;

        [NonSerialized] protected Vector3 m_MoveDirection;
        private float m_VerticalSpeed;
        private float characterVertVelocity;

        [NonSerialized] protected AnimFloat m_IsGrounded;

        [NonSerialized] protected int m_GroundFrame = -100;
        [NonSerialized] protected float m_GroundTime = -100f;
        [NonSerialized] protected float m_JumpTime = -100f;

        public void SetActiveController(bool isActive)
        {
            if (m_Controller)
                m_Controller.enabled = isActive;
        }

        public void OnStartup(ICharacter _character)
        {
            m_character = _character;
            m_motionData = _character.MotionData;
            m_motionData.OnStartup(_character);
            transform = _character.transform;
            setupController(transform);

            m_motionData.StandLevel = new AnimFloat(1f, 1f);
        }

        void setupController(Transform _transform)
        {
            m_Controller = transform.GetComponent<CharacterController>();
            if (m_Controller == null)
            {
                m_Controller = transform.gameObject.AddComponent<CharacterController>();
            }
            m_Controller.height = m_motionData.Height;
            m_Controller.radius = m_motionData.Radius;
        }
        public void AfterStartup(ICharacter _character)
        {
            throw new System.NotImplementedException();
        }

        public void OnDisable()
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
        public void OnUpdate()
        {
            if (character == null)
            {
                Debug.LogError("ICharacter Not Found");
                return;
            }
            if (m_motionData == null)
            {
                m_motionData = character.MotionData;
            }

            m_motionData.StandLevel.UpdateWithDelta(this.character.Time.DeltaTime);
            handleGravity();
            if (character.IsControllable)
            {
                if (characterNavigation != null && characterNavigation.IsActive)
                {
                    characterNavigation.Update();
                    //isGrounded = m_Controller.isGrounded;//setting isGrounded as this is changed only on handle Jump when the character is player and No navigation is currently active
                }
                else if (character.IsPlayer)
                {
                    m_motionData?.OnUpdate();
                    handleJump(character);
                }
                characterRotation();
            }
            characterMovement();//for vertical movement need to call on !IsControllable also
        }
        public void OnFixedUpdate()
        {
            throw new System.NotImplementedException();
        }

        //Movement Functions
        bool isFalling = false;
        void handleGravity()
        {
            characterVertVelocity = this.m_Controller.velocity.y;

            float gravity = this.m_Controller.velocity.y >= 0f
              ? m_motionData.GravityUpwards
              : m_motionData.GravityDownwards;

            this.m_VerticalSpeed += gravity * Time.deltaTime;

            if (this.m_Controller.isGrounded)
            {
                isFalling = false;

                if (character.Time.Time - this.m_GroundTime > COYOTE_TIME &&
                    character.Time.Frame - this.m_GroundFrame > COYOTE_FRAMES)
                {
                    OnLand(m_VerticalSpeed);
                }

                this.m_GroundTime = character.Time.Time;
                this.m_GroundFrame = character.Time.Frame;

                this.m_VerticalSpeed = Mathf.Max(
                    this.m_VerticalSpeed, gravity
                );
            }
            else if (!this.m_motionData.IsJumping && !isFalling)
            {
                this.m_VerticalSpeed = m_motionData.GravityDownwards/2;
                isFalling = true;
            }
            this.m_VerticalSpeed = Mathf.Max(m_VerticalSpeed, m_motionData.TerminalVelocity);
        }
        int currJumpCount = 0;

        void handleJump(ICharacter character)
        {
            if (!m_motionData.CanJump)
                return;

            if (IsGrounded)
            {
                m_motionData.IsJumping = false;
            }

            if (character.InputsManager.isJumped)
            {
                character.InputsManager.isJumped = false;

                if (m_motionData.IsJumping)//check for double Jump
                {
                    if (m_motionData.AirJumps > 0 && currJumpCount <= m_motionData.AirJumps)
                    {
                        //air jump
                        currJumpCount++;
                        m_VerticalSpeed = m_motionData.JumpForce;

                        //play air jump animation
                        character.CharacterJumpAnimation("airJump");
                    }
                }
                else
                {
                    currJumpCount = 0;
                    if (m_Controller.isGrounded)
                    {
                        //jump
                        m_VerticalSpeed = m_motionData.JumpForce;
                        currJumpCount++;
                        if (character.CharacterAnimation.jumpStartLeftLeg)
                        {
                            //play left jump animation
                            character.CharacterJumpAnimation("leftStart");
                        }
                        else if (character.CharacterAnimation.jumpStartRightLeg)
                        {
                            //play right jump animation
                            character.CharacterJumpAnimation("rightStart");
                        }
                    }
                }
                m_motionData.IsJumping = true;
            }
        }

        #region OnLand
        protected const float LAND_RECOVERY_SMOOTH_IN = 0.3f;
        protected const float LAND_RECOVERY_DURATION = 0.1f;
        protected const float LAND_RECOVERY_SMOOTH_OUT = 0.5f;
        void OnLand(float velocity)
        {
            character.EventOnCharacterLand?.Invoke(velocity);
            float amount = Math.Abs(velocity) / (m_motionData.JumpForce * 4f);

            m_motionData.StandLevel.SetTransient(new AnimFloat.Transient(
                Mathf.Clamp01(m_motionData.StandLevel.Current - amount),
                LAND_RECOVERY_SMOOTH_IN,
                LAND_RECOVERY_DURATION,
                LAND_RECOVERY_SMOOTH_OUT
            ));
        }
        #endregion
        void characterRotation()
        {
            var currentRotation = character.transform.rotation;
            character.transform.rotation = Quaternion.Slerp(currentRotation, m_motionData.MoveRotation, m_motionData.RotationSpeed * Time.deltaTime);
        }
        void characterMovement()
        {
            Vector3 movement = Vector3.up * (this.m_VerticalSpeed * this.character.Time.DeltaTime);
            if (character.IsControllable)//blocking the navigation movement on IsControllable false
            {
                Vector3 kinetic = this.m_motionData.MovementType switch
                {
                    Character.MovementType.MoveToDirection => this.UpdateMoveToDirection(),
                    Character.MovementType.MoveToPosition => this.UpdateMoveToPosition(),
                    _ => Vector3.zero
                };
                movement += kinetic;
            }
            if (m_Controller.enabled) m_Controller.Move(movement);
        }

        protected virtual Vector3 UpdateMoveToDirection()
        {
            //Debug.Log("@@@@@ updateMoveToDirection" + this.m_motionData.GetType().ToString());

            this.m_MoveDirection = this.m_motionData.MoveDirection;
            return this.m_MoveDirection * this.character.Time.DeltaTime;
        }
        protected virtual Vector3 UpdateMoveToPosition()
        {
            //Debug.Log("@@@@@ updateMoveToPosition" + this.m_motionData.GetType().ToString());

            float distance = Vector3.Distance(this.Transform.position, this.m_motionData.MovePosition);
            float brakeRadiusHeuristic = Math.Max(this.m_motionData.Height, this.m_motionData.Radius * 2f);
            float velocity = this.m_motionData.MoveDirection.magnitude;

            if (distance < brakeRadiusHeuristic)
            {
                velocity = Mathf.Lerp(
                    this.m_motionData.LinearSpeed, Mathf.Max(this.m_motionData.LinearSpeed * 0.25f, 1f),
                    1f - Mathf.Clamp01(distance / brakeRadiusHeuristic)
                );
            }

            this.m_MoveDirection = this.m_motionData.MoveDirection;
            return this.m_MoveDirection.normalized * (velocity * this.character.Time.DeltaTime);
        }
        //Motion Functions
        public ICharacterNavigation CharacterNavigation { get => characterNavigation; set => characterNavigation = value; }
        private ICharacterNavigation characterNavigation;

        bool canUseNavigation(int _priority)
        {
            if (characterNavigation == null || _priority >= characterNavigation.Priority)
                return true;
            else
                return false;
        }
        public void Turn(float yRotation, Action OnFinished = null, int priority = 0)
        {
            if (!canUseNavigation(priority))
                return;
            StopNavigation();

            if (transform.rotation.eulerAngles.y == yRotation)
            {
                OnFinished?.Invoke();
            }
            else
            {
                characterNavigation = new Character_Rotation();
                var lookAtTarget = characterNavigation as Character_Rotation;
                lookAtTarget.Start(this, Quaternion.Euler(Vector3.up * yRotation), OnFinished, priority);
            }
        }
        public void LookAt(Vector3 direction, Action OnFinished = null, int priority = 0)
        {
            if (!canUseNavigation(priority))
                return;

            //Debug.Log("look At " + direction);

            StopNavigation();
            characterNavigation = new Character_Rotation();
            var lookAtTarget = characterNavigation as Character_Rotation;
            lookAtTarget.Start(this, direction, OnFinished, priority);
        }
        public void LookAt(Transform target, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToPosition(Vector3 target, float stopDistance = 0f, Action OnFinished = null, int priority = 0)
        {
            if (!canUseNavigation(priority))
                return;
            StopNavigation();
            characterNavigation = new Character_MoveTo();
            var moveToTarget = characterNavigation as Character_MoveTo;
            moveToTarget.Start(this, target, stopDistance, OnFinished, priority);
        }

        public void MoveToTransform(Transform target, float stopDistance = 0f, Action OnFinished = null, int priority = 0)
        {
            if (!canUseNavigation(priority))
                return;
            StopNavigation();
            characterNavigation = new Character_MoveTo();
            var moveToTarget = characterNavigation as Character_MoveTo;
            moveToTarget.Start(this, target.position, stopDistance, OnFinished, priority);
        }

        public void MoveInTrack(List<Transform> target, bool moveInLoop = true, Action _OnFinished = null, int priority = 0)
        {
            if (!canUseNavigation(priority))
                return;
            StopNavigation();
            characterNavigation = new Character_MoveInTrack();
            var followTarget = characterNavigation as Character_MoveInTrack;
            followTarget.Start(this, target, moveInLoop, _OnFinished, priority);
        }


        public void StartFollow(Transform target, float minRadius, float maxRadius, int _priority = 0)
        {
            if (!canUseNavigation(_priority))
                return;
            StopNavigation();
            characterNavigation = new Character_FollowTarget();
            var followTarget = characterNavigation as Character_FollowTarget;
            followTarget.Start(this, target, minRadius, maxRadius, _priority);
        }

        public void StopNavigation()
        {
            if (characterNavigation != null && characterNavigation.IsActive)
                characterNavigation.Stop();
            characterNavigation = null;
        }
        // POSITION METHODS: ----------------------------------------------------------------------
        protected virtual Vector3 UpdateMoveToDirection(ICharacterMotionData motion)
        {
            Debug.Log("@@@@@ updateMoveToDirection" + motion.GetType().ToString());

            this.m_MoveDirection = motion.MoveDirection;
            return this.m_MoveDirection * this.character.Time.DeltaTime;
        }

        protected virtual Vector3 UpdateMoveToPosition(ICharacterMotionData motion)
        {
            Debug.Log("@@@@@ updateMoveToPosition" + motion.GetType().ToString());

            float distance = Vector3.Distance(this.character.transform.position, motion.MovePosition);
            float brakeRadiusHeuristic = Math.Max(motion.Height, motion.Radius * 2f);
            float velocity = motion.MoveDirection.magnitude;

            if (distance < brakeRadiusHeuristic)
            {
                velocity = Mathf.Lerp(
                    motion.LinearSpeed, Mathf.Max(motion.LinearSpeed * 0.25f, 1f),
                    1f - Mathf.Clamp01(distance / brakeRadiusHeuristic)
                );
            }

            this.m_MoveDirection = motion.MoveDirection;
            return this.m_MoveDirection.normalized * (velocity * this.character.Time.DeltaTime);

        }
    }
}