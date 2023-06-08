namespace Alter.Runtime.Hooks
{
    using Alter.Runtime.Character;
    using UnityEngine;

    [AddComponentMenu("Alter/Hooks/PlayerHook", 100)]
    [RequireComponent(typeof(ICharacter))]
    public class PlayerHook : IHook<PlayerHook>
    {
    }
}