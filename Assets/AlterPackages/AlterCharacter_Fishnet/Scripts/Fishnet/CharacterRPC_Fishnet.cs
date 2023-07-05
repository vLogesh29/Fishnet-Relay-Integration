using Alter.Runtime.Character;
using FishNet.Object;
using System.Threading.Tasks;
using UnityEngine;
namespace Alter.Runtime.CharacterNetworked
{
    [RequireComponent(typeof(ICharacter))]
    public class CharacterRPC_Fishnet : NetworkBehaviour
    {
        ICharacter character;
        ICharacter Character()
        {
            if (character == null)
                character = GetComponent<ICharacter>();
            return character;
        }

        #region Emotes
        bool isEmote = false;
        public void PlayEmoteRPC(string emoteName)
        {
            Debug.Log($"%% Char-Fish-RPC called for \"PlayEmoteRPC()\" -> IsOwner:{IsOwner}");
            if (base.IsOwner)
                PlayEmote(emoteName);
        }
        public void StopEmoteRPC()
        {
            Debug.Log("%% Char-Fish-RPC called for \"StopEmoteRPC()\" -> IsOwner:{IsOwner}");
            if (isEmote)
                StopEmote();
        }

        void PlayEmote(string emoteName) => PlayEmoteServerRpc(emoteName);

        [ServerRpc]
        private void PlayEmoteServerRpc(string emoteName) => PlayEmoteObserversRpc(emoteName);

        [ObserversRpc]
        private void PlayEmoteObserversRpc(string emoteName) => PlayEmoteAsync(emoteName);

        async void PlayEmoteAsync(string emoteName)
        {
            Debug.Log($"%% Playing Emote @{gameObject.name}");
            isEmote = true;
            var emoteData = EmotesManager_Fishnet.Instance.EmotesDatas.Find(x => x.emoteName == emoteName);
            if (emoteData != null)
            {
                await RunPlayCharacterGesture(emoteData, Character(), true);
                character.IsControllable = true;
            }
        }
        void StopEmote() => StopEmoteServerRpc();

        [ServerRpc]
        private void StopEmoteServerRpc() => StopEmoteObserversRpc();

        [ObserversRpc]
        private void StopEmoteObserversRpc()
        {
            Debug.Log($"%% Stoping Emote Async @{gameObject.name}");
            isEmote = false;
            Character().Gestures.Stop(0, 0.1f);
            character.IsControllable = true;
        }

        protected async Task RunPlayCharacterGesture(EmotesData data, ICharacter character, bool isWaitUntilFinish)
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
        #endregion

        #region Character Interaction
        void InteractionRequest() => InteractionRequestServerRpc();

        [ServerRpc]
        void InteractionRequestServerRpc() => InteractionRequestObserverRpc();

        [ObserversRpc]
        void InteractionRequestObserverRpc()
        {
            Debug.Log("Received Interaction Request :: ");
        }
        #endregion
    }
}