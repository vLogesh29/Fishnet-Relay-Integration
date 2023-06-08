using System;
using UnityEngine;
namespace Alter.Runtime.Character
{
    public class Character_Rotation : ICharacterNavigation
    {
        public bool IsActive { get => isActive; }
        bool isActive = false;
        public bool IsCancellableOnPlayerInput { get => isCancellableOnPlayerInput; }
        bool isCancellableOnPlayerInput = false;
        public int Priority { get => priority; }
        int priority = 0;

        ICharacterDriver CharacterDriver;
        public Vector3 Target { get { return new Vector3(m_Target.x, CharacterDriver.Transform.position.y, m_Target.z); } }
        Vector3 m_Target = Vector3.zero;
        Action m_OnFinished;

        //Turning properties
        bool isTurn = false;
        Quaternion m_Rotation;
        public void Start(ICharacterDriver _characterDriver, int _priority = 0)
        {
            throw new System.NotImplementedException();
        }
        public void Start(ICharacterDriver _characterDriver, Quaternion newRotation, Action onFinished = null, int _priority = 0)
        {
            isTurn = true;
            m_Rotation = newRotation;
            CharacterDriver = _characterDriver;
            m_OnFinished = onFinished;
            priority = _priority;
            isActive = true;
        }
        public void Start(ICharacterDriver _characterDriver, Transform LookAtTarget, Action onFinished = null, int _priority = 0)
        {
            if (!LookAtTarget) return;
            Start(_characterDriver, LookAtTarget.position, onFinished, _priority);
        }
        public void Start(ICharacterDriver _characterDriver, Vector3 _target, Action onFinished = null, int _priority = 0)
        {
            isTurn = false;
            m_Target = _target;
            CharacterDriver = _characterDriver;
            m_OnFinished = onFinished;
            priority = _priority;
            isActive = true;
        }
        public void Stop()
        {
            isActive = false;
            if (CharacterDriver != null)
                CharacterDriver.MotionData.MovementType = Character.MovementType.None;
            m_OnFinished?.Invoke();
        }

        bool isRotating = false;
        Quaternion TargetRotation;
        public void Update()
        {
            if (!isActive) return;

            if (!isRotating)
            {
                if (isTurn)
                {
                    TargetRotation = m_Rotation;
                    this.CharacterDriver.MotionData.MoveRotation = TargetRotation;
                }
                else
                {
                    Vector3 direction = Target - this.CharacterDriver.Transform.position;
                    TargetRotation = Quaternion.LookRotation(direction);
                    TargetRotation.z = 0;
                    TargetRotation.x = 0;

                    this.CharacterDriver.MotionData.MoveRotation = TargetRotation;
                }
                this.CharacterDriver.MotionData.MovementType = Character.MovementType.None;
                isRotating = true;
            }
            else
            {
                //if (CharacterDriver.Transform.rotation == TargetRotation)
                if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(CharacterDriver.Transform.rotation, TargetRotation)), 1.0f))
                {
                    Debug.Log("Turn or Look At Finished");
                    this.CharacterDriver.StopNavigation();
                }
            }
        }
    }
}