namespace Alter.Runtime.Character
{
    using Alter.Runtime.Hooks;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class EmotesManager : MonoBehaviour
    {
        [SerializeField] protected GameObject UI_GameObject;
        public void EnableUI(bool isEnable)
        {
            UI_GameObject.SetActive(isEnable);
        }
        [SerializeField] protected List<EmotesData> emotesDatas;
        public List<EmotesData> EmotesDatas => emotesDatas;
        ICharacter playerCache;
        void Start()
        {
            getPlayerCharacter();
        }

        protected ICharacter getPlayerCharacter()
        {
            if (playerCache != null)
                return playerCache;
            else
            {
                var player = PlayerHook.Instance;
                if (player != null)
                {
                    playerCache = player.transform.GetComponent<ICharacter>();
                    return playerCache;
                }
            }
            return null;
        }
        public async virtual void playEmotes(int index)
        {
            if (emotesDatas == null || emotesDatas.Count < index + 1)
                return;
            var character = getPlayerCharacter();
            if (character == null) return;
            var _data = emotesDatas[index];
            await RunPlayCharacterGesture(_data, character, !_data.isPlayerControllable);
            character.IsControllable = true;
        }

        public async virtual void playEmotes(string emoteName)
        {
            if (emotesDatas == null)
                return;
            var character = getPlayerCharacter();
            if (character == null) return;
            foreach (EmotesData _emoteData in emotesDatas)
            {
                if (emoteName == _emoteData.emoteName)
                {
                    var _data = _emoteData;
                    await RunPlayCharacterGesture(_data, character, !_data.isPlayerControllable);
                    character.IsControllable = true;
                }
            }

        }
        protected async virtual Task RunPlayCharacterGesture(EmotesData data, ICharacter character, bool isWaitUntilFinish)
        {
            if (data.animationClip == null) return;
            character.IsControllable = data.isPlayerControllable;

            ConfigGesture configuration = new ConfigGesture(
                data.delay, data.animationClip.length / data.speed,
                data.speed, data.useRootMotion,
                data.transitionIn, data.transitionOut
            );

            Task gestureTask = character.Gestures.CrossFade(
                data.animationClip, data.avatarMask, data.blendMode,
                configuration, false
            );
            if (isWaitUntilFinish) await gestureTask;
            else return;
        }
    }
    public enum EmoteType
    {
        Default,
        Action,
        Dance,
        Greet,
        Romance,
        Banter,
        Discuss
    }

    [System.Serializable]
    public class EmotesData
    {
        public string emoteName;
        public Sprite emoteIcon;
        public EmoteType type;
        public AnimationClip animationClip = null;
        public bool isPlayerControllable = true;
        public AvatarMask avatarMask = null;
        public BlendMode blendMode = BlendMode.Blend;
        public float delay = 0f;
        public float speed = 1f;
        public bool useRootMotion = false;
        public float transitionIn = 0.1f;
        public float transitionOut = 0.1f;
        public AudioClip audioClip;
    }
}