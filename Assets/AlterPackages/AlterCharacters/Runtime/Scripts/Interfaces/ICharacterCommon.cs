using UnityEngine;
namespace Alter.Runtime.Character
{
    public interface ICharacterCommon
    {
        void OnStartup(ICharacter _character);
        void AfterStartup(ICharacter _character);
        void OnDispose(ICharacter _character);

        void OnEnable();
        void OnDisable();

        void OnUpdate();
        void OnFixedUpdate();
    }
}
