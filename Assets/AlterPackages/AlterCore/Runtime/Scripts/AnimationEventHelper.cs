namespace Alter.Core
{
    using UnityEngine;
    using UnityEngine.Events;

    public class AnimationEventHelper : MonoBehaviour
    {
        public UnityEvent animationEvent;
        public AnimationEventString animationEventString;
        public AnimationEventFloat animationEventFloat;
        public AnimationEventInt animationEventInt;
        public AnimationEventObject animationEventObject;
        void AnimationEvent()
        {
            animationEvent?.Invoke();
        }
        void StringAnimationEvent(string value)
        {
            animationEventString?.Invoke(value);
        }
        void FloatAnimationEvent(float value)
        {
            animationEventFloat?.Invoke(value);
        }
        void IntAnimationEvent(int value)
        {
            animationEventInt?.Invoke(value);
        }
        void ObjectAnimationEvent(Object value)
        {
            animationEventObject?.Invoke(value);
        }
    }
    [System.Serializable]
    public class AnimationEventString : UnityEvent<string> { }
    [System.Serializable]
    public class AnimationEventFloat : UnityEvent<float> { }
    [System.Serializable]
    public class AnimationEventInt : UnityEvent<float> { }
    [System.Serializable]
    public class AnimationEventObject : UnityEvent<Object> { }
}