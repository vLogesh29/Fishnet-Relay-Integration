using Alter.Runtime.Properties;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Navigation")]
    [UnitTitle("Character Stop Navigation")]
    public class CharacterStopNavigation_Unit : Unit
    {
        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize, PortLabelHidden] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize]
        public ValueInput valueCharacter;

        protected override async void Definition() //The method to set what our node will be doing.
        {
            inputTrigger = ControlInput("input", (flow) =>
            {
                Run(flow);
                return outputTrigger;
            });
            outputTrigger = ControlOutput("output");
            valueCharacter = ValueInput<CharacterProperty>("Character", null);
        }

        protected async Task Run(Flow _flow)
        {
            var characterProp = _flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null) return;

            character.CharacterDriver.StopNavigation();
        }
    }
}