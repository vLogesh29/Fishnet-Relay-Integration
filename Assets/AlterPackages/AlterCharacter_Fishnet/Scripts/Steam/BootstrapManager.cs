using FishNet.Managing;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    public const string HOST_KEY = "HostAddress";
    public const string LOBBY_KEY = "LobbyName";

    private static BootstrapManager Instance;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuScene;
    [SerializeField] private List<SceneAsset> constDontDestroyOnLoadScenes = new();
#endif
    [SerializeField] private string menuSceneName;
    [SerializeField] private List<string> constDontDestroyOnLoadSceneNames;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    private void Awake()
    {
        Instance = this;
#if UNITY_EDITOR
        menuSceneName = menuScene.name;
        constDontDestroyOnLoadSceneNames = constDontDestroyOnLoadScenes.ConvertAll<string>(x => x.name);
#endif
    }

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;

    private string hostName = HOST_KEY;
    private string lobbyName = LOBBY_KEY;
    private HashSet<ulong> lobbyIDs = new();

    public static ulong currentLobbyID;

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyList = Callback<LobbyMatchList_t>.Create(OnInitializeLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
    }

    #region Steamworks Callbacks
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("SetingUp Lobby-Data on lobby-creation with callback-result: " + callback.m_eResult.ToString());

        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Lobby creation was unsuccessful!");
            return;
        }

        // set as host
        currentLobbyID = callback.m_ulSteamIDLobby;
        hostName = SteamUser.GetSteamID().ToString();

        if (lobbyName == null || lobbyName == string.Empty)
            lobbyName = SteamFriends.GetPersonaName().ToString() + "'s lobby";

        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), HOST_KEY, hostName);
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), LOBBY_KEY, lobbyName);

        _fishySteamworks.SetClientAddress(hostName);
        _fishySteamworks.StartConnection(server: true);
        // starting connection by giving the host-client (Client&Server) connection, server-attrb = true
        Debug.Log("Lobby creation was successful!");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // get as client
        currentLobbyID = callback.m_ulSteamIDLobby;
        lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), LOBBY_KEY);
        hostName = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), HOST_KEY);

        MainMenuManager.Instance.LobbyEntered(lobbyName, _networkManager.IsServer);

        _fishySteamworks.SetClientAddress(hostName);
        // starting connection by giving the client (ClientOnly) connection, server-attrb = false
        _fishySteamworks.StartConnection(false);
    }

    private void OnInitializeLobbyList(LobbyMatchList_t lobbymatchList)
    {
        MainMenuManager.Instance.DestroyLobbies();
        for (int i = 0; i < lobbymatchList.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID.m_SteamID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }
    private void OnLobbyDataUpdated(LobbyDataUpdate_t updatedLobby)
    {
        MainMenuManager.Instance.DisplayLobbies(lobbyIDs, updatedLobby);
    }
    #endregion

    #region Static Methods
    public static void CreateLobby(ELobbyType lobbyType, int maxPlayers, string lobbyName = null)
    {
        Instance.lobbyName = lobbyName;
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }

    public static void JoinByID(CSteamID steamID)
    {
        Debug.Log("Attempting to join lobby with ID: " + steamID.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(steamID))
            SteamMatchmaking.JoinLobby(steamID);
        else
            Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);
    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
        currentLobbyID = 0;

        Instance._fishySteamworks.StopConnection(false);
        if (Instance._networkManager.IsServer)
            Instance._fishySteamworks.StopConnection(true);
    }

    public static void GetLobbyList(int maxFetchLobbies = 100,
        ELobbyDistanceFilter lobbyDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide)
    {
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxFetchLobbies);
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(lobbyDistanceFilter);
        SteamMatchmaking.RequestLobbyList();
    }
    #endregion

    #region Public Functions
    public void GoToMenu()
    {
        Debug.Log($"Steamworks intialized, connected to Local-User. Loading MainMenu Scene:\"{menuSceneName}\"...");
        StartCoroutine(WaitInitialize(() => 
        {
            CloseUnwantedScenes();
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
        }));
    }
    #endregion
    
    private IEnumerator WaitInitialize(Action callback)
    {
        if (Instance == null)
            yield return new WaitUntil(() => Instance != null);
        else
            yield return null;
        callback?.Invoke();
    }

    public static void CloseUnwantedScenes(string[] scenesToDontDestroyOnLoad = null, bool removeDuplicates = true)
    {
        if (Instance == null)
        {
            Debug.LogError("BootStrap-Manager Instance not found!");
            return;
        }

        // unity's scene manager used here...
        int noOfActiveScenes = SceneManager.sceneCount;
        HashSet<string> activeScenes = new();

        for (int i = 0; i < noOfActiveScenes; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if ((Instance.constDontDestroyOnLoadSceneNames != null && Instance.constDontDestroyOnLoadSceneNames.Contains(scene.name))
                || (scenesToDontDestroyOnLoad != null && scenesToDontDestroyOnLoad.Contains(scene.name)))
            {
                if (!removeDuplicates || activeScenes.Add(scene.name))
                {
                    Debug.Log($"Scene:\"{scene.name}\" Dont-Destroy-On-Load!");
                    continue;
                }
                else
                {
                    Debug.Log($"Scene:\"{scene.name}\" multiple instances present, unloading duplicates!");
                }
            }
            SceneManager.UnloadSceneAsync(scene);
        }

    }
}