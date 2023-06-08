using Alter.Runtime.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Alter.Runtime.Character
{
    public class StateAnimation : StateOverrideAnimator
    {
        [SerializeField] private AnimationClip m_StateClip;

        // SERIALIZATION CALLBACKS: ---------------------------------------------------------------

        protected sealed override void BeforeSerialize()
        {
            if (this.m_Controller == null) return;
            this.m_Controller["Human@Action"] = this.m_StateClip;
        }

        protected sealed override void AfterSerialize()
        { }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(StateAnimation))]
    public class StateAnimationEditor
    {
        // PAINT METHODS: -------------------------------------------------------------------------

        //protected override void CreateContent()
        //{
        //    SerializedProperty animation = this.serializedObject.FindProperty("m_StateClip");
        //    PropertyTool fieldAnimation = new PropertyTool(animation);

        //    this.m_Root.Add(fieldAnimation);
        //}

        // CREATE STATE: --------------------------------------------------------------------------

        [MenuItem("Assets/Create/Alter/Characters/Animation State", false, 0)]
        internal static void CreateFromMenuItem()
        {
            StateAnimation state = new StateAnimation();
            state.name = "Animation State";
        }
    }
#endif
}