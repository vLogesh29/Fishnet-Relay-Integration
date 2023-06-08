using Alter.Runtime.Character;
using Alter.Runtime.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AlterCharacterWorkShop : MonoBehaviour
{
    [SerializeField] Character character;

    [Header("Character Gesture Properties")]
    [SerializeField] private AnimationClip m_AnimationClip = null;
    [SerializeField] private AvatarMask m_AvatarMask = null;
    [SerializeField] private BlendMode m_BlendMode = BlendMode.Blend;

    [Space]
    [SerializeField] private float m_Delay = 0f;
    [SerializeField] private float m_Speed = 1f;
    [SerializeField] private bool m_UseRootMotion = false;
    [SerializeField] private float m_TransitionIn = 0.1f;
    [SerializeField] private float m_TransitionOut = 0.1f;

    [Space]
    [SerializeField] private bool m_WaitToComplete = true;


    [ContextMenu("Play Character Gesture")]
    public async void PlayCharacterGesture()
    {
        await RunPlayCharacterGesture();
        Debug.Log("Log After gesture");
    }
    protected async Task RunPlayCharacterGesture()
    {
        if (this.m_AnimationClip == null) return;

        //Character character = this.m_Character.Get<Character>(args);
        if (character == null) return;

        ConfigGesture configuration = new ConfigGesture(
            this.m_Delay, this.m_AnimationClip.length / this.m_Speed,
            this.m_Speed, this.m_UseRootMotion,
            this.m_TransitionIn, this.m_TransitionOut
        );

        Task gestureTask = character.Gestures.CrossFade(
            this.m_AnimationClip, this.m_AvatarMask, this.m_BlendMode,
            configuration, false
        );

        if (this.m_WaitToComplete) await gestureTask;
    }

    [SerializeField] private float m_StopDelay = 0f;
    [SerializeField] private float m_StopTransition = 0.1f;


    [ContextMenu("Stop Character Gesture")]
    public async void StopCharacterGesture()
    {
        await RunStopCharacterGesture();
        Debug.Log("Log After gesture");
    }

    protected async Task RunStopCharacterGesture()
    {
        if (character == null) return;
        character.Gestures.Stop(this.m_StopDelay, this.m_StopTransition);
        return;
    }




    [Header("Character State Properties")]
    [SerializeField] StateAnimation stateAnim;
    [SerializeField] private StateData m1_State = new StateData(StateData.StateType.State);
    [SerializeField] private BlendMode m1_BlendMode = BlendMode.Blend;

    [SerializeField] int m1_layer = 0;

    [Space]
    [SerializeField]
    private float m1_Delay = 0f;

    [SerializeField]
    private float m1_Speed = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float m1_Weight = 1f;

    [SerializeField]
    private float m1_Transition = 0.1f;
    [ContextMenu("Test Character Enter State")]
    public async void CharacterStateTest()
    {
        await CharacterStateTest_Run();
        Debug.Log("log after Character state Enter");
    }
    protected async Task CharacterStateTest_Run()
    {
        if (!this.m1_State.IsValid()) return;

        //Character character = this.m_Character.Get<Character>(args);
        if (character == null) return;

        ConfigState configuration = new ConfigState(
            this.m1_Delay, this.m1_Speed, this.m1_Weight,
            this.m1_Transition, 0f
        );

        int layer = this.m1_layer;

        _ = character.States.SetState(
            this.m1_State, layer,
            this.m1_BlendMode, configuration
        );
        return;
    }

    [Space(20)]
    [Header("Character Follow Target")]
    [SerializeField] CharacterProperty characterToFollow;
    [SerializeField] Transform target;
    [SerializeField] int priority = 0;
    [SerializeField] float minRadius = 0f, maxRadius = 0f;

    [ContextMenu("Make Character Follow Target")]
    void MakeCharacterFollowTarget()
    {
        var _character = characterToFollow.getCharacter();
        if (_character != null)
            _character.CharacterDriver.StartFollow(target, minRadius, maxRadius, priority);
    }
    [ContextMenu("Stop Character Follow")]
    void MakeCharacterStopFollow()
    {
        var _character = characterToFollow.getCharacter();
        if (_character != null)
            _character.CharacterDriver.StopNavigation();
    }

    [Space(20)]
    [Header("Character LookAt Target")]
    [SerializeField] CharacterProperty characterToLookAt;
    [SerializeField] Transform LookAtTarget;
    [SerializeField] int lookAtPriority = 0;

    [ContextMenu("Make Character LookAt Target")]
    void MakeCharacterLookAtTarget()
    {
        var _character = characterToLookAt.getCharacter();
        if (_character != null)
            _character.CharacterDriver.LookAt(LookAtTarget.position, LookAtFinished, lookAtPriority);
    }
    void LookAtFinished()
    {
        Debug.Log("look At Finished");
    }

    [Space(20)]
    [Header("Character MoveTo Target")]
    [SerializeField] CharacterProperty characterMoveTo;
    [SerializeField] Transform MoveToTarget;
    [SerializeField] float stopDistance = 0f;
    [SerializeField] int MoveToPriority = 0;
    Action onMoveToFinished;

    [ContextMenu("Make Character MoveTo Target")]
    void MakeCharacterMoveToTarget()
    {
        var _character = characterMoveTo.getCharacter();
        if (_character != null)
        {
            onMoveToFinished += OnMoveToFinised;
            _character.CharacterDriver.MoveToTransform(MoveToTarget, stopDistance, onMoveToFinished, MoveToPriority);
        }
    }
    void OnMoveToFinised()
    {
        Debug.Log("On move To finished");
    }

    [Space(20)]
    [Header("Character MoveTo Target")]
    [SerializeField] CharacterProperty characterMoveInTrack;
    [SerializeField] List<Transform> MoveInTrackTarget;
    [SerializeField] bool moveinLoop = true;
    [SerializeField] int MoveInTrackPriority = 0;
    Action onMoveInTrackFinished;

    [ContextMenu("Make Character Move In Track")]
    void MakeCharacterMoveInTrack()
    {
        var _character = characterMoveInTrack.getCharacter();
        if (_character != null)
        {
            onMoveInTrackFinished += OnMoveInTrackFinised;
            _character.CharacterDriver.MoveInTrack(MoveInTrackTarget, moveinLoop, onMoveInTrackFinished, MoveToPriority);
        }
    }
    void OnMoveInTrackFinised()
    {
        Debug.Log("On move To finished");
    }


    [Space(20)]
    [Header("Turn Character")]
    [SerializeField] CharacterProperty characterToTurn;
    [SerializeField] Transform transformToGetTurn;

    [ContextMenu("Make Character Turn")]
    void makeCharacterTurn()
    {
        var _character = characterToTurn.getCharacter();
        if (_character != null)
        {
            _character.CharacterDriver.Turn(transformToGetTurn.rotation.eulerAngles.y, OnCharacterTurnFinished);
        }
    }

    void OnCharacterTurnFinished()
    {
        var _character = characterToTurn.getCharacter();
        if (_character != null)
        {

            Debug.Log("Character Turn Finished" + (_character.CharacterDriver.CharacterNavigation.IsActive));
        }
    }
}