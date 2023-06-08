using System;
using UnityEngine;

namespace Alter.Runtime.Character
{
    [Serializable]
    public abstract class LocomotionSingle
    {
        public AnimationClip m_Animation;
    }
    
    [Serializable]
    public class StandSingle : LocomotionSingle
    { }
    
    [Serializable]
    public class CrouchSingle : LocomotionSingle
    { }
}