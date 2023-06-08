using Alter.Runtime.Properties;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Navigation")]
    [UnitTitle("Character Follow Target")]
    public class CharacterFollowTarget_Unit : Unit
    {
        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput transform;

        [DoNotSerialize]
        public ValueInput minRadius;

        [DoNotSerialize]
        public ValueInput maxRadius;

        [DoNotSerialize]
        public ValueInput priority;

        protected override async void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("input", (flow) =>
            {
                Run(flow);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("output");

            valueCharacter = ValueInput<CharacterProperty>("Character", null);
            transform = ValueInput<Transform>("Transform", null);
            minRadius = ValueInput<float>("Min Radius", 0f);
            maxRadius = ValueInput<float>("Max Radius", 0f);
            priority = ValueInput<int>("Priority", 0);
        }

        protected void Run(Flow _flow)
        {
            var characterProp = _flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null) return;

            var _transform = _flow.GetValue<Transform>(transform);
            if (_transform == null) return;

            character.CharacterDriver.StartFollow(_transform, _flow.GetValue<float>(minRadius), _flow.GetValue<float>(maxRadius), _flow.GetValue<int>(priority));
        }
    }
}