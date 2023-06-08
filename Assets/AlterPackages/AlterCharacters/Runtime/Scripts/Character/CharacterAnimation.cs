namespace Alter.Runtime.Character
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class CharacterAnimation : ICharacterCommon
    {
        ICharacter character;
        [SerializeField] public Animator animator;
        [SerializeField] public AnimationClip jumpStartLeftLeg, jumpStartRightLeg, airJumpAnimation;

        [SerializeField] protected float m_SmoothTime = 0.5f;

        private const float SMOOTH_PIVOT = 0.01f;
        private const float SMOOTH_GROUNDED = 0.2f;
        private const float SMOOTH_STAND = 0.1f;

        // STATIC PROPERTIES: ---------------------------------------------------------------------

        private static readonly int K_SPEED_Z = Animator.StringToHash("Speed-Z");
        private static readonly int K_SPEED_X = Animator.StringToHash("Speed-X");
        private static readonly int K_SPEED_Y = Animator.StringToHash("Speed-Y");

        private static readonly int K_SURFACE_SPEED = Animator.StringToHash("Movement");
        // private static readonly int K_PIVOT_SPEED = Animator.StringToHash("Pivot");

        private static readonly int K_GROUNDED = Animator.StringToHash("Grounded");
        private static readonly int K_STAND = Animator.StringToHash("Stand");

        private static readonly int K_RANDOMIDLE = Animator.StringToHash("IdleRandom");

        // MEMBERS: -------------------------------------------------------------------------------

        protected Dictionary<int, AnimFloat> m_SmoothParameters;
        protected Dictionary<int, AnimFloat> m_IndependentParameters;

        // INITIALIZE METHODS: --------------------------------------------------------------------
        public void OnStartup(ICharacter _character)
        {
            character = _character;
            if (this.animator == null)
                animator = _character.transform.GetComponentInChildren<Animator>();
            var animEvent = this.animator.GetComponent<CharacterAnimationEvents>();
            if (animEvent == null)
                animEvent = this.animator.gameObject.AddComponent<CharacterAnimationEvents>();
            animEvent.Init(this);

            this.m_SmoothParameters = new Dictionary<int, AnimFloat>
            {
                { K_SPEED_Z, new AnimFloat(0f, this.m_SmoothTime) },
                { K_SPEED_X, new AnimFloat(0f, this.m_SmoothTime) },
                { K_SPEED_Y, new AnimFloat(0f, this.m_SmoothTime) },
                { K_SURFACE_SPEED, new AnimFloat(0f, this.m_SmoothTime) },
                { K_RANDOMIDLE, new AnimFloat(0f, this.m_SmoothTime) }
            };

            this.m_IndependentParameters = new Dictionary<int, AnimFloat>
            {
               // { K_PIVOT_SPEED, new AnimFloat(0f, SMOOTH_PIVOT) },
                { K_GROUNDED, new AnimFloat(1f, SMOOTH_GROUNDED) },
                { K_STAND, new AnimFloat(1f, SMOOTH_STAND) },
            };
        }
        public void AfterStartup(ICharacter character)
        {
        }
        public void OnDispose(ICharacter character)
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }
        public void OnUpdate()
        {
            //var inputs = character.InputsManager;
            var characterMotion = character.MotionData;
            var characterDriver = character.CharacterDriver;
            //if (inputs == null)
            //    inputs = AlterInputsManager.Instance;
            if (this.animator == null) return;
            if (!this.animator.gameObject.activeInHierarchy) return;

            //this.animator.updateMode = this.character.Time.UpdateTime == TimeMode.UpdateMode.GameTime
            //    ? AnimatorUpdateMode.Normal
            //    : AnimatorUpdateMode.UnscaledTime;

            //IUnitMotion motion = this.Character.Motion;
            //IUnitDriver driver = this.Character.Driver;
            //IUnitFacing facing = this.Character.Facing;

            Vector3 movementDirection = characterMotion.LinearSpeed > 0f
                ? characterDriver.LocalMoveDirection / characterMotion.LinearSpeed
                : Vector3.zero;
            float movementMagnitude = Vector3.Scale(new Vector3(1, 0, 1), movementDirection).magnitude;
            //Debug.Log("Movement Direction :: " + movementDirection);
            //Debug.Log("Movement Magnitude :: " + movementMagnitude);
            //float pivot = facing.PivotSpeed;

            foreach (KeyValuePair<int, AnimFloat> parameter in this.m_SmoothParameters)
            {
                parameter.Value.Smooth = this.m_SmoothTime;
            }

            float deltaTime = character.Time.DeltaTime;

            // Update anim parameters:
            if (characterMotion.CanRotate)
            {
                this.m_SmoothParameters[K_SPEED_Z].UpdateWithDelta(movementMagnitude, deltaTime);
                this.m_SmoothParameters[K_SPEED_X].UpdateWithDelta(0, deltaTime);
            }
            else
            {
                this.m_SmoothParameters[K_SPEED_Z].UpdateWithDelta(movementDirection.z, deltaTime);
                this.m_SmoothParameters[K_SPEED_X].UpdateWithDelta(movementDirection.x, deltaTime);
            }
            this.m_SmoothParameters[K_SPEED_Y].UpdateWithDelta(movementDirection.y, deltaTime);
            this.m_SmoothParameters[K_SURFACE_SPEED].UpdateWithDelta((characterMotion.LinearSpeed == characterMotion.RunSpeed) ? movementMagnitude : movementMagnitude / 2, deltaTime);

            //this.m_IndependentParameters[K_PIVOT_SPEED].UpdateWithDelta(pivot, deltaTime);
            this.m_IndependentParameters[K_GROUNDED].UpdateWithDelta(characterDriver.IsGrounded, deltaTime);
            this.m_IndependentParameters[K_STAND].UpdateWithDelta(characterMotion.StandLevel.Current, deltaTime);

            // Update animator parameters:
            this.animator.SetFloat(K_SPEED_Z, this.m_SmoothParameters[K_SPEED_Z].Current);
            this.animator.SetFloat(K_SPEED_X, this.m_SmoothParameters[K_SPEED_X].Current);
            this.animator.SetFloat(K_SPEED_Y, this.m_SmoothParameters[K_SPEED_Y].Current);
            this.animator.SetFloat(K_SURFACE_SPEED, this.m_SmoothParameters[K_SURFACE_SPEED].Current);

            //this.animator.SetFloat(K_PIVOT_SPEED, this.m_IndependentParameters[K_PIVOT_SPEED].Current);
            this.animator.SetFloat(K_GROUNDED, this.m_IndependentParameters[K_GROUNDED].Current);
            this.animator.SetFloat(K_STAND, this.m_IndependentParameters[K_STAND].Current);

            if (IsPlayingIdle && this.m_SmoothParameters[K_SURFACE_SPEED].Current > 0f)
            {
                IsPlayingIdle = false;
                m_IdleIndex = 0;
            }
            else if (!IsPlayingIdle && this.m_SmoothParameters[K_SURFACE_SPEED].Current <= 0)
            {
                IsPlayingIdle = true;
                m_IdleIndex = 0;
            }
            this.m_SmoothParameters[K_RANDOMIDLE].UpdateWithDelta(m_IdleIndex, deltaTime);
            this.animator.SetFloat(K_RANDOMIDLE, this.m_SmoothParameters[K_RANDOMIDLE].Current);
            ////================================================
            ////if (!this.animator.gameObject.activeInHierarchy) return;

            ////if (this.animator == null) throw new UnityException("No Animator Found");
            ////if (this.character == null) throw new UnityException("No Character Found");

            ////if (inputs.isMovementPressed)
            ////{
            ////    animator.SetFloat("Movement", (inputs.isRunning && character.canRun) ? 1f : 0.5f);
            ////    if (character.faceDirection == AlterCharacter.FaceDirection.movementDirection)
            ////    {
            ////        animator.SetFloat("Speed-Z", 1f);
            ////        animator.SetFloat("Speed-X", 0);
            ////    }
            ////    else
            ////    {
            ////        animator.SetFloat("Speed-Z", lastMovement.z);
            ////        animator.SetFloat("Speed-X", lastMovement.x);
            ////    }
            ////}
            ////else
            ////    animator.SetFloat("Movement", 0);
        }

        public void OnFixedUpdate()
        {
        }

        bool IsPlayingIdle = false;
        int standIdleCount = 2;
        float m_IdleIndex = 0f;
        public void SetRandomIdleIndex()
        {
            var index = Random.Range(1, standIdleCount + 1);
            if (m_IdleIndex == index) index = Random.Range(1, standIdleCount + 1);
            m_IdleIndex = index;
        }
    }
}