using Alter.Runtime.Properties;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Properties")]
    [UnitTitle("Character Is Controllable")]
    public class CharacterIsControllable_Unit : Unit
    {
        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput valueIsControllable;

        //[DoNotSerialize]
        //public ValueOutput result;

        protected override async void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("input", (flow) =>
            {
                Run(flow);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("output");

            valueCharacter = ValueInput<CharacterProperty>("Character", null);
            valueIsControllable = ValueInput<bool>("Is Controllable", true);
        }

        protected void Run(Flow _flow)
        {
            var characterProp = _flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null) return;

            var isControllable = _flow.GetValue<bool>(valueIsControllable);
            character.IsControllable = isControllable;
        }
    }
}