namespace Alter.Runtime.Character
{
    using Alter.Runtime.Hooks;
    using Alter.Runtime.Properties;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class CoupleInteractionSystem : MonoBehaviour
    {
        public static CoupleInteractionSystem Instance;
        public bool IsBusy = false;
        [SerializeField] private List<CoupleInteractionData> coupleInteractions;
        public List<CoupleInteractionData> CoupleInteractions => coupleInteractions;

        public AudioSource audioSource;

        ICharacter mainPlayer;
        public ICharacter MainPlayer()
        {
            if (mainPlayer != null)
                return mainPlayer;
            else
            {
                var playerHook = PlayerHook.Instance;
                if (playerHook)
                    mainPlayer = playerHook.GetComponent<ICharacter>();
                return mainPlayer;
            }
        }
        ICharacter currentVictim;
        public ICharacter CurrentVictim
        {
            get
            {
#if UNITY_EDITOR
                if (currentVictim == null)
                    currentVictim = TestVictim.getCharacter();
#endif
                return currentVictim;
            }
            set => currentVictim = value;
        }

        private void Awake()
        {
            Instance = this;
            if (!audioSource)
                audioSource = this.GetComponentInChildren<AudioSource>();
        }

        [Header("Interaction test properties")]
        public CharacterProperty TestVictim;
        public int TestIndex = 0;
        public Transform testMoveToPos;

        [ContextMenu("Test Character Interaction")]
        public void TestCharacterInteraction(int index = -1)
        {
            index = index < 0 ? TestIndex : index;
            Interact(MainPlayer(), TestVictim.getCharacter(), index);
        }
        // public ICharacter currentVictim;
        [NonSerialized] public int currentIndex = 0;
        public virtual void Interact(ICharacter _attacker, ICharacter _victim, int _index)
        {
            if (IsBusy)
            {
                Debug.LogError("Interaction System Busy");
                return;
            }
            if (CoupleInteractions != null && CoupleInteractions[_index] != null)
            {
                IsBusy = true;
                //MainPlayer = _attacker;
                CurrentVictim = _victim;
                currentIndex = _index;

                CurrentVictim.IsControllable = false;

                var data = CoupleInteractions[_index];
                Vector3 moveToPos = CurrentVictim.transform.position + CurrentVictim.transform.TransformVector(data.OffsetFromVictim);
                moveToPos.y = 0;

                testMoveToPos.position = CurrentVictim.transform.position;
                Vector3 TurnTo = CurrentVictim.transform.TransformVector(CurrentVictim.transform.rotation.eulerAngles + (Vector3.up * data.YRotationWRTVictim));
                testMoveToPos.rotation = Quaternion.Euler(TurnTo);
                testMoveToPos.position = testMoveToPos.position + testMoveToPos.TransformVector(Vector3.forward);
                _attacker.CharacterDriver.MoveToPosition(moveToPos, 0.1f, () =>
                {
                    Debug.Log("MoveToPos Finished 0");
                    _attacker.CharacterDriver.Turn(TurnTo.y, () =>
                    {
                        StartInteraction(data, _attacker, CurrentVictim);
                    });
                });
            }
        }
        async void StartInteraction(CoupleInteractionData _data, ICharacter attacker, ICharacter victim)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                attacker.IsControllable = false;
                victim.IsControllable = false;
            });
            PlayGesture(attacker, _data.AttackerClip);
            if (_data.AudioClip)
            {
                var midPoint = (attacker.transform.position + victim.transform.position) / 2;
                PlayAudio(_data.AudioClip, midPoint);
            }
            await PlayGesture(victim, _data.VictimClip);
            IsBusy = false;
        }

        public async Task PlayGesture(ICharacter character, AnimationClip clip)
        {
            ConfigGesture configuration = new ConfigGesture(
             0, clip.length / 1,
             1, false,
             0.1f, 0.1f
         );

            Task gestureTask = character.Gestures.CrossFade(
                clip, null, BlendMode.Blend,
                configuration, false
            );

            await gestureTask;
            character.IsControllable = true;
        }
        public void PlayAudio(AudioClip audioClip, Vector3 position)
        {
            if (audioClip)
            {
                audioSource.transform.position = position;
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
    }
    [Serializable]
    public class CoupleInteractionData
    {
        [SerializeField] private string name;
        [SerializeField] private bool permissionRequired;
        [SerializeField] private string requestMessage;
        [SerializeField] private Sprite icon;
        [SerializeField] private AnimationClip attackerClip, victimClip;
        [SerializeField] private Vector3 offsetFromVictim;
        [SerializeField] private float yRotationWRTVictim;
        [SerializeField] private AudioClip audioClip;
        public string Name => name;
        public bool PermissionRequired => permissionRequired;
        public string RequestMessage => requestMessage;
        public Sprite Icon => icon;
        public AnimationClip AttackerClip => attackerClip;
        public AnimationClip VictimClip => victimClip;
        public Vector3 OffsetFromVictim => offsetFromVictim;
        public float YRotationWRTVictim => yRotationWRTVictim;
        public AudioClip AudioClip => audioClip;
    }
}