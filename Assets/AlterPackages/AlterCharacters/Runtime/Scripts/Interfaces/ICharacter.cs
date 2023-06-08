using Alter.Runtime.Character.Animim;
using Alter.Runtime.CameraSystem;
using Alter.Runtime.Common;
using Alter.Runtime.Inputs;
using System;
using UnityEngine;

namespace Alter.Runtime.Character
{
    public interface ICharacter
    {
        bool IsPlayer { get; set; }
        bool IsControllable { get; set; }

        TimeMode Time { get; set; }

        AlterPlayerInputs InputsManager { get; set; }
        ICharacterMotionData MotionData { get; }
        ICharacterDriver CharacterDriver { get; }
        CharacterAnimation CharacterAnimation { get; }
        AnimimGraph AnimimGraph { get; }
        GesturesOutput Gestures { get; }
        CharacterRagdoll Ragdoll { get; }
        bool AutoInitializeRagdoll { get; }
        Transform transform { get; }

        CharacterCameraData CameraData { get; }
        async void CharacterJumpAnimation(string jumpType) { }
        public Action<float> EventOnCharacterLand { get; set; }
        public Action EventAfterChangeModel { get; set; }
        GameObject ChangeModel(GameObject go, bool createDuplicate = false) { return null; }
        public void SetRagdoll(bool active, bool autoStand = false) { }
        public bool IsRagdoll() { return false; }
    }
}