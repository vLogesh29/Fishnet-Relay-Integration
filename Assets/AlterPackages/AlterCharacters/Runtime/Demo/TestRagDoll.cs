using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alter.Runtime.Character;
using Alter.Runtime.Properties;

public class TestRagDoll : MonoBehaviour
{
    [SerializeField] CharacterProperty characterProp;
    private void OnTriggerEnter(Collider other)
    {
        var _character = other.GetComponent<ICharacter>();
        if (_character != null && !_character.Ragdoll.isBusy)
            StartRagDoll(_character);
    }

    [ContextMenu("Start Ragdoll")]
    public void SetRagDoll()
    {
        StartRagDoll(characterProp.getCharacter(), false);
    }

    void StartRagDoll(ICharacter _character, bool isAutoRecover = true)
    {
        _character.SetRagdoll(true, isAutoRecover);
    }
    [ContextMenu("Recover from Ragdoll")]
    public void RecoverRagDoll()
    {
        characterProp.getCharacter().SetRagdoll(false);
    }

    bool isOtherTestingActive = false;
    public void OtherTest()
    {
        var _character = characterProp.getCharacter();

        _character.CharacterDriver.SetActiveController(isOtherTestingActive);
        _character.CharacterAnimation.animator.enabled = isOtherTestingActive;

        Transform model = _character.CharacterAnimation.animator.transform;
        switch (!isOtherTestingActive)
        {
            case true:
                //this.Ragdoll.Ragdoll(true, autoStand);
                model.SetParent(null, true);
                break;

            case false:
                model.SetParent(_character.transform, true);
                //this.Ragdoll.Ragdoll(false, autoStand);
                break;
        }
        isOtherTestingActive = !isOtherTestingActive;
    }
}
