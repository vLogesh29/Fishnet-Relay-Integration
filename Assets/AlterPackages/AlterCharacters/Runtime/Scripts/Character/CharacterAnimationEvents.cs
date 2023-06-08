using UnityEngine;
namespace Alter.Runtime.Character
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        CharacterAnimation Anim;
        public void Init(CharacterAnimation _characterAnim)
        {
            Anim = _characterAnim;
        }

        public void PlayRandomStandIdle()
        {
            Debug.Log("Test Random Idle");
            Anim.SetRandomIdleIndex();
        }
#if UNITY_EDITOR
        [ContextMenu("Test Random Idle")]
        public void CallRandomIdle()
        {
            PlayRandomStandIdle();
        }
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(10, 10, 50, 50), "test"))
        //        PlayRandomStandIdle();
        //}
#endif
    }
}