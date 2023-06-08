using Alter.Runtime.Character;
using Alter.Runtime.Properties;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Navigation")]
    [UnitTitle("Character Move To")]
    public class CharacterMoveToUnit : WaitUnit
    {
        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput valueTargetTransform;

        [DoNotSerialize]
        public ValueInput valueTargetVector;

        [DoNotSerialize]
        public ValueInput valueStopDistance;

        [DoNotSerialize]
        public ValueInput valueWaitToComplete;

        [DoNotSerialize]
        public ValueInput valuePriority;

        private Flow coroutineFlow;
        protected override async void Definition() //The method to set what our node will be doing.
        {
            base.Definition();
            
            valueCharacter = ValueInput<CharacterProperty>("Character", null);

            valueTargetTransform = ValueInput<Transform>("Target Transform", null);
            valueTargetVector = ValueInput<Vector3>("Target Vector3", Vector3.negativeInfinity);

            valueStopDistance = ValueInput<float>("Stop Distance", 0f);
            valueWaitToComplete = ValueInput<bool>("Wait To Complete", true);
            valuePriority = ValueInput<int>("Priority", 0);
        }

        bool isCompleted = false;
        void OnFinished()
        {
            isCompleted = true;
        }

        protected override IEnumerator Await(Flow flow)
        {
            isCompleted = false;
            coroutineFlow = flow;
            var characterProp = flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null)
            {
                yield return exit;
            }

            var _transform = flow.GetValue<Transform>(valueTargetTransform);
            var _vectorPos = flow.GetValue<Vector3>(valueTargetVector);
            if (_transform == null)
            {
                if (_vectorPos.Equals(Vector3.negativeInfinity))
                {
                    yield return exit;
                }
                else
                {
                    character.CharacterDriver.MoveToPosition(_vectorPos, flow.GetValue<float>(valueStopDistance), OnFinished, flow.GetValue<int>(valuePriority));
                }
            }
            else
            {
                character.CharacterDriver.MoveToTransform(_transform, flow.GetValue<float>(valueStopDistance), OnFinished, flow.GetValue<int>(valuePriority));
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
    }
}