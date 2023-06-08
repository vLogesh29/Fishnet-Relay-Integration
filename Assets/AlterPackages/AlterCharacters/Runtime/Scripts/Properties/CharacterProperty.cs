using Alter.Runtime.Character;
using Alter.Runtime.Hooks;
using UnityEngine;
using System;
using Unity.VisualScripting;

namespace Alter.Runtime.Properties
{
    [Serializable]
    [Inspectable]
    public class CharacterProperty
    {
        public enum CharacterType
        {
            Player,
            Character
        }
        [Inspectable] public CharacterType characterType;
        [Inspectable] public GameObject character;
        public ICharacter getCharacter()
        {
            if (characterType == CharacterType.Player)
                return PlayerHook.Instance.GetComponent<ICharacter>();
            else
            {
                if (character)
                    return character.GetComponent<ICharacter>();
                else
                    return null;
            }
        }
    }
}