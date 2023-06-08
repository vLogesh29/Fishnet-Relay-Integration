using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Alter.Runtime.Character
{
    public class Character_MoveInTrack : ICharacterNavigation
    {
        public bool IsActive { get => isActive; }
        bool isActive = false;
        public bool IsCancellableOnPlayerInput { get => isCancellableOnPlayerInput; }
        bool isCancellableOnPlayerInput = false;
        public int Priority { get => priority; }
        int priority = 0;

        public List<Transform> TargetListTrans { get => targetListTrans; set => targetListTrans = value; }
        private List<Transform> targetListTrans;

        public bool m_MoveInLoop, m_HasFinished = false;
        Action m_OnFinished;
        ICharacterDriver CharacterDriver;

        public void Start(ICharacterDriver _characterDriver, int _priority = 0)
        {
            throw new System.NotImplementedException();
        }

        //public void Start(ICharacterDriver _characterDriver, List<Transform> target, bool _moveInLoop = true, int _priority = 0)
        //{
        //    CharacterDriver = _characterDriver;
        //    TargetListTrans = target;
        //    moveInLoop = _moveInLoop;
        //    priority = _priority;
        //}
        public void Start(ICharacterDriver _characterDriver, List<Transform> target, bool _moveInLoop = true, Action _OnFinished = null, int _priority = 0)
        {
            m_OnFinished = _OnFinished;
            if (target == null || target.Count <= 0)
            {
                _characterDriver.StopNavigation();
                return;
            }
            CharacterDriver = _characterDriver;
            TargetListTrans = target;
            m_NextTarget = TargetListTrans[0];
            m_MoveInLoop = _moveInLoop;
            priority = _priority;
            m_HasFinished = false;
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

        public void Update()
        {
            UpdateMovement();
        }

        float m_Threshold = 0.5f;
        int m_CurrentTargetIndex = 0;
        Transform m_NextTarget;
        public void UpdateMovement()
        {
            if (this.targetListTrans == null || this.targetListTrans.Count == 0)
            {
                CharacterDriver.StopNavigation();
                return;
            }
            //continue from here added new transform field
            float distance = Vector3.Distance(this.CharacterDriver.Transform.position, this.m_NextTarget.position);
            bool shouldStop = (!this.IsActive ||
                (!m_MoveInLoop && m_CurrentTargetIndex >= this.targetListTrans.Count && distance <= this.m_Threshold)
            );
            //Debug.Log("11111111111 :: " + (m_MoveInLoop));
            //Debug.Log("22222222222 :: " + (m_CurrentTargetIndex >= this.targetListTrans.Count));
            //Debug.Log("33333333333 :: " + (distance <= this.m_Threshold));
            if (distance <= this.m_Threshold)
            {
                this.m_CurrentTargetIndex++;
                if (this.m_CurrentTargetIndex >= this.targetListTrans.Count)
                {
                    if (m_MoveInLoop)
                    {
                        this.m_CurrentTargetIndex = 0;
                        m_NextTarget = this.targetListTrans[this.m_CurrentTargetIndex];
                    }
                }
                else
                    m_NextTarget = this.targetListTrans[this.m_CurrentTargetIndex];
            }

            Vector3 direction = this.m_NextTarget.position - this.CharacterDriver.Transform.position;
            direction.Normalize();
            if (shouldStop)
            {
                //if (distance <= 0f)
                //{
                //    this.CharacterDriver.StopNavigation();
                //    return;
                //}

                //if (distance < 0f)
                //{
                //    direction = Vector3.zero;
                //}

                direction = this.CharacterDriver.MotionData.CalculateSpeed(direction);
                direction = this.CharacterDriver.MotionData.CalculateAcceleration(direction);

                this.CharacterDriver.MotionData.MoveDirection = direction;
                Vector3 moveToPos = this.m_NextTarget.position;
                moveToPos.y = this.CharacterDriver.Transform.position.y;
                this.CharacterDriver.MotionData.MovePosition = moveToPos;
                distance = Vector3.Distance(this.CharacterDriver.Transform.position, moveToPos);
                if (distance <= 0.01f)
                {
                    this.CharacterDriver.StopNavigation();
                    return;
                }
                else
                {
                    this.CharacterDriver.MotionData.MovementType = Character.MovementType.MoveToPosition;

                }

                //if (this.m_NextTarget.position != this.CharacterDriver.Transform.position)
                //    this.CharacterDriver.MotionData.MovementType = Character.MovementType.MoveToPosition;
                //else
                //{
                //    this.CharacterDriver.MotionData.MovementType = Character.MovementType.None;
                //    this.CharacterDriver.StopNavigation();
                //    return;
                //}
            }
            else
            {
                this.CharacterDriver.MotionData.MovePosition = this.m_NextTarget.position;

                direction = CharacterDriver.MotionData.CalculateSpeed(direction);

                this.CharacterDriver.MotionData.MoveDirection = direction;
                CharacterDriver.MotionData.MovementType = Character.MovementType.MoveToDirection;
            }
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