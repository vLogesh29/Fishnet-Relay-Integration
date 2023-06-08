namespace Alter.Runtime.CameraSystem
{
    using Alter.Runtime.Character;
    using Alter.Runtime.Hooks;
    using Cinemachine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class CharacterCameraManager : MonoBehaviour
    {
        public static CharacterCameraManager Instance;
        public bool CanRotate
        {
            get
            {
                return CM_Cameras[currentCameraIndex].CanRotateCharacter;
            }
        }
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            setUpCameraPrefData();
        }

        const string cameraPrefKey = "CameraType";
        int currentCameraIndex = -1;

        ICharacter playerCharacter;
        ICharacter PlayerCharacter()
        {
            if (playerCharacter == null)
            {
                var Player = PlayerHook.Instance;
                if (Player != null)
                {
                    playerCharacter = Player.GetComponent<ICharacter>();
                }
            }
            return playerCharacter;
        }

        CinemachineBrain cinemachineBrain;
        CinemachineBrain CinemachineBrain
        {
            get
            {
                if (cinemachineBrain)
                    return cinemachineBrain;
                else
                {
                    var camera = CameraHook.Instance;
                    if (camera)
                        return camera.GetComponent<CinemachineBrain>();
                    else
                        return null;
                }
            }
        }

        private void Start()
        {
            if (blendSettings != null)
            {
                if (CinemachineBrain)
                    CinemachineBrain.m_CustomBlends = blendSettings;
            }
            //SetUpCameraForPlayer();
        }
        [ContextMenu("Switch Camera View")]
        public void ChangeCameraView()
        {
            var nextIndex = currentCameraIndex + 1;
            if (nextIndex >= CM_Cameras.Count)
                nextIndex = 0;
            ActivateCamera(nextIndex);

            //if (isThirdPerson)
            //    EnableFirstPersonCamera();
            //else
            //    EnableThirdPersonCamera();
        }

        public void SetUpCameraForPlayer(ICharacter character)
        {
            if (character == null)
            {
                Debug.LogError("No Character Found");
                return;
            }
            setupCameraProperties(character.CameraData.CamTarget_TP(), character.CameraData.CamTarget_FP());
            if (IsDollyIntro && dollySpawnIntroCamera)
            {
                ActivateDollyCam(character);
            }
            else
            {
                ActivateCamera(currentCameraIndex);
            }
        }
        public Action OnCameraChange;
        public void SetActiveIntroCamera(bool activate)
        {
            dollySpawnIntroCamera.gameObject.SetActive(activate);
        }
        //public void EnableThirdPersonCamera()
        //{
        //    //PlayerCharacter().MotionData.CanRotate = canRotateOnTPP;
        //    thirdPersonCamera.gameObject.SetActive(true);
        //    firstPersonCamera.gameObject.SetActive(false);
        //    SetActiveIntroCamera(false);
        //    isThirdPerson = true;
        //    saveCameraData(1);

        //    OnCameraChange?.Invoke();
        //}
        public void ActivateCamera(int index)
        {
            Debug.Log("camera Count :: " + CM_Cameras.Count + "  Index :: " + index);
            if (index >= CM_Cameras.Count)
                return;
            Debug.Log("--------------------------");

            var nextCam = CM_Cameras[index].CM_Camera;
            nextCam.gameObject.SetActive(true);
            if (nextCam != CM_Cameras[currentCameraIndex].CM_Camera) CM_Cameras[currentCameraIndex].CM_Camera.gameObject.SetActive(false);
            currentCameraIndex = index;

            SetActiveIntroCamera(false);
            saveCameraData(currentCameraIndex);
            if (blendWaitCor != null)
                StopCoroutine(blendWaitCor);
            blendWaitCor = StartCoroutine("WaitForCinemachineBlend");
        }

        Coroutine blendWaitCor;
        IEnumerator WaitForCinemachineBlend()
        {
            yield return new WaitForEndOfFrame();
            if (CinemachineBrain.IsBlending)        //script and brain attached to same GameObject
                StartCoroutine(WaitForCinemachineBlend());
            else
            {
                OnCameraChange?.Invoke();
            }
        }

        //[SerializeField] CinemachineFreeLook thirdPersonCamera;
        [SerializeField] List<CM_CameraDatas> CM_Cameras;
        //[SerializeField] CinemachineVirtualCamera thirdPersonCamera, firstPersonCamera;
        [SerializeField] bool IsDollyIntro = true;
        [SerializeField] CinemachineVirtualCamera dollySpawnIntroCamera;
        [SerializeField] Transform dollyTrack;
        [SerializeField] CinemachineBlenderSettings blendSettings;
        void setupCameraProperties(Transform targetTP, Transform targetFP)
        {
            CM_Cameras[0].CM_Camera.Follow = targetTP;
            CM_Cameras[0].CM_Camera.LookAt = targetTP;
            CM_Cameras[2].CM_Camera.Follow = targetTP;
            CM_Cameras[2].CM_Camera.LookAt = targetTP;

            CM_Cameras[1].CM_Camera.Follow = targetFP;
            CM_Cameras[1].CM_Camera.LookAt = null;

            dollySpawnIntroCamera.LookAt = targetTP;
        }
        void ActivateDollyCam(ICharacter character)
        {
            dollyTrack.position = character.transform.position;
            dollySpawnIntroCamera.GetComponent<Animator>().enabled = true;
        }
        public void OnDollyCamFinished()
        {
            dollySpawnIntroCamera.GetComponent<Animator>().enabled = false;
            ActivateCamera(currentCameraIndex);
        }
        public void UpdateDollyTrackPosition(Vector3 pos)
        {
            dollyTrack.position = pos;
        }
        void setUpCameraPrefData()
        {
            if (PlayerPrefs.HasKey(cameraPrefKey))
            {
                currentCameraIndex = PlayerPrefs.GetInt(cameraPrefKey);
            }
            else
                currentCameraIndex = 0;
        }

        void saveCameraData(int cameraType)
        {
            PlayerPrefs.SetInt(cameraPrefKey, cameraType);
        }

        [SerializeField] TMP_InputField XInput, YInput;
        /// <summary>
        /// To Test and update the input speed for devices after build
        /// </summary>
        public void UpdateSpeed()
        {
            Vector2 speed = new Vector2(float.Parse(XInput.text), float.Parse(YInput.text));
            foreach (var item in CM_Cameras)
            {
                item.CM_Camera.GetComponent<AlterCinemachineInputFeeder>().UpdateSpeed(speed);
            }
        }
    }
    [Serializable]
    class CM_CameraDatas
    {
        [SerializeField] CinemachineVirtualCamera CM_camera;
        public CinemachineVirtualCamera CM_Camera => CM_camera;
        [SerializeField] bool canRotateCharacter = false;
        public bool CanRotateCharacter => canRotateCharacter;
    }
}