namespace Alter.Runtime.Inputs
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Alter.Runtime.Character;
    using Alter.Runtime.Hooks;

    [AddComponentMenu("")]
    public class TouchStickManager : MonoBehaviour
    {
        public static TouchStickManager Instance;
        protected const string RESOURCE_PATH = "Alter/Input/TouchStick";

        [SerializeField] protected VirtualJoystickFloating touchStick;

        public bool isActive = false;
        protected void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
            //SceneManager.sceneLoaded += this.OnSceneLoad;
            //this.UpdatePlayerEvents();
        }
        private void OnEnable()
        {
            isActive = true;
            SetActiveJumpUI(AlterPlayerInputs.Instance.CanJump);
        }
        private void OnDisable()
        {
            isActive = false;
        }

        [SerializeField] GameObject jump_Input;
        public void SetActiveJumpUI(bool toActive)
        {
            if (jump_Input)
                jump_Input.SetActive(toActive);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        //private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        //{
        //    bool visible = (PlayerHook.Instance != null);
        //    this.SetVisibility(visible);
        //    this.UpdatePlayerEvents();
        //}

        //private void UpdatePlayerEvents()
        //{
        //    if (PlayerHook.Instance != null)
        //    {
        //        ICharacter player = PlayerHook.Instance.gameObject.GetComponent<ICharacter>();

        //        //player.onIsControllable -= this.OnChangeIsControllable;
        //        //player.onIsControllable += this.OnChangeIsControllable;
        //    }
        //}

        private void OnChangeIsControllable(bool isControllable)
        {
            // this.SetVisibility(isControllable);
        }


        public virtual void SetVisibility(bool visible)
        {
            this.touchStick.gameObject.SetActive(visible);
        }
    }
}