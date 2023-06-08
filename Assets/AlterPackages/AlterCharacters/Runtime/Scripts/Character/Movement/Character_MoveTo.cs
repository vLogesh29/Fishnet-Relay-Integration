using System;
using UnityEngine;
namespace Alter.Runtime.Character
{
    public class Character_MoveTo : ICharacterNavigation
    {
        public bool IsActive { get => isActive; }
        bool isActive = false;
        public bool IsCancellableOnPlayerInput { get => isCancellableOnPlayerInput; }
        bool isCancellableOnPlayerInput = false;
        public int Priority { get => priority; }
        int priority = 0;

        ICharacterDriver CharacterDriver;
        Vector3 m_Target;
        Action m_OnFinished;
        public void Start(ICharacterDriver _characterDriver, int _priority = 0)
        {
            throw new System.NotImplementedException();
        }
        public void Start(ICharacterDriver _characterDriver, Transform target, float stopDistance = 0f, Action onFinished = null, int _priority = 0)
        {
            CharacterDriver = _characterDriver;
            m_Target = target.position;
            m_Threshold = stopDistance;
            m_OnFinished = onFinished;
            priority = _priority;
            isActive = true;
        }
        public void Start(ICharacterDriver _characterDriver, Vector3 target, float stopDistance = 0f, Action onFinished = null, int _priority = 0)
        {
            CharacterDriver = _characterDriver;
            m_Target = target;
            m_Threshold = stopDistance;
            m_OnFinished = onFinished;
            priority = _priority;
            isActive = true;
        }

        public void Stop()
        {
            this.m_HasFinished = true;
            isActive = false;
            if (CharacterDriver != null)
                CharacterDriver.MotionData.MovementType = Character.MovementType.None;
            this.m_OnFinished?.Invoke();
        }

        bool m_HasFinished = false;
        float m_Threshold = 0f;
        public void Update()
        {
            if (this.m_HasFinished)
            {
                CharacterDriver.MotionData.MovementType = Character.MovementType.None;
                return;
            }

            Vector3 source = this.CharacterDriver.Transform.position;
            Vector3 target = m_Target;

            float distance = Vector3.Distance(source, target);
            float slowdownDistance = this.m_Threshold + this.CharacterDriver.MotionData.Radius * 2f;

            //if (this.Direction != Vector3.zero && distance <= slowdownDistance)
            //{
            //    IUnitFacing facing = this.Character.Facing;
            //    this.m_FacingLayerKey = facing.SetLayerDirection(
            //        this.m_FacingLayerKey,
            //        this.Direction,
            //        true
            //    );
            //}

            if (distance <= this.m_Threshold)
            {
                this.Stop();
                return;
            }

            Vector3 direction = target - source;
            direction.Normalize();

            if (distance < this.m_Threshold)
            {
                direction = Vector3.zero;
            }

            direction = this.CharacterDriver.MotionData.CalculateSpeed(direction);
            direction = this.CharacterDriver.MotionData.CalculateAcceleration(direction);

            this.CharacterDriver.MotionData.MoveDirection = direction;
            this.CharacterDriver.MotionData.MovePosition = this.m_Target;

            if (direction.sqrMagnitude > float.Epsilon)
                this.CharacterDriver.MotionData.MovementType = Character.MovementType.MoveToPosition;
            else
                this.CharacterDriver.MotionData.MovementType = Character.MovementType.None;

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