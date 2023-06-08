using Alter.Runtime.Character;
using Alter.Runtime.Properties;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Alter.VisualScripting
{
    [UnitCategory("Alter/Character/Gesture")]
    [UnitTitle("Character Play Gesture")]
    public class CharacterPlayGesture_Unit : WaitUnit
    {
        [DoNotSerialize]
        public ValueInput valueCharacter;

        [DoNotSerialize]
        public ValueInput valueAnimationClip;

        [DoNotSerialize]
        public ValueInput valueAvatarMask;

        [DoNotSerialize]
        public ValueInput valueBlendMode;

        [DoNotSerialize]
        public ValueInput valueDelay;
        [DoNotSerialize]
        public ValueInput valueSpeed;
        [DoNotSerialize]
        public ValueInput valueUseRootMotion;
        [DoNotSerialize]
        public ValueInput valueTransitionIn;
        [DoNotSerialize]
        public ValueInput valueTransitionOut;

        [DoNotSerialize]
        public ValueInput valueWaitToComplete;

        protected override async void Definition() //The method to set what our node will be doing.
        {
            base.Definition();

            valueCharacter = ValueInput<CharacterProperty>("Character", null);
            valueAnimationClip = ValueInput<AnimationClip>("Animation clip", null);
            valueAvatarMask = ValueInput<AvatarMask>("Avatar mask", null);
            valueBlendMode = ValueInput<BlendMode>("Blend Mode", BlendMode.Blend);
            valueDelay = ValueInput<float>("Delay", 0f);
            valueSpeed = ValueInput<float>("Speed", 1f);
            valueUseRootMotion = ValueInput<bool>("Use rootmotion", false);

            valueTransitionIn = ValueInput<float>("Transition In", 0.1f);
            valueTransitionOut = ValueInput<float>("Transition Out", 0.1f);
            valueWaitToComplete = ValueInput<bool>("wait To Complete", true);
        }
        protected override IEnumerator Await(Flow flow)
        {
            isCompleted = false;
            Run(flow);

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
        protected async Task Run(Flow _flow)
        {
            var animClip = _flow.GetValue<AnimationClip>(valueAnimationClip);
            if (animClip == null) return;
            var characterProp = _flow.GetValue<CharacterProperty>(valueCharacter);
            var character = characterProp.getCharacter();
            if (character == null) return;
            ConfigGesture configuration = new ConfigGesture(
                _flow.GetValue<float>(valueDelay), animClip.length / _flow.GetValue<float>(valueSpeed),
                _flow.GetValue<float>(valueSpeed), _flow.GetValue<bool>(valueUseRootMotion),
                _flow.GetValue<float>(valueTransitionIn), _flow.GetValue<float>(valueTransitionOut)
            );

            Task gestureTask = character.Gestures.CrossFade(
                animClip, _flow.GetValue<AvatarMask>(valueAvatarMask), _flow.GetValue<BlendMode>(valueBlendMode),
                configuration, false
            );

            if (_flow.GetValue<bool>(valueWaitToComplete))
            {
                await gestureTask;
                isCompleted = true;
            }
        }
    }
}