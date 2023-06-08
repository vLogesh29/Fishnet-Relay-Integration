#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Alter.Core;
using Alter.Runtime.Character;
using Alter.Runtime.Hooks;

namespace Alter.Editor.Character
{
    public class CharacterEditor : MonoBehaviour
    {
        protected const string CHARACTER_PREFAB_PATH = "Assets/AlterPackages/AlterCharacters/Runtime/Prefabs/Character/Alter_Character.prefab";

        [MenuItem("GameObject/Alter/Character/Create Player", false, 0)]
        static void CreatePlayer()
        {
            Debug.Log("Creating Player");
            var player = CreateCharacter();
            if (!player)
            {
                Debug.LogError("Character Prefab not found in path " + CHARACTER_PREFAB_PATH);
                return;
            }
            player.name = "AlterPlayer";
            player.GetComponent<ICharacter>().IsPlayer = true;
            player.AddComponent<PlayerHook>();
            player.tag = "Player";
        }
        [MenuItem("GameObject/Alter/Character/Create NPC", false, 0)]
        static void CreateNPC()
        {
            Debug.Log("Creating NPC");
            var character = CreateCharacter();
            if (!character)
            {
                Debug.LogError("Character Prefab not found in path " + CHARACTER_PREFAB_PATH);
                return;
            }
        }

        static GameObject CreateCharacter()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHARACTER_PREFAB_PATH);
            if (prefab == null) return null;

            GameObject instance = Instantiate(prefab);
            instance.name = prefab.name;
            instance = CreateSceneObject.Create(instance, true);
            return instance;
        }
    }
}
#endif