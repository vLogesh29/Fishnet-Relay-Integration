namespace Alter.Runtime.Inputs
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.OnScreen;
    using UnityEngine.UI;

    public enum VirtualJoystickType { Fixed, Floating }

    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {

        [SerializeField] private RectTransform centerArea = null;
        [SerializeField] private RectTransform handle = null;
        [InputControl(layout = "Vector2")]
        [SerializeField] private string stickControlPath;
        [SerializeField] private float movementRange = 100f;

        [SerializeField] bool IsSecondInput = false;
        [SerializeField][Range(0f, 1f)] float minDistanceForSecondInput = 0.5f;
        [InputControl(layout = "Button")]
        [SerializeField] private string secondInputButtonPath;

        protected VirtualJoystickType joystickType = VirtualJoystickType.Fixed;
        protected bool _hideOnPointerUp = false;
        protected bool _centralizeOnPointerUp = true;
        private Canvas canvas;
        protected RectTransform baseRect = null;
        protected OnScreenStick handleStickController = null;
        protected OnScreenButton handleButtonController = null;
        protected CanvasGroup bgCanvasGroup = null;
        protected Vector2 initialPosition = Vector2.zero;


        protected virtual void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            baseRect = GetComponent<RectTransform>();
            bgCanvasGroup = centerArea.GetComponent<CanvasGroup>();
            handleStickController = handle.gameObject.AddComponent<OnScreenStick>();
            handleStickController.movementRange = movementRange;
            handleStickController.controlPath = stickControlPath;


            Vector2 center = new Vector2(0.5f, 0.5f);
            centerArea.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;

            initialPosition = centerArea.anchoredPosition;

            if (IsSecondInput)
            {
                handleButtonController = handle.gameObject.AddComponent<OnScreenButton>();
                handleButtonController.controlPath = secondInputButtonPath;
            }

            if (joystickType == VirtualJoystickType.Fixed)
            {
                if (IsSecondInput)
                {
                    handleButtonController.enabled = false;
                    handle.gameObject.AddComponent<FixedJoystick_SecondInputHandler>().Initialize(this);
                }
                centerArea.anchoredPosition = initialPosition;
                bgCanvasGroup.alpha = 1;
            }
            else if (joystickType == VirtualJoystickType.Floating)
            {
                handle.GetComponent<Image>().raycastTarget = false;
                if (_hideOnPointerUp) bgCanvasGroup.alpha = 0;
                else bgCanvasGroup.alpha = 1;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerEventData constructedEventData = new PointerEventData(EventSystem.current);
            constructedEventData.position = handle.position;
            handleStickController.OnPointerDown(constructedEventData);

            if (joystickType == VirtualJoystickType.Floating)
            {
                centerArea.anchoredPosition = GetAnchoredPosition(eventData.position);

                if (_hideOnPointerUp)
                    bgCanvasGroup.alpha = 1;
            }
        }
        Canvas localCanvas;
        public float GetWidth(RectTransform rt)
        {
            if (!localCanvas)
            {
                localCanvas = GetComponentInParent<Canvas>();
            }
            var w = (rt.anchorMax.x - rt.anchorMin.x) * Screen.width + rt.sizeDelta.x * localCanvas.scaleFactor;
            return w;
        }
        public void OnDrag(PointerEventData eventData)
        {

            if (joystickType == VirtualJoystickType.Floating)
            {
                if (IsSecondInput)
                {
                    float centerAreadWidth = GetWidth(centerArea);
                    var responsiveRange = movementRange * (centerAreadWidth/centerArea.sizeDelta.x);
                   
                    float currentNormalizedDistance = Vector3.Distance(centerArea.position, handle.position)/ responsiveRange;
                    if (currentNormalizedDistance < minDistanceForSecondInput)
                    {
                        handleButtonController.OnPointerUp(eventData);
                    }
                    else
                    {
                        handleButtonController.OnPointerDown(eventData);
                    }
                }
                handleStickController.OnDrag(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (joystickType == VirtualJoystickType.Floating)
            {
                if (IsSecondInput)
                {
                    handleButtonController.OnPointerUp(eventData);
                }
                if (_centralizeOnPointerUp)
                    centerArea.anchoredPosition = initialPosition;

                if (_hideOnPointerUp) bgCanvasGroup.alpha = 0;
                else bgCanvasGroup.alpha = 1;
            }

            PointerEventData constructedEventData = new PointerEventData(EventSystem.current);
            constructedEventData.position = Vector2.zero;

            handleStickController.OnPointerUp(constructedEventData);
        }

        protected Vector2 GetAnchoredPosition(Vector2 screenPosition)
        {
            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null;
            Vector2 localPoint = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
            {
                Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
                return localPoint - (centerArea.anchorMax * baseRect.sizeDelta) + pivotOffset;
            }

            return Vector2.zero;
        }

        public void FixedStickSecondInputUpdate(PointerEventData eventData)
        {
            if (!IsSecondInput)
                return;
            float centerAreadWidth = GetWidth(centerArea);
            var responsiveRange = movementRange * (centerAreadWidth / centerArea.sizeDelta.x);

            float currentNormalizedDistance = Vector3.Distance(centerArea.position, handle.position) / responsiveRange;
            if (currentNormalizedDistance < minDistanceForSecondInput)
            {
                handleButtonController.enabled = true;
                handleButtonController.OnPointerDown(eventData);
            }
            else
            {
                handleButtonController.OnPointerUp(eventData);
                handleButtonController.enabled = false;
            }
        }
    }
}