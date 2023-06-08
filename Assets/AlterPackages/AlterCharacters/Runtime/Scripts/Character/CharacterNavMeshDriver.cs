using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Alter.Runtime.Character
{
    public class CharacterNavMeshDriver : ICharacterDriver
    {
        ICharacter _character;
        public ICharacter character { get => character; }

        public Transform Transform { get; }

        public ICharacterMotionData MotionData { get => m_motionData; }
        private ICharacterMotionData m_motionData;

        public Vector3 WorldMoveDirection => throw new System.NotImplementedException();

        public Vector3 LocalMoveDirection => throw new System.NotImplementedException();

        public float SkinWidth => throw new System.NotImplementedException();

        public bool IsGrounded => throw new System.NotImplementedException();

        public Vector3 FloorNormal => throw new System.NotImplementedException();

        public bool Collision { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ICharacterNavigation CharacterNavigation { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public void SetActiveController(bool isActive)
        {
            throw new NotImplementedException();
        }
        public void OnStartup(ICharacter character)
        {
            _character = character;
        }
        public void AfterStartup(ICharacter character)
        {
            throw new System.NotImplementedException();
        }

        public void OnDisable()
        {
            throw new System.NotImplementedException();
        }

        public void OnDispose(ICharacter character)
        {
            throw new System.NotImplementedException();
        }

        public void OnEnable()
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate()
        {
            throw new System.NotImplementedException();
        }
        public void Turn(float yRotation, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }
        public void LookAt(Vector3 direction, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void LookAt(Transform target, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToPosition(Vector3 target, float stopDistance = 0f, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToTransform(Transform target, float stopDistance = 0f, Action OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void MoveInTrack(List<Transform> target, bool moveInLoop = true, Action _OnFinished = null, int priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void StartFollow(Transform _transform, float minRadius, float maxRadius, int _priority = 0)
        {
            throw new System.NotImplementedException();
        }

        public void StopNavigation()
        {
            throw new System.NotImplementedException();
        }
    }
}