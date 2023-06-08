namespace Alter.Runtime.Hooks
{
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Alter/Hooks/CameraHook", 100)]
    public class CameraHook : IHook<CameraHook>
    {
    }
}