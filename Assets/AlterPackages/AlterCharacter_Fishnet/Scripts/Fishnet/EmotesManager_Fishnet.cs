using Alter.Runtime.CharacterNetworked;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Alter.Runtime.Character
{
    public class EmotesManager_Fishnet : EmotesManager
    {
        public static EmotesManager_Fishnet Instance;
        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private List<Button> emoteBtns;
        [SerializeField] private List<TextMeshProUGUI> emoteNameTexts;

        private void Start()
        {
            for (int i=0; i < emotesDatas.Count; i++) 
            {
                int index = i;
                emoteBtns[i].onClick.AddListener(() => playEmotes(index));
                emoteNameTexts[i].text = emotesDatas[i].emoteName;
            }
        }

        public async override void playEmotes(int index)
        {
            Debug.Log($"EmotesManager-Fishnet[{index}] called!");
            if (emotesDatas == null || emotesDatas.Count < index + 1)
                return;
            var character = getPlayerCharacter();
            Debug.Log("local-player character:" + character);
            if (character == null) return;

            var _data = emotesDatas[index];

            var rpc = character.transform.GetComponent<CharacterRPC_Fishnet>();
            rpc.PlayEmoteRPC(_data.emoteName);

            ////enable bone sync overNetwork
            //await RunPlayCharacterGesture(_data, character, true);//wait is true so that we can always wait for the network sync to be disabled
            ////Disable bone sync overNetwork
            //character.IsControllable = true;
        }

        public async override void playEmotes(string emoteName)
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
                    var rpc = character.transform.GetComponent<CharacterRPC_Fishnet>();
                    rpc.PlayEmoteRPC(_data.emoteName);
                }
            }

            ////enable bone sync overNetwork
            //await RunPlayCharacterGesture(_data, character, true);//wait is true so that we can always wait for the network sync to be disabled
            ////Disable bone sync overNetwork
            //character.IsControllable = true;
        }
    }
}