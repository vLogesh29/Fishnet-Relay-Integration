using System;
using System.Collections.Generic;
using UnityEngine;
namespace Alter.Runtime.Character
{
    public interface ICharacterDriver : ICharacterCommon
    {
        ICharacter character { get; }
        void SetActiveController(bool isActive);
        Transform Transform { get; }
        ICharacterMotionData MotionData { get; }
        Vector3 WorldMoveDirection { get; }
        Vector3 LocalMoveDirection { get; }

        float SkinWidth { get; }
        bool IsGrounded { get; }
        //Vector3 FloorNormal { get; }

        bool Collision { get; set; }

        //Character Navigation
        ICharacterNavigation CharacterNavigation { get; set; }
        void Turn(float yRotation, Action OnFinished = null, int priority = 0);
        void LookAt(Vector3 direction, Action OnFinished = null, int priority = 0);
        void LookAt(Transform target, Action OnFinished = null, int priority = 0);
        public void MoveToPosition(Vector3 target, float stopDistance = 0f, Action OnFinished = null, int priority = 0);
        public void MoveToTransform(Transform target, float stopDistance = 0f, Action OnFinished = null, int priority = 0);
        public void MoveInTrack(List<Transform> target, bool moveInLoop = true, Action _OnFinished = null, int priority = 0);
        void StartFollow(Transform _transform, float minRadius, float maxRadius, int _priority = 0);
        void StopNavigation();
    }
}