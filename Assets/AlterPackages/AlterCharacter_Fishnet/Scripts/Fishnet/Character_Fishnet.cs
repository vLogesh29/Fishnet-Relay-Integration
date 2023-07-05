namespace Alter.Runtime.Character
{
    using Alter.Runtime.Character.Animim;
    using Alter.Runtime.Common;
    using Alter.Runtime.Hooks;
    using Alter.Runtime.Inputs;
    using FishNet.Connection;
    using FishNet.Object;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public class Character_Fishnet : NetworkBehaviour, ICharacter
    {
        public enum MovementType
        {
            None,
            MoveToDirection,
            MoveToPosition,
        }
        public bool IsPlayer { get => isPlayer; set => isPlayer = value; }
        public bool IsControllable
        {
            get
            {
                if (Ragdoll.isBusy)
                    return false;
                else
                    return isControllable;
            }
            set
            {
                if (!value)
                {
                    if (characterDriver.CharacterNavigation != null)
                    {
                        characterDriver.CharacterNavigation.Stop();
                        characterDriver.CharacterNavigation = null;
                    }
                    characterDriver.MotionData.MoveDirection = new Vector3(0, 0, 0);
                    characterDriver.MotionData.MovePosition = new Vector3(0, 0, 0);
                    characterDriver.MotionData.MoveRotation = this.transform.rotation;
                }
                isControllable = value;
            }
        }
        [SerializeField] bool isPlayer = false, isControllable = true;

        public TimeMode Time
        {
            get => this.m_Time;
            set => this.m_Time = value;
        }
        [SerializeField] protected TimeMode m_Time;

        AlterPlayerInputs ICharacter.InputsManager { get => _inputsManager; set => _inputsManager = value; }

        [SerializeField] private AlterPlayerInputs _inputsManager;
        public ICharacterMotionData MotionData { get => motionData; }
        [SerializeReference] private ICharacterMotionData motionData = new CharacterMotionData();
        public ICharacterDriver CharacterDriver { get => characterDriver; }
        private ICharacterDriver characterDriver;
        CharacterAnimation ICharacter.CharacterAnimation { get => _characterAnimation; }
        [SerializeField] private CharacterAnimation _characterAnimation;
        public AnimimGraph AnimimGraph { get => m_AnimimGraph; }
        protected AnimimGraph m_AnimimGraph = new AnimimGraph();
        [SerializeField] private CharacterIK _characterIK;
        public GesturesOutput Gestures => this.m_AnimimGraph.Gestures;
        public StatesOutput States => this.m_AnimimGraph.States;
        public bool AutoInitializeRagdoll => m_AutoInitializeRagdoll;
        [SerializeField] private bool m_AutoInitializeRagdoll = false;

        //public AnimationClip StandFaceDown => standFaceDown;
        //public AnimationClip StandFaceUp => standFaceUp;
        //[SerializeField] private AnimationClip standFaceDown, standFaceUp;

        //public float RagDollMass => ragdollMass;
        //[SerializeField] private float ragdollMass = 80f;
        public CharacterRagdoll Ragdoll => m_CharacterRagdoll;
        [SerializeField] private CharacterRagdoll m_CharacterRagdoll;
        public CharacterCameraData CameraData => cameraData;
        [SerializeField] private CharacterCameraData cameraData;

        void Initialize()
        {
            if (isPlayer)
            {
                if (IsOwner)
                    this.gameObject.AddComponent<PlayerHook>();
                _inputsManager = AlterPlayerInputs.Instance;
            }
            if (characterDriver == null)
            {
                characterDriver = new CharacterControllerDriver();
            }
        }

        //------ Unity Events ------//
        private void OnAwake()
        {
            Initialize();
            characterDriver?.OnStartup(this);
            _characterAnimation?.OnStartup(this);
            this.m_AnimimGraph?.OnStartup(this);
            if (this._characterAnimation.animator != null && this.AutoInitializeRagdoll)
            {
                this.InitializeRagdoll();
            }
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            OnAwake();
            OnStart();
        }

        public override void OnSpawnServer(NetworkConnection conn)
        {
            base.OnSpawnServer(conn);
            int id = (conn != null) ? conn.ClientId : -1;
            gameObject.name = gameObject.name.Replace("Clone", $"ID={id}");
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void OnStart()
        {
            if (IsOwner && this.isPlayer)
            {
                InitializePlayerComponents(this);
                CameraData?.AfterStartUp(this);
            }
            this.m_AnimimGraph?.AfterStartup(this);
            _characterIK?.AfterStartup(this);
        }

        private void Update()
        {
            if (IsOwner)
            {
                //if (isControllable)
                characterDriver?.OnUpdate();
                this.m_AnimimGraph?.OnUpdate();
                _characterAnimation?.OnUpdate();
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (this.Ragdoll == null) return;
            if (this.Ragdoll.AutoRagdollOn == CharacterRagdoll.StartRagdollOn.Fall || this.Ragdoll.GetState() != CharacterRagdoll.State.Normal)
            {
                this.Ragdoll.Update();
            }
        }

        private void OnDestroy()
        {
            this.m_AnimimGraph?.OnDispose(this);
        }

        //private void OnAnimatorIK(int layerIndex)
        //{
        //    Debug.Log("_____________ " + layerIndex);
        //    _characterIK?.OnAnimatorIK(layerIndex);
        //}

        //------ ****** ------//

        // Other Methods --------------------------
        void InitializePlayerComponents(ICharacter character)
        {
            character.InputsManager = AlterPlayerInputs.Instance;
        }
        //------ Jump Animation Area ------//
        public Action<float> EventOnCharacterLand { get => eventOnCharacterLand; set => eventOnCharacterLand = value; }
        private Action<float> eventOnCharacterLand;

        public async void CharacterJumpAnimation(string jumpType)
        {
            JumpAnimationServerRPC(jumpType);
            //JumpAnimationAsync(jumpType);
            ClientOnServerAcknowledgment += () => TestAck("ServerRPC callback ack test!");
            ServerRPC(base.Owner);
        }

        [ServerRpc(RequireOwnership = true)]
        private void JumpAnimationServerRPC(string jumpType)
        {
            JumpAnimationRPC(jumpType);
        }

        [ObserversRpc(ExcludeOwner = false)] //ObserversRpc allows the server to run logic on clients. thus dont call directly from clients
        private void JumpAnimationRPC(string jumpType)
        {
            JumpAnimationAsync(jumpType);
        }

        private Action ClientOnServerAcknowledgment;
        [ServerRpc]
        private void ServerRPC(NetworkConnection clientConn)
        {
            // Perform server-side logic

            // Send targeted RPC to the client as an acknowledgement
            TargetedRPCAcknowledgement(clientConn);
        }
        [TargetRpc]
        private void TargetedRPCAcknowledgement(NetworkConnection targetConn)
        {
            // Handle the acknowledgement from the server
            // This method will be called only on the client that called serverRPC()
            ClientOnServerAcknowledgment?.Invoke();
        }
        private void TestAck(string msg)
        {
            Debug.Log(msg);
            ClientOnServerAcknowledgment = null;
        }

        async void JumpAnimationAsync(string jumpType)
        {
            AnimationClip clip = null;
            switch (jumpType)
            {
                case "leftStart":
                    {
                        clip = _characterAnimation.jumpStartLeftLeg;
                    }
                    break;
                case "rightStart":
                    {
                        clip = _characterAnimation.jumpStartRightLeg;
                    }
                    break;
                case "airJump":
                    {
                        clip = _characterAnimation.airJumpAnimation;
                    }
                    break;
            }
            if (clip == null) return;
            var speed = 1;
            ConfigGesture configuration = new ConfigGesture(
                0, clip.length / speed,
                speed, false,
                0.1f, 0.1f
            );

            this.Gestures.CrossFade(
                clip, null, BlendMode.Blend,
                configuration, false
            );
        }

        //------ Character Model Change Feature ------//
        Action ICharacter.EventAfterChangeModel { get => eventAfterChangeModel; set => eventAfterChangeModel = value; }
        Action eventAfterChangeModel;
        public GameObject ChangeModel(GameObject go, bool createDuplicate = false)
        {
            if (go == null || _characterAnimation == null)
                return null;
            RuntimeAnimatorController runtimeController = null;
            if (this._characterAnimation.animator != null)
            {
                runtimeController = this._characterAnimation.animator.runtimeAnimatorController;
                Destroy(this._characterAnimation.animator.gameObject);
            }
            GameObject instance = (createDuplicate) ? Instantiate<GameObject>(go) : go;
            instance.name = go.name;
            instance.transform.parent = transform;
            instance.transform.localPosition = new Vector3(0, 0, 0);
            instance.transform.localRotation = Quaternion.identity;
            Animator instanceAnimator = instance.GetComponent<Animator>();
            if (instanceAnimator != null)
            {
                this._characterAnimation.animator = instanceAnimator;
                this._characterAnimation.animator.applyRootMotion = false;
                instanceAnimator.runtimeAnimatorController = runtimeController;
                this._characterAnimation.animator.gameObject.AddComponent<CharacterAnimationEvents>().Init(this._characterAnimation);
            }
            eventAfterChangeModel?.Invoke();
            return instance;
        }

        //------ Character Ragdoll ------//
        public void SetRagdoll(bool active, bool autoStand = false)
        {
            if (active && this.Ragdoll.GetState() != CharacterRagdoll.State.Normal) return;
            if (!active && this.Ragdoll.GetState() == CharacterRagdoll.State.Normal) return;

            if (active) this.CharacterDriver.SetActiveController(!active);
            this.motionData.ResetMovementData();

            this._characterAnimation.animator.enabled = !active;

            Transform model = this._characterAnimation.animator.transform;
            switch (active)
            {
                case true:
                    this.Ragdoll.Ragdoll(true, autoStand);
                    model.SetParent(null, true);
                    break;

                case false:
                    model.SetParent(transform, true);
                    this.Ragdoll.Ragdoll(false, autoStand);
                    break;
            }
        }

        public void InitializeRagdoll()
        {
            this.m_CharacterRagdoll = new CharacterRagdoll(this);
        }
        public bool IsRagdoll()
        {
            return (this.Ragdoll != null && this.Ragdoll.GetState() != CharacterRagdoll.State.Normal);
        }
    }
}