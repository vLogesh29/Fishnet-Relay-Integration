using Alter.Runtime.Character;
using Alter.Runtime.Properties;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Navigation")]
    [UnitTitle("Character Look At")]
    public class CharacterLookAt_Unit : WaitUnit
    {
        //[DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        //public ControlInput inputTrigger; //Adding the ControlInput port variable

        //[DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        //public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput valueTarget;

        [DoNotSerialize]
        public ValueInput valueWaitToComplete;

        [DoNotSerialize]
        public ValueInput valuePriority;

        //[DoNotSerialize]
        //public ValueOutput result;

        protected override async void Definition() //The method to set what our node will be doing.
        {
            base.Definition();
            ////Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            //inputTrigger = ControlInput("input", (flow) =>
            //{
            //    Run(flow);
            //    return outputTrigger;
            //});
            ////Making the ControlOutput port visible and setting its key.
            //outputTrigger = ControlOutput("output");

            valueCharacter = ValueInput<CharacterProperty>("Character", null);
            valueTarget = ValueInput<Vector3>("Target", Vector3.negativeInfinity);
            valueWaitToComplete = ValueInput<bool>("Wait To Complete", false);
            valuePriority = ValueInput<int>("Priority", 0);
        }
        protected override IEnumerator Await(Flow flow)
        {
            isCompleted = false;
            var _target = flow.GetValue<Vector3>(valueTarget);
            if (_target.Equals(Vector3.negativeInfinity)) yield return exit;

            var characterProp = flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null) yield return exit;

            character.CharacterDriver.LookAt(_target, OnFinished, flow.GetValue<int>(valuePriority));
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