using UnityEngine;
namespace Alter.Runtime.Character
{
    public interface ICharacterNavigation
    {
        bool IsActive { get; }
        bool IsCancellableOnPlayerInput { get; }//only for player
        int Priority { get; }
        void Start(ICharacterDriver _characterDriver, int _priority = 0);
        void Update();
        void Stop();
    }
}