namespace Alter.Runtime.Character
{
    using Alter.Runtime.CameraSystem;
    using Alter.Runtime.Hooks;
    using UnityEngine;

    [System.Serializable]
    public class CharacterCameraData
    {
        [SerializeField] Transform camTarget_TP, camTarget_FP;
        [SerializeField] bool IsTP_LookAtBoneTarget = true;
        [SerializeField] HumanBodyBones TP_LookAtBone = HumanBodyBones.Head;
        [SerializeField] bool IsFP_FollowBoneTarget = true;
        [SerializeField] HumanBodyBones FP_FollowBone = HumanBodyBones.Head;
        CharacterCameraManager CameraController;
        ICharacter m_Character;
        public void AfterStartUp(ICharacter character)
        {
            if (!character.IsPlayer)
                return;
            m_Character = character;

            CameraController = CharacterCameraManager.Instance;
            if (CameraController != null)
            {
                CameraController.OnCameraChange += OnCameraChange;
                m_Character.EventAfterChangeModel += UpdateCameraTarget;
                UpdateCameraTarget();
            }
            else
            {
                Debug.LogError("Not Handled:: AlterCharacterCameraController Instance not found case");
            }
        }
        void UpdateCameraTarget()
        {
            var FP_Target = (IsFP_FollowBoneTarget) ? m_Character.CharacterAnimation.animator.GetBoneTransform(FP_FollowBone) : this.camTarget_FP;
            CameraController.SetUpCameraForPlayer(m_Character);
        }
        void OnCameraChange()
        {
            this.m_Character.MotionData.CanRotate = CameraController.CanRotate;
        }

        public Transform CamTarget_TP()
        {
            if (IsTP_LookAtBoneTarget) this.camTarget_TP.position = m_Character.CharacterAnimation.animator.GetBoneTransform(TP_LookAtBone).position;
            return camTarget_TP;
        }
        public Transform CamTarget_FP()
        {
            var FP_Target = (IsFP_FollowBoneTarget) ? m_Character.CharacterAnimation.animator.GetBoneTransform(FP_FollowBone) : this.camTarget_FP;
            return FP_Target;
        }

        Camera cacheCamera;
        public Camera GetMainCamera()
        {
            if (this.cacheCamera != null)
                return this.cacheCamera;

            if (CameraHook.Instance != null)
            {
                this.cacheCamera = CameraHook.Instance.Get<Camera>();
                return this.cacheCamera;
            }
            Debug.LogError("Camera Hook not found");
            return null;
        }
    }
}