using Alter.Runtime.Character;
using Alter.Runtime.Properties;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Navigation")]
    [UnitTitle("Character Move In Track")]
    public class CharacterMoveInTrack_Unit : WaitUnit
    {
        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput valueTargetList;

        [DoNotSerialize]
        public ValueInput valueMoveInLoop;

        [DoNotSerialize]
        public ValueInput valueWaitToComplete;

        [DoNotSerialize]
        public ValueInput valuePriority;
        protected override async void Definition() //The method to set what our node will be doing.
        {
            base.Definition();

            valueCharacter = ValueInput<CharacterProperty>("Character", null);
            valueTargetList = ValueInput<List<Transform>>("Target Transforms", null);
            valueMoveInLoop = ValueInput<bool>("Is MoveInLoop", true);
            valueWaitToComplete = ValueInput<bool>("wait To Complete", true);
            valuePriority = ValueInput<int>("Priority", 0);
        }

        protected override IEnumerator Await(Flow flow)
        {
            isCompleted = false;
            var characterProp = flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null)
            {
                yield return exit;
            }

            var _transformList = flow.GetValue<List<Transform>>(valueTargetList);
            if (_transformList == null || _transformList.Count <= 0)
            {
                yield return exit;
            }
            else
            {
                character.CharacterDriver.MoveInTrack(_transformList, flow.GetValue<bool>(valueMoveInLoop), OnFinished, flow.GetValue<int>(valuePriority));
            }
            if (flow.GetValue<bool>(valueWaitToComplete))
            {
                yield return new WaitUntil(() => isCompleted);
                yield return exit;
            }
            else
            {
                yield return exit;
            }
        }

        bool isCompleted = false;
        void OnFinished()
        {
            isCompleted = true;
        }
    }
}