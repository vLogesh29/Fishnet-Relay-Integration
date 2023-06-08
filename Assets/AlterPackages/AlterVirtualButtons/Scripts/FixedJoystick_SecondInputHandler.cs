namespace Alter.Runtime.Inputs
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class FixedJoystick_SecondInputHandler : MonoBehaviour, IDragHandler, IPointerUpHandler
    {
        VirtualJoystick fixedStick;
        public void Initialize(VirtualJoystick fixedjoystick)
        {
            fixedStick = fixedjoystick;
        }
        public void OnDrag(PointerEventData eventData)
        {
            fixedStick.FixedStickSecondInputUpdate(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            fixedStick.FixedStickSecondInputUpdate(eventData);
        }
    }
}