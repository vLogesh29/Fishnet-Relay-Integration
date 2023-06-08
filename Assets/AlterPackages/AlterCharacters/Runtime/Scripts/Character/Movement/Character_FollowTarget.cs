using UnityEngine;
namespace Alter.Runtime.Character
{
    public class Character_FollowTarget : ICharacterNavigation
    {
        public bool IsActive { get => isActive; }
        bool isActive = false;
        public bool IsCancellableOnPlayerInput { get => isCancellableOnPlayerInput; }
        bool isCancellableOnPlayerInput = false;
        public int Priority { get => priority; }
        int priority = 0;
        public Transform TargetTrans { get => m_Target; }
        Transform m_Target;

        ICharacterDriver CharacterDriver;

        //Other properties
        Vector3 m_LastKnownPosition;
        bool m_IsFollowing = false;
        float m_MinRadius = 0, m_MaxRadius = 0;
        public void Start(ICharacterDriver _characterDriver, int _priority = 0)
        {
        }
        public void Start(ICharacterDriver _characterDriver, Transform _transform, int _priority = 0)
        {
        }
        public void Start(ICharacterDriver _characterDriver, Transform _transform, float _minRadius = 0, float _maxRadius = 0, int _priority = 0)
        {
            if (_transform != null && _characterDriver != null)
            {
                isActive = true;
                CharacterDriver = _characterDriver;
                m_Target = _transform;
                m_MinRadius = _minRadius;
                m_MaxRadius = _maxRadius;
                priority = _priority;
                m_IsFollowing = true;
            }
        }

        public void Stop()
        {
            if (CharacterDriver != null)
                CharacterDriver.MotionData.MovementType = Character.MovementType.None;
            isActive = false;
        }

        public void Update()
        {
            if (this.m_Target) this.m_LastKnownPosition = this.m_Target.position;

            float distance = Vector3.Distance(this.CharacterDriver.Transform.position, this.m_LastKnownPosition);
            bool shouldStop = (
                !this.m_Target ||
                !this.IsActive ||
                (this.m_IsFollowing && distance <= this.m_MinRadius) ||
                (!this.m_IsFollowing && distance <= this.m_MaxRadius)
            );

            Vector3 direction = this.m_Target.position - this.CharacterDriver.Transform.position;
            direction.Normalize();
            if (shouldStop)
            {
                direction = Vector3.zero;
                direction = this.CharacterDriver.MotionData.CalculateSpeed(direction);
                direction = this.CharacterDriver.MotionData.CalculateAcceleration(direction);

                this.CharacterDriver.MotionData.MoveDirection = direction;
                this.CharacterDriver.MotionData.MovePosition = this.CharacterDriver.Transform.TransformDirection(Vector3.forward);

                this.m_IsFollowing = false;

                CharacterDriver.MotionData.MovementType = direction.sqrMagnitude > float.Epsilon
                    ? Character.MovementType.MoveToDirection
                    : Character.MovementType.None;
                //UpdateRotation(direction);
                return;
            }

            //    rotation is not workings// rotation will work 
            //    this.CharacterDriver.MotionData.MoveRotation

            this.m_IsFollowing = true;
            this.CharacterDriver.MotionData.MovePosition = this.m_LastKnownPosition;

            direction = CharacterDriver.MotionData.CalculateSpeed(direction);
            direction = CharacterDriver.MotionData.CalculateAcceleration(direction);

            this.CharacterDriver.MotionData.MoveDirection = direction;
            CharacterDriver.MotionData.MovementType = Character.MovementType.MoveToPosition;
            UpdateRotation(direction);
        }
        void UpdateRotation(Vector3 direction)
        {
            if (CharacterDriver.MotionData.CanRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation.z = 0;
                targetRotation.x = 0;

                this.CharacterDriver.MotionData.MoveRotation = targetRotation;
            }
        }
    }
}