using UnityEngine;
namespace Alter.Runtime.Character
{
    public interface ICharacterMotionData : ICharacterCommon
    {
        Vector3 MoveDirection { get; set; }
        Vector3 MovePosition { get; set; }
        Quaternion MoveRotation { get; set; }

        float StopThreshold { get; }

        float FollowMinDistance { get; }
        float FollowMaxDistance { get; }

        Character.MovementType MovementType { get; set; }

        float LinearSpeed { get; set; }
        float AngularSpeed { get; set; }

        AnimFloat StandLevel { get; set; }

        float WalkSpeed { get; set; }
        bool CanRun { get; set; }
        float RunSpeed { get; set; }
        float RotationSpeed { get; set; }
        float Mass { get; set; }
        float Height { get; set; }
        float Radius { get; set; }

        bool UseAcceleration { get; }
        float Acceleration { get; set; }
        float Deceleration { get; set; }

        bool CanJump { get; set; }
        int AirJumps { get; set; }

        bool IsJumping { get; set; }
        float JumpForce { get; set; }
        float JumpCooldown { get; set; }

        bool CanRotate { get; set; }
        float GravityUpwards { get; set; }
        float GravityDownwards { get; set; }
        float TerminalVelocity { get; set; }

        void ResetMovementData();
        void ResetRotationData();
        Vector3 CalculateSpeed(Vector3 tarDirection);
        Vector3 CalculateAcceleration(Vector3 tarDirection);
    }
}